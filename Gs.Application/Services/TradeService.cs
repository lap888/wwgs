using CSRedis;
using Dapper;
using Gs.Core;
using Gs.Core.Extensions;
using Gs.Core.Utils;
using Gs.Domain.Configs;
using Gs.Domain.Context;
using Gs.Domain.Entity;
using Gs.Domain.Entitys;
using Gs.Domain.Enums;
using Gs.Domain.Models;
using Gs.Domain.Models.Dto;
using Gs.Domain.Repository;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gs.Application.Services
{
    public class TradeService : ITradeService
    {
        private readonly string CacheCottonCoinLockKey = "CacheCottonCoinLockKey:";
        private readonly String CottonCoinTableName = "user_account_cotton_coin";
        private readonly String CottonCoinRecordTableName = "user_account_cotton_coin_record";

        private readonly string CacheHonorLockKey = "CacheHonorLockKey:";
        private readonly String HonorTableName = "user_account_honor";
        private readonly String HonorRecordTableName = "user_account_honor_record";

        private readonly HttpClient Client;
        private readonly Models.AppSetting AppSetting;
        private readonly WwgsContext context;
        private readonly CSRedisClient RedisCache;
        private readonly ITicketService TicketSub;
        public TradeService(WwgsContext mySql, ITicketService ticketSub, IHttpClientFactory factory, CSRedisClient cSRedis, IOptionsMonitor<Models.AppSetting> monitor)
        {
            context = mySql;
            RedisCache = cSRedis;
            Client = factory.CreateClient("JPushSMS");
            AppSetting = monitor.CurrentValue;
            TicketSub = ticketSub;
        }

        /// <summary>
        /// 取消交易订单
        /// </summary>
        /// <param name="title"></param>
        /// <param name="orderNum"></param>
        /// <param name="tradePwd"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> CancleTrade(string orderNum, string tradePwd, int userId)
        {
            MyResult<object> result = new MyResult<object>();
            if (userId < 0) { return result.SetStatus(ErrorCode.ErrorSign, "Sign Error"); }
            if (string.IsNullOrEmpty(tradePwd)) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码不能为空"); }
            if (string.IsNullOrEmpty(orderNum)) { return result.SetStatus(ErrorCode.InvalidData, "订单号不能为空"); }
            if (!ProcessSqlStr(orderNum)) { return result.SetStatus(ErrorCode.InvalidData, "非法操作"); }

            //用户信息
            var userInfo = context.Dapper.QueryFirstOrDefault<UserEntity>($"select * from user where id={userId}");
            if (SecurityUtil.MD5(tradePwd) != userInfo.TradePwd) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码有误"); }

            CoinTrade TradeInfo = context.Dapper.QueryFirstOrDefault<CoinTrade>($"select * from coin_trade where id ={orderNum};");
            if (TradeInfo.Status != 1) { return result.SetStatus(ErrorCode.SystemError, "订单状态异常"); }
            if (TradeInfo.BuyerUid == userInfo.Id)
            {
                context.Dapper.Execute($"update coin_trade set status=0 where id={orderNum};");//取消订单
                result.Data = true;
            }
            if (TradeInfo.SellerUid == userInfo.Id)
            {
                var res = await FrozenWalletAmount(CottonCoinTableName, CacheCottonCoinLockKey, null, false, userId, -(decimal)(TradeInfo.Amount + TradeInfo.Fee));
                if (res.Code != 200)
                {
                    return result.SetStatus(ErrorCode.SystemError, res.Message);
                }
                context.Dapper.Execute($"update coin_trade set status=0 where id={orderNum};");//取消订单
            }
            return result;
        }

        /// <summary>
        /// 订单申述
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<object> CreateAppeal(CreateAppealDto model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Sign Error");
            }
            if (string.IsNullOrEmpty(model.Description))
            {
                return result.SetStatus(ErrorCode.InvalidData, "申诉内容不能为空");
            }
            if (string.IsNullOrEmpty(model.PicUrl))
            {
                return result.SetStatus(ErrorCode.InvalidData, "申诉凭据不能为空");
            }
            if (string.IsNullOrEmpty(model.OrderId))
            {
                return result.SetStatus(ErrorCode.InvalidData, "订单号不能为空");
            }
            var orderInfo = context.Dapper.QueryFirstOrDefault<CoinTrade>($"select status from coin_trade where id ={model.OrderId}");
            if (orderInfo == null)
            {
                return result.SetStatus(ErrorCode.SystemError, "订单状态异常");
            }
            //创建申诉记录 更改订单状态 写入消息通知
            if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
            using (IDbTransaction transaction = context.Dapper.BeginTransaction())
            {
                try
                {
                    //记录
                    context.Dapper.Execute($"insert into appeals (orderId, description, picUrl) values ('{model.OrderId}', '{model.Description}', '{model.PicUrl}')", null, transaction);
                    //update
                    context.Dapper.Execute($"update coin_trade set status=5, appealTime = now() where id='{model.OrderId}'", null, transaction);
                    //我的信息记录
                    var msg = $"您发起的{orderInfo.Amount}买单,卖家发起申诉";
                    context.Dapper.Execute($"insert into notice_infos (userId, content, type,title) values ({userId}, '{msg}', '1','申诉通知')", null, transaction);
                    result.Data = new { status = 5 };
                    transaction.Commit();
                }
                catch (System.Exception ex)
                {
                    SystemLog.Debug(ex);
                    transaction.Rollback();
                    return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                }
            }
            context.Dapper.Close();
            return result;
        }

        /// <summary>
        /// 确认出售
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> DealBuy(TradeDto model, int userId)
        {
            MyResult result = new MyResult();
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                if (model == null)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "入参非法");
                }
                if (userId < 0)
                {
                    return result.SetStatus(ErrorCode.ErrorSign, "Sign Error");
                }

                if (string.IsNullOrEmpty(model.TradePwd))
                {
                    return result.SetStatus(ErrorCode.InvalidPassword, "交易密码不能为空");
                }
                if (string.IsNullOrEmpty(model.OrderNum))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "订单号不能为空");
                }
                if (!ProcessSqlStr(model.OrderNum))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "非法操作");
                }
                if (DateTime.Now.Hour >= 21 || DateTime.Now.Hour < 9)
                {
                    return result.SetStatus(ErrorCode.TimeNoOpen, $"交易时间为每天9:00-21:00...");
                }


                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"DealBuy:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                var lastDayForMonth = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
                var startDayForMonth = DateTime.Now.ToString("yyyy-MM-01");

                #region  查询是否被封禁 (买方)
                //查询是否被封禁
                //用户信息
                var userInfo = context.Dapper.QueryFirstOrDefault<UserEntity>($"select * from user where id={userId}");
                if (userInfo == null)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "用户信息不能为空");
                }
                var coinBalance = context.Dapper.QueryFirstOrDefault<decimal>($"select `Balance` from `user_account_cotton_coin` where userId={userId}");

                var honnorBalance = context.Dapper.QueryFirstOrDefault<decimal>($"select `Balance` from `user_account_honor` where userId={userId}");
                var banOrderCount = context.Dapper.QueryFirstOrDefault<int>($"select count(*) as count from coin_trade where buyerBan=1 and buyerUid={userId} and dealTime>'{startDayForMonth}' and dealTime<'{lastDayForMonth}'");
                if (banOrderCount != 0)
                {
                    //最近一次封禁时间
                    var lastBanTime = context.Dapper.QueryFirstOrDefault<CoinTrade>($"select entryOrderTime from coin_trade where buyerBan=1 and buyerUid={userId} and dealTime>'{startDayForMonth}' and dealTime<'{lastDayForMonth}' order by id desc limit 1");
                    if (lastBanTime != null)
                    {
                        DateTime beginTime = DateTime.Now;
                        beginTime = ((DateTime)lastBanTime.EntryOrderTime).AddDays(7);
                        if (DateTime.Now < beginTime)
                        {
                            return result.SetStatus(ErrorCode.Forbidden, $"您当前已经被封禁交易，解封时间为：{beginTime.ToString("yyyy-MM-dd HH:mm")}");
                        }
                    }
                }

                #endregion
                //支付宝账号信息判断
                if (string.IsNullOrWhiteSpace(userInfo.Alipay))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "您没有设置支付宝，买家无法打款给您。请完善信息后进行交易。");
                }
                Regex reg = new Regex(@"[\u4e00-\u9fa5]");
                if (reg.IsMatch(userInfo.Alipay))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "您的支付宝账号异常，请完善后进行交易。");
                }
                //查订单是否存在
                var order = context.Dapper.QueryFirstOrDefault<CoinTrade>($"SELECT `status`, buyerUid, amount, totalPrice, trendSide FROM coin_trade WHERE id = {model.OrderNum};");
                if (order == null) { return result.SetStatus(ErrorCode.SystemError, "此订单已经被别人抢单..."); }
                if (order.Status != 1) { return result.SetStatus(ErrorCode.SystemError, "此订单已经被别人抢单..."); }
                if (!order.TrendSide.Equals("BUY", StringComparison.OrdinalIgnoreCase)) { return result.SetStatus(ErrorCode.SystemError, "订单类型错误"); }

                //计算手续费
                decimal fee = 0;

                #region 计算新的等级
                string UserLevel = userInfo.Level.ToLower();

                #endregion

                #region 新的计算手续费
                if (userInfo.Level.ToLower().Equals("lv0")) { return result.SetStatus(ErrorCode.InvalidData, "LV0禁止交易"); }

                decimal SellRate = 0.01M;
                var settingInfo = AppSetting.Levels.FirstOrDefault(o => o.Level.Equals(userInfo.Level.ToLower()));
                SellRate = settingInfo.SellRate - 1;
                fee = order.Amount.Value * SellRate;
                #endregion

                if (SecurityUtil.MD5(model.TradePwd) != userInfo.TradePwd)
                {
                    return result.SetStatus(ErrorCode.InvalidPassword, "交易密码有误");
                }
                //
                Decimal TotalCandy = fee + order.Amount.Value;

                if (coinBalance < TotalCandy)
                {
                    return result.SetStatus(ErrorCode.InvalidData, $"您当前MBM为:{coinBalance},不足以出售{model.Amount}个MBM");
                }
                if (honnorBalance < (order.Amount.Value * 2))
                {
                    return result.SetStatus(ErrorCode.InvalidData, $"您当前荣誉值为:{honnorBalance},不足以出售{model.Amount}个MBM");
                }
                // if (userInfo.AuditState != 2)
                // {
                //     return result.SetStatus(ErrorCode.NoAuth, "请去新视讯实名...");
                // }
                if (string.IsNullOrEmpty(userInfo.Alipay))
                {
                    return result.SetStatus(ErrorCode.NoAuth, "交易前请设置支付宝账号...");
                }
                if (userInfo.Status == 2 || userInfo.Status == 3 || userInfo.Status == 4 || userInfo.Status == 5)
                {
                    return result.SetStatus(ErrorCode.AccountDisabled, "账号已被封禁 请联系管理员");
                }

                #region 今日 是否存在未完成的订单
                StringBuilder QueryProgressOrderSql = new StringBuilder();
                QueryProgressOrderSql.Append($"SELECT count(id) AS count FROM coin_trade WHERE ( `sellerUid` = @UserId OR `buyerUid` = @UserId) AND `status` IN ( 1, 2, 3, 5 );");
                var isHasOrder = context.Dapper.QueryFirstOrDefault<int>(QueryProgressOrderSql.ToString(), new { UserId = userId });
                if (isHasOrder > 0)
                {
                    return result.SetStatus(ErrorCode.HasValued, "您今天还有订单未完成，请先完成当前订单再交易");
                }
                #endregion

                var orderCount = context.Dapper.QueryFirstOrDefault<int>($"select count(*) as count from coin_trade where sellerUid={userId} and status=4 and to_days(dealTime)=to_days(now())");

                if (orderCount >= 3)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "出售次数受限，每日只可交易三次");
                }
                var buyerMobile = context.Dapper.QueryFirstOrDefault<string>($"select mobile from user where id={order.BuyerUid}");

                //买家和卖家不能相同
                if (order.BuyerUid == userId)
                {
                    return result.SetStatus(ErrorCode.SystemError, "自己无法卖给自己!");
                }
                //冻结 更新用户余额 发记录 短信
                if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
                using (IDbTransaction transaction = context.Dapper.BeginTransaction())
                {
                    try
                    {
                        //fabi卖方 冻结其账户-NW
                        var res1 = await FrozenWalletAmount(CottonCoinTableName, CacheCottonCoinLockKey, transaction, true, userId, TotalCandy);
                        if (res1.Code != 200)
                        {
                            return result.SetStatus(ErrorCode.SystemError, res1.Message);
                        }
                        var res2 = await FrozenWalletAmount(HonorTableName, CacheHonorLockKey, transaction, true, userId, (order.Amount.Value * 2));

                        if (res2.Code != 200)
                        {
                            return result.SetStatus(ErrorCode.SystemError, res2.Message);
                        }

                        context.Dapper.Execute($"update coin_trade set sellerUid = {userId},sellerAlipay='{userInfo.Alipay}',fee = {fee},dealTime='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',dealEndTime='{DateTime.Now.AddMinutes(60).ToString("yyyy-MM-dd HH:mm:ss")}', status = 2 where id = {model.OrderNum}", null, transaction);

                        //我的信息记录
                        var msg = $"你发布的{Math.Round((decimal)order.Amount.Value, 4)}买单已被接单，请到“订单”-“交易中” 查看卖家支付宝，并按买单中显示的金额付款，上传付款截图";
                        context.Dapper.Execute($"insert into notice_infos (userId, content, type,title) values ({order.BuyerUid}, '{msg}', '1','买单通知')", null, transaction);
                        transaction.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        transaction.Rollback();
                        SystemLog.Debug("确认出售", ex);
                        return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                    }
                }

                context.Dapper.Close();
                //短信通知
                await CommonSendToBuyer(buyerMobile);
                result.Data = true;
            }
            catch (System.Exception ex)
            {
                SystemLog.Debug($"{userId}==>{model.GetJson()}", ex);
                return result.SetStatus(ErrorCode.InvalidData, "入参非法[L]");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
            return result;
        }


        /// <summary>
        /// 通知买方 CommonSendToBuyer
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<MsgDto>> CommonSendToBuyer(string mobile)
        {
            MyResult<MsgDto> result = new MyResult<MsgDto>();

            StringContent content = new StringContent(new { mobile = mobile, temp_id = "198016" }.GetJson());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await this.Client.PostAsync("https://api.sms.jpush.cn/v1/messages", content);
            String res = await response.Content.ReadAsStringAsync();
            result.Data = res.GetModel<MsgDto>();
            if (result.Data != null && !string.IsNullOrEmpty(result.Data.Msg_Id))
            {
                #region 写入数据库
                StringBuilder InsertSql = new StringBuilder();
                DynamicParameters Param = new DynamicParameters();
                InsertSql.Append("INSERT INTO `user_vcodes`(`mobile`, `msgId`, `createdAt`) VALUES (@Mobile, @MsgId , NOW());");
                Param.Add("Mobile", mobile, DbType.String);
                Param.Add("MsgId", result.Data.Msg_Id, DbType.String);
                await context.Dapper.ExecuteAsync(InsertSql.ToString(), Param);
                #endregion
            }
            return result;
        }

        /// <summary>
        /// 通知卖方 CommonSendToSeller
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<MsgDto>> CommonSendToSeller(string mobile)
        {
            MyResult<MsgDto> result = new MyResult<MsgDto>();

            StringContent content = new StringContent(new { mobile = mobile, temp_id = "198018" }.GetJson());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await this.Client.PostAsync("https://api.sms.jpush.cn/v1/messages", content);
            String res = await response.Content.ReadAsStringAsync();
            result.Data = res.GetModel<MsgDto>();
            if (result.Data != null && !string.IsNullOrEmpty(result.Data.Msg_Id))
            {
                #region 写入数据库
                StringBuilder InsertSql = new StringBuilder();
                DynamicParameters Param = new DynamicParameters();
                InsertSql.Append("INSERT INTO `user_vcodes`(`mobile`, `msgId`, `createdAt`) VALUES (@Mobile, @MsgId , NOW());");
                Param.Add("Mobile", mobile, DbType.String);
                Param.Add("MsgId", result.Data.Msg_Id, DbType.String);
                await context.Dapper.ExecuteAsync(InsertSql.ToString(), Param);
                #endregion
            }
            return result;
        }

        /// <summary>
        /// 控制面板
        /// </summary>
        /// <returns></returns>
        public MyResult<CoinTradeExt> GetTradeTotal(int userId)
        {
            MyResult<CoinTradeExt> result = new MyResult<CoinTradeExt>();

            String TradeTotalCoinKey = $"System:TradeTotalCoin";
            result.Data = RedisCache.Get<CoinTradeExt>(TradeTotalCoinKey);
            if (result.Data == null)
            {
                #region 计算面板信息
                StringBuilder QueryCoinSql = new StringBuilder();
                QueryCoinSql.Append("SELECT SUM( `amount` ) AS todayTradeAmount, AVG( `price` ) AS todayAvgPrice, MAX( `price` ) AS todayMaxPrice ");
                QueryCoinSql.Append($"FROM `coin_trade` WHERE `status` = 4 AND DATE( `dealTime` ) = DATE(NOW());");
                CoinTradeExt coinTrade = context.Dapper.QueryFirstOrDefault<CoinTradeExt>(QueryCoinSql.ToString());
                //当前可用MBM
                var canUserCottonCoin = context.Dapper.QueryFirstOrDefault<decimal>($"SELECT IFNULL(SUM(Balance),0) Balance from user_account_cotton_coin where UserId={userId}");
                //当前可用MBM
                var canUserCotton = context.Dapper.QueryFirstOrDefault<decimal>($"SELECT IFNULL(SUM(Balance),0) Balance from user_account_cotton where UserId={userId}");

                var orderInfo1 = context.Dapper.QueryFirstOrDefault<CoinTradeExt>($"SELECT IFNULL(SUM(amount),0) historyFinishCount,COUNT(id) historyFinishOrderCount FROM coin_trade where `status`=4 and coinType!='RZQ'");
                //
                var orderInfo2 = context.Dapper.QueryFirstOrDefault<CoinTradeExt>($"SELECT IFNULL(SUM(amount),0) todayBuyCount,COUNT(id) todayBuyOrderCount FROM coin_trade where `status`= 1 and coinType!='RZQ'");

                //查询今天是否有记录
                Int32 isHasRecord = context.Dapper.QueryFirstOrDefault<Int32>($"SELECT id FROM coin_trade_ext WHERE DATE(ctime) = DATE(NOW());");
                if (isHasRecord != 0)
                {
                    StringBuilder UpdateCoinSql = new StringBuilder();
                    UpdateCoinSql.Append("UPDATE `coin_trade_ext` SET ");
                    UpdateCoinSql.Append("`todayTradeAmount` = @TodayTradeAmount, ");
                    UpdateCoinSql.Append("`todayMaxPrice` = @TodayMaxPrice, ");
                    UpdateCoinSql.Append("`todayAvgPrice` = @TodayAvgPrice, ");
                    UpdateCoinSql.Append("`todayAmount` = @BuyCount, ");
                    UpdateCoinSql.Append("`upRate` = @UpRate WHERE DATE(ctime) = DATE(NOW())");

                    DynamicParameters UpdateCoinParam = new DynamicParameters();
                    UpdateCoinParam.Add("TodayTradeAmount", orderInfo1.HistoryFinishCount, DbType.Decimal);
                    UpdateCoinParam.Add("TodayMaxPrice", 0, DbType.Decimal);
                    UpdateCoinParam.Add("TodayAvgPrice", 0, DbType.Decimal);
                    UpdateCoinParam.Add("BuyCount", orderInfo2.TodayBuyCount, DbType.Int32);
                    UpdateCoinParam.Add("UpRate", 0.001M, DbType.Decimal);

                    context.Dapper.Execute(UpdateCoinSql.ToString(), UpdateCoinParam);
                }
                else
                {
                    StringBuilder InsertCoinSql = new StringBuilder();
                    InsertCoinSql.Append("INSERT INTO `coin_trade_ext` ( `todayTradeAmount`, `todayAvgPrice`, `todayAmount`, `todayMaxPrice` ) ");
                    InsertCoinSql.Append("VALUES (@TodayTradeAmount, @TodayAvgPrice, @BuyCount, @TodayMaxPrice);");

                    DynamicParameters InsertCoinParam = new DynamicParameters();
                    InsertCoinParam.Add("TodayTradeAmount", 0, DbType.Decimal);
                    InsertCoinParam.Add("TodayAvgPrice", 0, DbType.Decimal);
                    InsertCoinParam.Add("TodayMaxPrice", 0, DbType.Decimal);
                    InsertCoinParam.Add("BuyCount", orderInfo2.TodayBuyCount, DbType.Int32);
                    context.Dapper.Execute(InsertCoinSql.ToString(), InsertCoinParam);
                }
                #endregion

                #region 获取面板信息
                List<CoinTradeExt> tradeCoins = context.Dapper.Query<CoinTradeExt>($"SELECT * FROM `coin_trade_ext` ORDER BY id DESC LIMIT 1;").ToList();
                CoinTradeExt coinTradeExt = new CoinTradeExt();

                if (tradeCoins.Count == 1)
                {
                    coinTradeExt = new CoinTradeExt
                    {
                        SysMaxPrice = tradeCoins[0].SysMaxPrice,
                        SysMinPrice = tradeCoins[0].SysMinPrice,
                        CanUserCottonCoin = canUserCottonCoin,
                        CanUserCotton = canUserCotton,
                        HistoryFinishCount = orderInfo1.HistoryFinishCount,
                        HistoryFinishOrderCount = orderInfo1.HistoryFinishOrderCount,
                        TodayBuyCount = orderInfo2.TodayBuyCount,
                        TodayBuyOrderCount = orderInfo2.TodayBuyOrderCount,
                        UpRate = tradeCoins[0].UpRate
                    };
                }
                #endregion
                RedisCache.Set(TradeTotalCoinKey, coinTradeExt, 60);
                result.Data = coinTradeExt;
            }
            return result;
        }

        /// <summary>
        /// 我的交易订单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<List<TradeListReturnDto>>> MyTradeList(MyTradeListDto model, int userId)
        {
            MyResult<List<TradeListReturnDto>> result = new MyResult<List<TradeListReturnDto>>();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            model.PageIndex = model.PageIndex < 1 ? 1 : model.PageIndex;
            #region 拼接SQL 查询条件
            StringBuilder SqlStr = new StringBuilder();
            SqlStr.Append("FROM ");
            SqlStr.Append("`coin_trade` AS ct ");
            SqlStr.Append("LEFT JOIN `user` AS s ON ct.`sellerUid` = s.id ");
            SqlStr.Append("LEFT JOIN `user` AS b ON ct.`buyerUid` = b.id ");
            SqlStr.Append("LEFT JOIN `authentication_infos` ais ON ct.`sellerUid` = ais.`userId` ");
            SqlStr.Append("LEFT JOIN `authentication_infos` aib ON ct.`buyerUid` = aib.`userId` ");
            SqlStr.Append($"WHERE 1 = 1 ");

            DynamicParameters QueryParam = new DynamicParameters();
            QueryParam.Add("CosUrl", Constants.CosUrl, DbType.String);

            if (model.CoinType == "RZQ")
            {
                QueryParam.Add("CoinType", model.CoinType, DbType.String);
                SqlStr.Append("AND ct.coinType=@CoinType ");
            }
            else
            {
                QueryParam.Add("CoinType", "USDT", DbType.String);
                SqlStr.Append("AND ct.coinType=@CoinType ");
            }
            if (String.IsNullOrWhiteSpace(model.Sale) || model.Sale.Equals("BUY", StringComparison.OrdinalIgnoreCase))
            {
                if (model.Status == 1)
                {
                    QueryParam.Add("Sale", "BUY", DbType.String);
                    SqlStr.Append("AND ct.trendSide=@Sale ");

                    QueryParam.Add("UserId", userId, DbType.Int32);
                    SqlStr.Append("AND ct.buyerUid=@UserId AND ct.status=1 ");
                }
                else if (model.Status == 2)
                {
                    QueryParam.Add("UserId", userId, DbType.Int32);
                    SqlStr.Append("AND (ct.buyerUid=@UserId OR ct.sellerUid=@UserId) AND ct.status IN (2,3,5) ");
                }
                else if (model.Status == 3)
                {
                    QueryParam.Add("UserId", userId, DbType.Int32);
                    SqlStr.Append("AND (ct.buyerUid=@UserId OR ct.sellerUid=@UserId) AND ct.status=4 ");
                }
            }
            else
            {
                if (model.Status == 1)
                {
                    QueryParam.Add("Sale", "SELL", DbType.String);
                    SqlStr.Append("AND ct.trendSide=@Sale ");
                    QueryParam.Add("UserId", userId, DbType.Int32);
                    SqlStr.Append("AND ct.sellerUid=@UserId AND ct.status=1 ");
                }
                else if (model.Status == 2)
                {
                    QueryParam.Add("UserId", userId, DbType.Int32);
                    SqlStr.Append("AND (ct.sellerUid=@UserId OR ct.sellerUid=@UserId) AND ct.status IN (2,3,5) ");
                }
                else if (model.Status == 3)
                {
                    QueryParam.Add("UserId", userId, DbType.Int32);
                    SqlStr.Append("AND (ct.sellerUid=@UserId OR ct.sellerUid=@UserId) AND ct.status=4 ");
                }
            }
            SqlStr.Append("ORDER BY ct.utime DESC ");
            #endregion

            #region 查询总数
            StringBuilder CountSqlStr = new StringBuilder();
            CountSqlStr.Append("SELECT COUNT(ct.`id`) ");
            CountSqlStr.Append(SqlStr);
            CountSqlStr.Append(";");
            #endregion

            #region 拼接查询字段
            StringBuilder QuerySqlStr = new StringBuilder();
            QuerySqlStr.Append("SELECT ct.`id` AS `Id`, ");
            QuerySqlStr.Append("ais.`trueName` AS `SellerTrueName`, ");
            QuerySqlStr.Append("aib.`trueName` AS `BuyerTrueName`, ");
            QuerySqlStr.Append("ct.`tradeNumber` AS `TradeNumber`, ");
            QuerySqlStr.Append("ct.`buyerUid` AS `BuyerUid`, ");
            QuerySqlStr.Append("b.`mobile` AS `BuyerMobile`, ");
            QuerySqlStr.Append("CONCAT(@CosUrl, b.`avatarUrl`) AS `BuyerAvatarUrl`, ");
            QuerySqlStr.Append("ct.`buyerAlipay` AS `BuyerAlipay`, ");
            QuerySqlStr.Append("ct.`sellerUid` AS `SellerUid`, ");
            QuerySqlStr.Append("s.`mobile` AS `SellerMobile`, ");
            QuerySqlStr.Append("CONCAT(@CosUrl, s.`avatarUrl`) AS `SellerAvatarUrl`, ");
            QuerySqlStr.Append("ct.`sellerAlipay` AS `SellerAlipay`, ");
            QuerySqlStr.Append("CONCAT(@CosUrl, ct.`pictureUrl`) AS `PictureUrl`, ");
            QuerySqlStr.Append("ct.`amount` AS `Amount`, ");
            QuerySqlStr.Append("ct.`price` AS `Price`, ");
            QuerySqlStr.Append("ct.`totalPrice` AS `TotalPrice`, ");
            QuerySqlStr.Append("ct.`dealTime` AS `DealTime`, ");
            QuerySqlStr.Append("ct.`dealEndTime` AS `DealEndTime`, ");
            QuerySqlStr.Append("ct.`paidTime` AS `PaidTime`, ");
            QuerySqlStr.Append("ct.`paidEndTime` AS `PaidEndTime`, ");
            QuerySqlStr.Append("ct.`status` AS `Status` ");
            #endregion

            QuerySqlStr.Append(SqlStr);
            QuerySqlStr.Append("LIMIT @PageIndex, @PageSize;");
            QueryParam.Add("PageIndex", (model.PageIndex - 1) * model.PageSize, DbType.Int32);
            QueryParam.Add("PageSize", model.PageSize, DbType.Int32);

            try
            {
                IEnumerable<TradeListReturnDto> coinTradeInfo = await context.Dapper.QueryAsync<TradeListReturnDto>(QuerySqlStr.ToString(), QueryParam);

                result.Data = new List<TradeListReturnDto>();
                foreach (var item in coinTradeInfo)
                {
                    TradeListReturnDto tradeInfo = item;
                    if (!String.IsNullOrWhiteSpace(tradeInfo.BuyerMobile))
                    {
                        tradeInfo.BuyerMobile = Regex.Replace(item.BuyerMobile, "(\\d{3})\\d{4}(\\d{4})", "$1****$2");
                    }
                    if (!String.IsNullOrWhiteSpace(item.SellerMobile))
                    {
                        tradeInfo.SellerMobile = Regex.Replace(item.SellerMobile, "(\\d{3})\\d{4}(\\d{4})", "$1****$2");
                    }
                    result.Data.Add(tradeInfo);
                }
                result.RecordCount = await context.Dapper.QueryFirstOrDefaultAsync<Int32>(CountSqlStr.ToString(), QueryParam);
                result.PageCount = result.RecordCount / model.PageSize;
            }
            catch (Exception ex)
            {
                result.RecordCount = 0;
                result.PageCount = 0;
                result.Data = new List<TradeListReturnDto>();
                SystemLog.Debug(ex);
            }
            return result;
        }

        /// <summary>
        /// 确认支付
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> Paid(PaidDto model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            if (string.IsNullOrEmpty(model.OrderNum))
            {
                return result.SetStatus(ErrorCode.InvalidData, "订单号不能为空");
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"Paid:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//
                //查订单是否存在
                var orderInfo = context.Dapper.QueryFirstOrDefault<CoinTrade>($"select status,sellerUid from coin_trade where id ={model.OrderNum} and status=2");
                if (orderInfo == null)
                {
                    return result.SetStatus(ErrorCode.SystemError, "订单状态异常");
                }
                if (orderInfo.Status != 2)
                {
                    return result.SetStatus(ErrorCode.SystemError, "订单状态异常");
                }
                //卖方手机号
                var sellerMobile = context.Dapper.QueryFirstOrDefault<string>($"select mobile from user where id={orderInfo.SellerUid}");
                //更新订单状态 写记录 发短信
                if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
                using (IDbTransaction transaction = context.Dapper.BeginTransaction())
                {
                    try
                    {
                        //卖方消息通知
                        var msg = $"买家已标记付款，请到 订单-交易中 验证交易详情，确认支付宝是否到账，无误后请尽快释放";
                        var sql = $"insert into notice_infos (userId, content, type,title) values ({orderInfo.SellerUid}, '{msg}', '1','卖通知')";
                        context.Dapper.Execute(sql, null, transaction);
                        var paidTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        var paidEndTime = DateTime.Now.AddMinutes(60).ToString("yyyy-MM-dd HH:mm:ss");
                        //更新订单信息
                        context.Dapper.Execute($"update coin_trade set pictureUrl='{model.PicUrl}',paidTime='{paidTime}',paidEndTime='{paidEndTime}',status = 3 where id = {model.OrderNum}", null, transaction);
                        transaction.Commit();
                        result.Data = new { PaidTime = paidTime, PaidEndTime = paidEndTime };
                    }
                    catch (System.Exception ex)
                    {
                        SystemLog.Debug("Paid订单支付发生错误", ex);
                        transaction.Rollback();
                        return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                    }
                }
                //如果是认证券不发送短信
                //短信通知
                await CommonSendToSeller(sellerMobile);

                return result;
            }
            catch (Exception ex)
            {
                SystemLog.Debug("Paid订单支付发生错误", ex);
                return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }


        /// <summary>
        /// 确认到账 发放
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> PaidCoin(PaidDto model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            if (string.IsNullOrEmpty(model.OrderNum))
            {
                return result.SetStatus(ErrorCode.InvalidData, "订单号不能为空");
            }
            if (!ProcessSqlStr(model.OrderNum))
            {
                return result.SetStatus(ErrorCode.InvalidData, "非法操作");
            }
            if (string.IsNullOrEmpty(model.TradePwd))
            {
                return result.SetStatus(ErrorCode.InvalidPassword, "交易密码不能为空");
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"PaidCoin:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//
                //用户信息
                var userInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserEntity>($"select * from user where id={userId}");
                if (userInfo == null)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "用户信息不能为空...");
                }
                if (SecurityUtil.MD5(model.TradePwd) != userInfo.TradePwd)
                {
                    return result.SetStatus(ErrorCode.InvalidPassword, "交易密码有误");
                }
                //订单信息
                var orderInfo = context.Dapper.QueryFirstOrDefault<CoinTrade>($"SELECT `status`, amount, fee, buyerUid, sellerUid, trendSide,coinType FROM coin_trade WHERE id = {model.OrderNum} AND `status` =3;");
                if (orderInfo == null)
                {
                    return result.SetStatus(ErrorCode.SystemError, "订单状态异常 请联系管理员");
                }
                if (orderInfo.Status != 3)
                {
                    return result.SetStatus(ErrorCode.SystemError, "订单状态异常");
                }


                #region 计算正常手续费
                decimal NormalFee = 0;

                NormalFee = orderInfo.Amount.Value * 0.01M;
                #endregion

                if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
                if (orderInfo.CoinType == "RZQ")
                {
                    context.Dapper.Execute($"update coin_trade set status=4 where id = {model.OrderNum}");
                    //发放认证券
                    var rult = await TicketSub.ChangeAmount((long)orderInfo.BuyerUid, (decimal)orderInfo.Amount, TicketModifyType.TICKET_SUBSCRIBE, false, orderInfo.Amount.ToString());
                    if (rult == null || !rult.Success) { return rult; }
                    return result;
                }
                using (IDbTransaction transaction = context.Dapper.BeginTransaction())
                {
                    try
                    {
                        var systemUserId = 1;
                        var TotalCandy = orderInfo.Amount + orderInfo.Fee;

                        //减掉卖家用户的冻结账户中的冻结余额并添加流水
                        var res1 = await ChangeWalletAmount(CacheCottonCoinLockKey, CottonCoinTableName, CottonCoinRecordTableName, transaction, true, userId, -(decimal)orderInfo.Amount.Value, (int)ConchModifyType.Coin_Sell_Coin, true, Math.Round((decimal)orderInfo.Amount, 4).ToString());
                        if (res1.Code != 200)
                        {
                            return result.SetStatus(ErrorCode.SystemError, res1.Message);
                        }

                        //增加买家账户中的余额并添加流水
                        var res2 = await ChangeWalletAmount(CacheCottonCoinLockKey, CottonCoinTableName, CottonCoinRecordTableName, transaction, true, (long)orderInfo.BuyerUid, (decimal)orderInfo.Amount.Value, (int)ConchModifyType.Coin_buy_Coin, false, Math.Round((decimal)orderInfo.Amount, 4).ToString());
                        if (res2.Code != 200)
                        {
                            return result.SetStatus(ErrorCode.SystemError, res2.Message);
                        }

                        //荣誉值 减掉卖家用户的冻结账户中的冻结余额并添加流水
                        var res4 = await ChangeWalletAmount(CacheHonorLockKey, HonorTableName, HonorRecordTableName, transaction, true, userId, -(decimal)orderInfo.Fee.Value, (int)HonorModifyType.Seller_Sub_Honor, true, Math.Round((decimal)orderInfo.Amount, 4).ToString(), Math.Round((decimal)orderInfo.Fee.Value, 4).ToString());
                        if (res4.Code != 200)
                        {
                            return result.SetStatus(ErrorCode.SystemError, res4.Message);
                        }

                        // //荣誉值 增加买家账户中的余额并添加流水
                        // var res5 = await ChangeWalletAmount(CacheHonorLockKey, HonorTableName, HonorRecordTableName, transaction, true, (long)orderInfo.BuyerUid, (decimal)(orderInfo.Amount.Value * 4), (int)HonorModifyType.Buy_Reward_Honor, false, Math.Round((decimal)orderInfo.Amount, 4).ToString(), Math.Round((decimal)(orderInfo.Amount.Value * 4), 4).ToString());
                        // if (res5.Code != 200)
                        // {
                        //     return result.SetStatus(ErrorCode.SystemError, res5.Message);
                        // }

                        //将手续费划入系统
                        var res3 = await ChangeWalletAmount(CacheCottonCoinLockKey, CottonCoinTableName, CottonCoinRecordTableName, transaction, true, systemUserId, (decimal)orderInfo.Fee.Value, (int)ConchModifyType.Coin_Sell_Sys_Fee, false, orderInfo.TradeNumber, orderInfo.Fee.ToString());
                        if (res3.Code != 200)
                        {
                            return result.SetStatus(ErrorCode.SystemError, res3.Message);
                        }
                        //更新订单信息
                        context.Dapper.Execute($"update coin_trade set status=4 where id = {model.OrderNum}", null, transaction);
                        transaction.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        SystemLog.Debug(ex);
                        transaction.Rollback();
                        return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                    }
                }
                context.Dapper.Close();
                result.Data = true;
                return result;
            }
            catch (Exception)
            {
                return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }

        /// <summary>
        /// 发布买单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> StartBuy(TradeDto model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Sign Error");
            }
            if (model == null)
            {
                return result.SetStatus(ErrorCode.InvalidData, "数据类型非法...");
            }
            if (string.IsNullOrEmpty(model.TradePwd))
            {
                return result.SetStatus(ErrorCode.InvalidPassword, "交易密码不能为空");
            }
            if (model.Amount <= 0)
            {
                return result.SetStatus(ErrorCode.InvalidData, "数量不能小于0");
            }
            if (model.Price <= 0)
            {
                return result.SetStatus(ErrorCode.InvalidData, "价格不能低于0");
            }

            model.Price = Math.Round(model.Price, 4);
            #region 系统单价判定

            StringBuilder QueryPriceSql = new StringBuilder();
            QueryPriceSql.Append($"SELECT sysMinPrice, sysMaxPrice FROM coin_trade_ext ORDER BY id DESC");
            CoinTradeExt PriceLimit = await context.Dapper.QueryFirstOrDefaultAsync<CoinTradeExt>(QueryPriceSql.ToString());
            if (model.Price < PriceLimit.SysMinPrice.ToDouble())
            {
                return result.SetStatus(ErrorCode.InvalidData, "价格不能低于" + PriceLimit.SysMinPrice.ToString("0.00"));
            }
            if (model.Price > PriceLimit.SysMaxPrice.ToDouble())
            {
                return result.SetStatus(ErrorCode.InvalidData, "价格不能高于" + PriceLimit.SysMaxPrice.ToString("0.00"));
            }
            #endregion

            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"StartBuy:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                #region 查询是否交易封禁
                var lastDayForMonth = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
                var startDayForMonth = DateTime.Now.ToString("yyyy-MM-01");
                //查询是否被封禁
                var selectCoinSql = $"select count(*) as count from coin_trade where buyerBan=1 and buyerUid = { userId } and dealTime> '{startDayForMonth}' and dealTime<'{lastDayForMonth}'";
                var banOrderCount = context.Dapper.QueryFirstOrDefault<int>(selectCoinSql);
                UserEntity userInfo = context.Dapper.QueryFirstOrDefault<UserEntity>($"select * from user where id={userId}");
                if (banOrderCount != 0)
                {
                    //最近一次封禁时间
                    var lastBanTime = context.Dapper.QueryFirstOrDefault<CoinTrade>($"select entryOrderTime from coin_trade where buyerBan=1 and buyerUid={userId} and dealTime>'{startDayForMonth}' and dealTime<'{lastDayForMonth}' order by id desc limit 1");
                    if (lastBanTime != null)
                    {
                        DateTime beginTime = DateTime.Now;
                        beginTime = ((DateTime)lastBanTime.EntryOrderTime).AddDays(1);
                        if (DateTime.Now < beginTime)
                        {
                            return result.SetStatus(ErrorCode.Forbidden, $"您当前已经被封禁交易，解封时间为：{beginTime.ToString("yyyy-MM-dd HH:mm")}");
                        }
                    }
                }
                #endregion

                #region 会员信息验证
                //查询钱包余额
                var coinBalance = context.Dapper.QueryFirstOrDefault<decimal>($"select `Balance` from `user_account_wallet` where userId={userId}");
                if (userInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "用户信息不存在..."); }
                if (SecurityUtil.MD5(model.TradePwd) != userInfo.TradePwd) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码有误"); }
                // if (userInfo.AuditState != 2) { return result.SetStatus(ErrorCode.NoAuth, "请去新视讯实名认证..."); }
                if (string.IsNullOrEmpty(userInfo.Alipay))
                {
                    return result.SetStatus(ErrorCode.NoAuth, "交易前请设置支付宝账号...");
                }
                if (userInfo.Status == 2 || userInfo.Status == 3 || userInfo.Status == 5) { return result.SetStatus(ErrorCode.AccountDisabled, "账号异常 请联系管理员"); }

                #endregion

                #region 交易数量限制
                Int32 coinCount = context.Dapper.QueryFirstOrDefault<Int32>($"SELECT COUNT(*) AS count FROM coin_trade WHERE buyerUid = @UserId AND `status` != 0;", new { UserId = userId });
                coinCount += context.Dapper.QueryFirstOrDefault<Int32>($"SELECT COUNT(*) AS count FROM coin_trade WHERE sellerUid = @UserId AND `status` != 0;", new { UserId = userId });
                if (coinCount == 0 && model.Amount > 5) { return result.SetStatus(ErrorCode.InvalidData, "首次交易订单数量不得大于5"); }
                if (model.Amount > 500) { return result.SetStatus(ErrorCode.InvalidData, "交易订单数量不得大于500"); }

                StringBuilder QueryIngSql = new StringBuilder();
                QueryIngSql.Append($"SELECT COUNT(*) AS count FROM coin_trade WHERE buyerUid = @UserId AND trendSide = 'BUY' AND `status` != 0 AND `status` != 4 AND `status` != 6 ");
                QueryIngSql.Append($"UNION SELECT COUNT(*) AS count FROM coin_trade WHERE sellerUid = @UserId AND trendSide = 'BUY' AND `status` != 0 AND `status` != 4 AND `status` != 6;");

                // var orderIng = context.Dapper.QueryFirstOrDefault<int>(QueryIngSql.ToString(), new { UserId = userId });
                // if (orderIng >= 1) { return result.SetStatus(ErrorCode.InvalidData, "当前有未完成订单"); }
                #endregion
                //订单信息
                var orderNum = Gen.NewGuid20();
                var totalPrice = model.Amount * model.Price;

                //发布订单
                var insertSql = $"insert into coin_trade(tradeNumber,buyerUid,buyerAlipay,amount,price,totalPrice,fee,trendSide,status)values('{orderNum}',{userId},'{userInfo.Alipay}',{model.Amount},{model.Price},{totalPrice},0,'BUY',1);SELECT @@IDENTITY";
                var res = context.Dapper.ExecuteScalar<long>(insertSql);
                if (res == 0)
                {
                    return result.SetStatus(ErrorCode.SystemError, "发布订单失败");
                }

                result.Data = true;
                return result;
            }
            catch (Exception ex)
            {
                SystemLog.Debug(ex);
                return result.SetStatus(ErrorCode.SystemError, "发布订单失败");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }

        #region 账务操作核心
        /// <summary>
        /// Coin钱包账户余额变更 common
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <param name="useFrozen">使用冻结金额，账户金额增加时，此参数无效</param>
        /// <param name="modifyType">账户变更类型</param>
        /// <param name="Desc">描述</param>
        /// <returns></returns>
        public async Task<MyResult<object>> ChangeWalletAmount(string CacheLockKey, string AccountTableName, string RecordTableName, IDbTransaction OutTran, bool isUserOutTransaction, long userId, decimal Amount, int modifyType, bool useFrozen, params string[] Desc)
        {
            MyResult result = new MyResult { Data = false };
            if (Amount == 0) { return new MyResult { Data = true }; }   //账户无变动，直接返回成功
            if (Amount > 0 && useFrozen) { useFrozen = false; } //账户增加时，无法使用冻结金额
            CSRedisClientLock CacheLock = null;
            AccountInfo UserAccount;
            Int64 AccountId;
            String Field = String.Empty, EditSQl = String.Empty, RecordSql = String.Empty, PostChangeSql = String.Empty;
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}Init_{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }

                #region 验证账户信息
                String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {userId} LIMIT 1";
                if (isUserOutTransaction)
                {
                    UserAccount = await context.Dapper.QueryFirstOrDefaultAsync<AccountInfo>(SelectSql, null, OutTran);
                }
                else
                {
                    UserAccount = await context.Dapper.QueryFirstOrDefaultAsync<AccountInfo>(SelectSql);
                }
                if (UserAccount == null) { return result.SetStatus(ErrorCode.InvalidData, "账户不存在"); }
                if (Amount < 0)
                {
                    if (useFrozen)
                    {
                        if (UserAccount.Frozen < Math.Abs(Amount) || UserAccount.Balance < Math.Abs(Amount)) { return result.SetStatus(ErrorCode.InvalidData, "账户余额不足[F]"); }
                    }
                    else
                    {
                        if ((UserAccount.Balance - UserAccount.Frozen) < Math.Abs(Amount)) { return result.SetStatus(ErrorCode.InvalidData, "账户余额不足[B]"); }
                    }
                }
                #endregion

                AccountId = UserAccount.AccountId;
                Field = Amount > 0 ? "Revenue" : "Expenses";

                EditSQl = $"UPDATE `{AccountTableName}` SET `Balance`=`Balance`+{Amount},{(useFrozen ? $"`Frozen`=`Frozen`+{Amount}," : "")}`{Field}`=`{Field}`+{Math.Abs(Amount)},`ModifyTime`=NOW() WHERE `AccountId`={AccountId} {(useFrozen ? $"AND (`Frozen`+{Amount})>=0;" : $"AND(`Balance`-`Frozen`+{Amount}) >= 0;")}";

                PostChangeSql = $"IFNULL((SELECT `PostChange` FROM `{RecordTableName}` WHERE `AccountId`={AccountId} ORDER BY `RecordId` DESC LIMIT 1),0)";
                StringBuilder TempRecordSql = new StringBuilder($"INSERT INTO `{RecordTableName}` ");
                TempRecordSql.Append("( `AccountId`, `PreChange`, `Incurred`, `PostChange`,`ModifyType`,`ModifyDesc`, `ModifyTime` ) ");
                TempRecordSql.Append($"SELECT {AccountId} AS `AccountId`, ");
                TempRecordSql.Append($"{PostChangeSql} AS `PreChange`, ");
                TempRecordSql.Append($"{Amount} AS `Incurred`, ");
                TempRecordSql.Append($"{PostChangeSql}+{Amount} AS `PostChange`, ");
                TempRecordSql.Append($"{(int)modifyType} AS `ModifyType`, ");
                TempRecordSql.Append($"'{String.Join(',', Desc)}' AS `ModifyDesc`, ");
                TempRecordSql.Append($"'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' AS`ModifyTime`");
                RecordSql = TempRecordSql.ToString();

                #region 修改账务
                if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
                if (isUserOutTransaction)
                {
                    IDbTransaction Tran = OutTran;
                    try
                    {
                        Int32 EditRow = context.Dapper.Execute(EditSQl, null, Tran);
                        Int32 RecordId = context.Dapper.Execute(RecordSql, null, Tran);
                        if (EditRow == RecordId && EditRow == 1)
                        {
                            if (!isUserOutTransaction)
                            {
                                Tran.Commit();
                            }
                            return new MyResult { Data = true };
                        }
                        Tran.Rollback();
                        return result.SetStatus(ErrorCode.InvalidData, "账户变更发生错误");
                    }
                    catch (Exception ex)
                    {
                        Tran.Rollback();
                        SystemLog.Debug($"钱包账户余额变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                        return result.SetStatus(ErrorCode.InvalidData, "发生错误");
                    }
                    finally
                    {
                        if (!isUserOutTransaction)
                        {
                            if (context.Dapper.State == ConnectionState.Open) { context.Dapper.Close(); }
                        }
                    }
                }
                else
                {
                    using (IDbTransaction Tran = context.Dapper.BeginTransaction())
                    {
                        try
                        {
                            Int32 EditRow = context.Dapper.Execute(EditSQl, null, Tran);
                            Int32 RecordId = context.Dapper.Execute(RecordSql, null, Tran);
                            if (EditRow == RecordId && EditRow == 1)
                            {
                                if (!isUserOutTransaction)
                                {
                                    Tran.Commit();
                                }
                                return new MyResult { Data = true };
                            }
                            Tran.Rollback();
                            return result.SetStatus(ErrorCode.InvalidData, "账户变更发生错误");
                        }
                        catch (Exception ex)
                        {
                            Tran.Rollback();
                            SystemLog.Debug($"钱包账户余额变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                            return result.SetStatus(ErrorCode.InvalidData, "发生错误");
                        }
                        finally
                        {
                            if (!isUserOutTransaction)
                            {
                                if (context.Dapper.State == ConnectionState.Open) { context.Dapper.Close(); }
                            }
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                SystemLog.Debug($"钱包账户余额变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                return result.SetStatus(ErrorCode.InvalidData, "发生错误");
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
        }
        #endregion


        /// <summary>
        /// 钱包账户余额冻结操作
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> FrozenWalletAmount(string AccountTableName, string CacheLockKey, IDbTransaction OutTran, bool isUserOutTransaction, long userId, decimal Amount)
        {
            MyResult result = new MyResult { Data = false };
            CSRedisClientLock CacheLock = null;
            String UpdateSql = $"UPDATE `{AccountTableName}` SET `Frozen`=`Frozen`+{Amount} WHERE `UserId`={userId} AND (`Balance`-`Frozen`)>={Amount} AND (`Frozen`+{Amount})>=0";
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}Init_{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                if (isUserOutTransaction)
                {
                    int Row = await context.Dapper.ExecuteAsync(UpdateSql, null, OutTran);
                    if (Row != 1) { return result.SetStatus(ErrorCode.InvalidData, $"账户余额{(Amount > 0 ? "冻结" : "解冻")}操作失败"); }
                    result.Data = true;
                    return result;
                }
                else
                {
                    int Row = await context.Dapper.ExecuteAsync(UpdateSql);
                    if (Row != 1) { return result.SetStatus(ErrorCode.InvalidData, $"账户余额{(Amount > 0 ? "冻结" : "解冻")}操作失败"); }
                    result.Data = true;
                    return result;
                }

            }
            catch (Exception ex)
            {
                SystemLog.Debug($"账户余额冻结操作发生错误,\r\n{UpdateSql}", ex);
                return result.SetStatus(ErrorCode.InvalidData, "发生错误");
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
        }

        /// <summary>
        /// 交易订单列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<List<CoinTradeDto>>> TradeList(TradeReqDto model)
        {
            MyResult<List<CoinTradeDto>> result = new MyResult<List<CoinTradeDto>>();
            model.PageIndex = model.PageIndex < 1 ? 1 : model.PageIndex;

            if (!ProcessSqlStr(model.Type)) { return result.SetStatus(ErrorCode.Unauthorized, "你涉嫌非法入侵已经被我方风控系统抓捕稍后会有官方人员和你取得联系"); }

            try
            {
                #region 拼接SQL
                StringBuilder QuerySql = new StringBuilder();
                StringBuilder CountSql = new StringBuilder();
                //左连接
                var lefjoin = $"";
                if (String.IsNullOrWhiteSpace(model.Sale) || model.Sale.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                {
                    lefjoin = $" left join user u on u.id=t.buyerUid";
                }
                else
                {
                    lefjoin = $" left join user u on u.id=t.sellerUid";
                }
                CountSql.Append("SELECT COUNT(`id`) FROM `coin_trade` WHERE `status` = 1 ");
                QuerySql.Append("SELECT t.`id`, t.`tradeNumber`,u.`name`, t.`buyerUid`, t.`sellerUid`, t.`price`, t.`totalPrice`, t.`amount`, t.`entryOrderTime`, ");
                QuerySql.Append("(SELECT COUNT(id) FROM coin_trade WHERE buyerUid = t.buyerUid AND status = 4 AND dealTime BETWEEN DATE_SUB(NOW(),INTERVAL 30 DAY) AND NOW()) AS Dishonesty ");
                QuerySql.Append($"FROM `coin_trade` AS t{lefjoin} WHERE t.`status` = 1 ");

                if (String.IsNullOrWhiteSpace(model.Sale) || model.Sale.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                {
                    CountSql.Append("AND `trendSide` = 'BUY' ");
                    QuerySql.Append("AND `trendSide` = 'BUY' ");

                    if (!string.IsNullOrEmpty(model.SearchText))
                    {
                        CountSql.Append("AND buyerAlipay = @SearchText ");
                        QuerySql.Append("AND buyerAlipay = @SearchText ");
                    }

                    switch (model.Range)
                    {
                        case 1:
                            CountSql.Append("AND amount>0 AND amount<=10 ");
                            QuerySql.Append("AND amount>0 AND amount<=10 ");
                            break;
                        case 2:
                            CountSql.Append("AND amount>10 AND amount<=50 ");
                            QuerySql.Append("AND amount>10 AND amount<=50 ");
                            break;
                        case 3:
                            CountSql.Append("AND amount>50 AND amount<=100 ");
                            QuerySql.Append("AND amount>50 AND amount<=100 ");
                            break;
                        case 4:
                            CountSql.Append("AND amount>100 AND amount<=500 ");
                            QuerySql.Append("AND amount>100 AND amount<=500 ");
                            break;
                        default:
                            break;
                    }

                    switch (model.Type.ToLower())
                    {
                        case "system":
                            // CountSql.Append("AND `buyerUid` = 1 ");

                            // QuerySql.Append("AND `buyerUid` = 1 ");
                            QuerySql.Append("ORDER BY price");
                            break;
                        case "amount":
                            // CountSql.Append("AND `buyerUid` != 1 ");

                            // QuerySql.Append("AND `buyerUid` != 1 ");
                            QuerySql.Append("ORDER BY ");
                            QuerySql.Append(model.Type);
                            break;
                        case "price":
                            // CountSql.Append("AND `buyerUid` != 1 ");

                            // QuerySql.Append("AND `buyerUid` != 1 ");
                            QuerySql.Append("ORDER BY ");
                            QuerySql.Append(model.Type);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    CountSql.Append("AND `trendSide` = 'SELL' ");
                    QuerySql.Append("AND `trendSide` = 'SELL' ");

                    if (!string.IsNullOrEmpty(model.SearchText))
                    {
                        CountSql.Append("AND sellerAlipay = @SearchText ");
                        QuerySql.Append("AND sellerAlipay = @SearchText ");
                    }
                    switch (model.Type.ToLower())
                    {
                        case "system":
                            CountSql.Append("AND `sellerAlipay` = 1 ");

                            QuerySql.Append("AND `sellerAlipay` = 1 ");
                            QuerySql.Append("ORDER BY price");
                            break;
                        case "amount":
                            CountSql.Append("AND `sellerAlipay` != 1 ");

                            QuerySql.Append("AND `sellerAlipay` != 1 ");
                            QuerySql.Append("ORDER BY ");
                            QuerySql.Append(model.Type);
                            break;
                        case "price":
                            CountSql.Append("AND `sellerAlipay` != 1 ");

                            QuerySql.Append("AND `sellerAlipay` != 1 ");
                            QuerySql.Append("ORDER BY ");
                            QuerySql.Append(model.Type);
                            break;
                        default:
                            break;
                    }
                }

                if (model.Order.Equals("asc", StringComparison.OrdinalIgnoreCase))
                {
                    QuerySql.Append(" ASC ");
                }
                else { QuerySql.Append(" DESC "); }

                QuerySql.Append("LIMIT @PageIndex, @PageSize;");

                DynamicParameters Param = new DynamicParameters();
                Param.Add("SearchText", model.SearchText, DbType.String);
                Param.Add("PageIndex", (model.PageIndex - 1) * model.PageSize, DbType.Int32);
                Param.Add("PageSize", model.PageSize, DbType.Int32);

                #endregion
                IEnumerable<CoinTradeDto> TradeList = await context.Dapper.QueryAsync<CoinTradeDto>(QuerySql.ToString(), Param);
                result.Data = TradeList.ToList();
                result.RecordCount = await context.Dapper.QueryFirstOrDefaultAsync<Int32>(CountSql.ToString(), Param);
                result.PageCount = (result.RecordCount + model.PageSize - 1) / model.PageSize;
                return result;
            }
            catch (Exception ex)
            {
                SystemLog.Debug(ex);
                result.Data = new List<CoinTradeDto>();
                return result;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Str"></param>
        /// <returns></returns>
        private static bool ProcessSqlStr(string Str)
        {
            string SqlStr;
            SqlStr = " |,|=|'|and|exec|insert|select|delete|update|count|*|chr|mid|master|truncate|char|declare";
            Str = Str.ToLower();
            bool ReturnValue = true;
            if (String.IsNullOrWhiteSpace(Str)) { return ReturnValue; }
            try
            {
                if (Str != "")
                {
                    string[] anySqlStr = SqlStr.Split('|');
                    foreach (string ss in anySqlStr)
                    {
                        if (Str.IndexOf(ss) >= 0)
                        {
                            ReturnValue = false;
                        }
                    }
                }
            }
            catch
            {
                ReturnValue = false;
            }
            return ReturnValue;
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<ListModel<TradeOrder>>> QueryTradeOrder(QueryTradeOrder query)
        {
            MyResult<ListModel<TradeOrder>> Rult = new MyResult<ListModel<TradeOrder>>();

            DynamicParameters QueryParam = new DynamicParameters();

            StringBuilder QuerySql = new StringBuilder();
            QuerySql.Append("SELECT o.id,o.tradeNumber, o.buyerUid, o.buyerAlipay, o.sellerUid, au.trueName, o.sellerAlipay, ");
            QuerySql.Append("o.amount, o.price, o.totalPrice, o.paidTime, o.pictureUrl, o.dealTime, o.`status`, o.appealTime, o.dealTime, o.buyerBan, o.sellerBan ");
            QuerySql.Append("FROM coin_trade AS o LEFT JOIN authentication_infos AS au ON o.sellerUid = au.userId WHERE 1 = 1 ");

            StringBuilder QueryCuntSql = new StringBuilder();
            QueryCuntSql.Append("SELECT COUNT(o.id) FROM coin_trade AS o LEFT JOIN authentication_infos AS au ON o.sellerUid = au.userId WHERE 1 = 1 ");

            if (query.Status != TradeState.All)
            {
                QuerySql.Append("AND o.status = @Status ");
                QueryCuntSql.Append("AND o.status = @Status ");
            }
            if (!string.IsNullOrEmpty(query.CoinType))
            {
                var coinType = query.CoinType == "U" ? "USDT" : "RZQ";
                QuerySql.Append($"AND o.coinType ='{coinType}' ");
                QueryCuntSql.Append($"AND o.coinType ='{coinType}' ");
            }


            #region 类型
            switch (query.Type)
            {
                case "buyer":
                    if (!String.IsNullOrWhiteSpace(query.Alipay))
                    {
                        QueryParam.Add("Alipay", query.Alipay, DbType.String);
                        QuerySql.Append("AND o.buyerAlipay = @Alipay ");
                        QueryCuntSql.Append("AND o.buyerAlipay = @Alipay; ");
                    }
                    if (!String.IsNullOrWhiteSpace(query.Mobile))
                    {
                        QueryParam.Add("Mobile", query.Mobile, DbType.String);
                        QuerySql.Append("AND o.buyerUid = (SELECT id FROM `user` WHERE mobile = @Mobile LIMIT 1) ");
                        QueryCuntSql.Append("AND o.buyerUid = (SELECT id FROM `user` WHERE mobile = @Mobile LIMIT 1); ");
                    }
                    break;
                case "seller":
                    if (!String.IsNullOrWhiteSpace(query.Alipay))
                    {
                        QueryParam.Add("Alipay", query.Alipay, DbType.String);
                        QuerySql.Append("AND o.sellerAlipay = @Alipay ");
                        QueryCuntSql.Append("AND o.sellerAlipay = @Alipay; ");
                    }
                    if (!String.IsNullOrWhiteSpace(query.Mobile))
                    {
                        QueryParam.Add("Mobile", query.Mobile, DbType.String);
                        QuerySql.Append("AND o.sellerUid = (SELECT id FROM `user` WHERE mobile = @Mobile LIMIT 1) ");
                        QueryCuntSql.Append("AND o.sellerUid = (SELECT id FROM `user` WHERE mobile = @Mobile LIMIT 1); ");
                    }
                    break;
                default:
                    break;
            }
            #endregion
            QueryParam.Add("Status", (Int32)query.Status, DbType.Int32);
            QueryParam.Add("PageIndex", (query.PageIndex - 1) * query.PageSize, DbType.Int32);
            QueryParam.Add("PageSize", query.PageSize, DbType.Int32);

            Rult.RecordCount = await context.Dapper.QueryFirstOrDefaultAsync<Int32>(QueryCuntSql.ToString(), QueryParam);
            QuerySql.Append("ORDER BY o.id DESC LIMIT @PageIndex, @PageSize;");

            IEnumerable<CoinTradeModel> TradeOrders = await context.Dapper.QueryAsync<CoinTradeModel>(QuerySql.ToString(), QueryParam);

            Rult.PageCount = Rult.RecordCount / query.PageSize;
            Rult.Data = new ListModel<TradeOrder>()
            {
                List = new List<TradeOrder>()
            };
            foreach (var item in TradeOrders)
            {
                TradeOrder order = new TradeOrder()
                {
                    Id = item.Id,
                    OrderId = item.TradeNumber,
                    BuyerUid = item.BuyerUid ?? 0,
                    BuyerAlipay = item.BuyerAlipay,
                    SellerUid = item.SellerUid ?? 0,
                    SellerAlipay = item.SellerAlipay,
                    TrueName = item.TrueName,
                    UnitPrice = item.Price ?? 0,
                    SellCount = item.Amount ?? 0,
                    TotalPrice = item.TotalPrice ?? 0,
                    TradeFee = item.Fee ?? 0,
                    ConfirmTime = item.DealTime,
                    PayTime = item.PaidTime,
                    TradeState = (TradeState)item.Status,
                    AppealTime = item.AppealTime
                };
                if (item.BuyerBan != item.SellerBan)
                {
                    order.TimeOutUser = item.BuyerBan > item.SellerBan ? "买家超时" : "卖家超时";
                }
                if (!string.IsNullOrWhiteSpace(item.PictureUrl)) { order.PayPic = $"https://file.yoyoba.cn/{item.PictureUrl}"; }
                Rult.Data.List.Add(order);
            }

            return Rult;
        }

        /// <summary>
        /// 恢复订单至
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> ResumeTradeOrder(TradeOrder order)
        {
            MyResult<object> result = new MyResult<object>();

            CoinTrade TradeOrder = context.Dapper.QueryFirstOrDefault<CoinTrade>("SELECT * FROM coin_trade WHERE id = @Id;", new { Id = order.Id });
            order.TradeState = (TradeState)TradeOrder.Status;
            if (order.TradeState == TradeState.Completed) { return result.SetStatus(ErrorCode.SystemError, "此订单交易已完成"); }
            if (order.TradeState == TradeState.WaitPay) { return result.SetStatus(ErrorCode.SystemError, "此订单未支付"); }
            if (order.TradeState == TradeState.Normal || order.TradeState == TradeState.Cancelled || order.TradeState == TradeState.AlreadyPay) { return result.SetStatus(ErrorCode.SystemError, "此订单无需操作"); }

            StringBuilder ResumeTradeSql = new StringBuilder();
            DynamicParameters ResumeTradeParam = new DynamicParameters();
            Int32 rows = 0;
            if (order.TradeState == TradeState.Appeal)
            {
                ResumeTradeSql.Append("UPDATE `coin_trade` SET `status` = 3, `paidEndTime` = @PaidEndTime WHERE `id` = @Id;");
                ResumeTradeParam.Add("Id", order.Id, DbType.Int64);
                ResumeTradeParam.Add("PaidEndTime", DateTime.Now.AddDays(1), DbType.DateTime);
                rows = await context.Dapper.ExecuteAsync(ResumeTradeSql.ToString(), ResumeTradeParam);
            }
            TradeOrder.BuyerBan = TradeOrder.BuyerBan ?? 0;
            TradeOrder.SellerBan = TradeOrder.SellerBan ?? 0;
            if (order.TradeState == TradeState.TimeOut && TradeOrder.SellerBan == 1) { return result.SetStatus(ErrorCode.SystemError, "此订单不可恢复"); }
            if (order.TradeState == TradeState.TimeOut && TradeOrder.BuyerBan == 1)
            {
                ResumeTradeParam.Add("Id", order.Id, DbType.Int64);
                ResumeTradeParam.Add("UserId", TradeOrder.SellerUid, DbType.Int64);
                ResumeTradeParam.Add("PaidEndTime", DateTime.Now.AddDays(1), DbType.DateTime);
                ResumeTradeParam.Add("FreezeCandy", TradeOrder.Amount + TradeOrder.Fee, DbType.Decimal);
                rows = await context.Dapper.ExecuteAsync("UPDATE `user` SET freezeCandyNum = freezeCandyNum + @FreezeCandy, candyNum = candyNum - @FreezeCandy WHERE id = @UserId And candyNum > @FreezeCandy;", ResumeTradeParam);
                if (rows < 1) { return result.SetStatus(ErrorCode.SystemError, "恢复失败,卖家不足"); }
                rows = await context.Dapper.ExecuteAsync("UPDATE `coin_trade` SET `status` = 2, `paidEndTime` = @PaidEndTime, `dealEndTime` = @PaidEndTime WHERE `id` = @Id;", ResumeTradeParam);
            }
            if (rows < 1) { return result.SetStatus(ErrorCode.SystemError, "恢复失败"); }
            result.Data = true;
            return result;
        }

        /// <summary>
        /// 封禁买家
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> BanBuyer(TradeOrder order)
        {
            MyResult<object> result = new MyResult<object>();

            StringBuilder BaneSql = new StringBuilder();
            BaneSql.Append("UPDATE `user` SET `status` = 2, `passwordSalt` = @Reason WHERE `id` = @UserId;");
            DynamicParameters ResumeTradeParam = new DynamicParameters();
            ResumeTradeParam.Add("UserId", order.BuyerUid, DbType.Int64);
            ResumeTradeParam.Add("Reason", order.AppealReason ?? "", DbType.String);

            Int32 rows = await context.Dapper.ExecuteAsync(BaneSql.ToString(), ResumeTradeParam);
            if (rows < 1) { return result.SetStatus(ErrorCode.SystemError, "封禁失败"); }
            result.Data = true;
            return result;
        }

        /// <summary>
        /// 解除订单超时封禁
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> Unblock(TradeOrder order)
        {
            MyResult<object> rult = new MyResult<object>();
            DynamicParameters UpdateParam = new DynamicParameters();
            UpdateParam.Add("Id", order.Id);
            Int32 rows = await context.Dapper.ExecuteAsync("UPDATE coin_trade SET buyerBan = 0, sellerBan = 0 WHERE id = @Id;", UpdateParam);

            if (rows > 0)
            {
                rult.Data = rows;
                return rult;
            }
            return rult.SetStatus(ErrorCode.InvalidData, "解除失败");
        }

        /// <summary>
        /// 查询申诉
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<MyResult<List<TradeAppeals>>> ViewAppeal(TradeOrder order)
        {
            MyResult<List<TradeAppeals>> Rult = new MyResult<List<TradeAppeals>>();

            DynamicParameters QueryParam = new DynamicParameters();
            QueryParam.Add("OrderId", order.Id, DbType.Int64);
            IEnumerable<TradeAppeals> appeals = await context.Dapper.QueryAsync<TradeAppeals>("SELECT id, orderId, picUrl, description, createdAt, `status` FROM	appeals WHERE orderId = @OrderId;", QueryParam);

            Rult.Data = new List<TradeAppeals>();

            foreach (var item in appeals)
            {
                item.PicUrl = item.PicUrl ?? "";
                item.PicUrl = $"https://file.yoyoba.cn/{item.PicUrl}";
                Rult.Data.Add(item);
            }
            return Rult;
        }

        public MyResult<object> ForcePaidCoin()
        {
            throw new NotImplementedException();
        }

        public async Task<MyResult<object>> CloseTradeOrder(TradeOrder order)
        {
            MyResult result = new MyResult();
            var orderInfo = await context.Dapper.QueryFirstOrDefaultAsync<CoinTrade>($"select * from coin_trade where id={order.Id} and status!=0");
            if (orderInfo == null)
            {
                return result.SetStatus(ErrorCode.InvalidData, "订单不存在");
            }
            //关闭订单
            var bueryOrSellerSql = "";
            if (orderInfo.Status == 2)
            {
                bueryOrSellerSql = ",buyerBan = 1";
            }

            if (orderInfo.Status == 3 && orderInfo.CoinType != "RZQ")
            {
                bueryOrSellerSql = ",sellerBan = 1";
            }
            //解冻卖方
            if (orderInfo.CoinType != "RZQ")
            {
                var res = await FrozenWalletAmount(CottonCoinTableName, CacheCottonCoinLockKey, null, false, (long)orderInfo.SellerUid, -(decimal)(orderInfo.Amount + orderInfo.Fee));
                if (res.Code != 200)
                {
                    return result.SetStatus(ErrorCode.InvalidData, res.Message);
                }
            }

            await context.Dapper.ExecuteAsync($"UPDATE coin_trade SET status = 0{bueryOrSellerSql} WHERE id = {orderInfo.Id};");
            return result;
        }
    }

}
