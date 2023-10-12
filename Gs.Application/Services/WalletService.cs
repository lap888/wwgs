using CSRedis;
using Dapper;
using System;
using System.Data;
using System.Linq;
using System.Text;
using Gs.Domain.Enums;
using Gs.Domain.Repository;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Gs.Core;
using Gs.Domain.Entity;
using Gs.Domain.Context;
using Gs.Domain.Models.Dto;
using Gs.Core.Utils;

namespace Gs.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly String CacheLockKey = "WalletAccount:";
        private readonly String AccountTableName = "user_account_wallet";
        private readonly String RecordTableName = "user_account_wallet_record";
        private readonly CSRedisClient RedisCache;
        private readonly IAlipay AlipaySub;
        private readonly IWePayPlugin WePaySub;
        private readonly Models.AppSetting AppSetting;
        private readonly WwgsContext context;
        public WalletService(WwgsContext mysql, CSRedisClient redis, IAlipay alipay, IWePayPlugin wePay, IOptionsMonitor<Models.AppSetting> monitor)
        {
            this.RedisCache = redis;
            this.AlipaySub = alipay;
            this.WePaySub = wePay;
            this.context = mysql;
            this.AppSetting = monitor.CurrentValue;
        }

        /// <summary>
        /// 初始化钱包账户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public async Task<MyResult<object>> InitWalletAccount(long userId)
        {
            MyResult result = new MyResult { Data = false };
            CSRedisClientLock CacheLock = null;
            String InsertSql = $"INSERT INTO `{AccountTableName}` (`UserId`, `Revenue`, `Expenses`, `Balance`, `Frozen`, `ModifyTime`) VALUES ({userId}, '0', '0', '0', '0', NOW())";
            String SelectSql = $"SELECT COUNT(1) AS `Have` FROM `{AccountTableName}` WHERE `UserId` = {userId} LIMIT 1";
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}Init_{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                int Have = await context.Dapper.QueryFirstOrDefaultAsync<int>(SelectSql);
                if (Have != 0) { return result.SetStatus(ErrorCode.InvalidData, "账户已存在"); }
                int Row = await context.Dapper.ExecuteAsync(InsertSql);
                if (Row != 1) { return result.SetStatus(ErrorCode.InvalidData, "账户初始化失败"); }
                result.Data = true;
                return result;
            }
            catch (Exception ex)
            {
                SystemLog.Debug($"初始化钱包账户发生错误\r\n插入语句：{InsertSql}\r\n判断语句：{SelectSql}", ex);
                return result.SetStatus(ErrorCode.InvalidData, "发生错误");
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
        }

        /// <summary>
        /// 获取钱包账户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<UserAccountWallet>> WalletAccountInfo(long userId)
        {
            MyResult<UserAccountWallet> result = new MyResult<UserAccountWallet>();
            await InitWalletAccount(userId);
            String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {userId} LIMIT 1";
            UserAccountWallet accInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserAccountWallet>(SelectSql);
            if (accInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "账户不存在"); }
            result.Data = accInfo;
            return result;
        }

        /// <summary>
        /// 获取红包账户记录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <param name="ModifyType"></param>
        /// <returns></returns>
        public async Task<MyResult<List<UserAccountWalletRecord>>> WalletAccountRecord(long userId, int PageIndex = 1, int PageSize = 20, AccountModifyType ModifyType = AccountModifyType.ALL)
        {
            MyResult<List<UserAccountWalletRecord>> result = new MyResult<List<UserAccountWalletRecord>>();
            if (PageIndex < 1) { PageIndex = 1; }

            String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {userId} LIMIT 1";
            UserAccountWallet accInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserAccountWallet>(SelectSql);
            if (accInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "账户不存在"); }
            #region 拼接SQL
            DynamicParameters QueryParam = new DynamicParameters();
            QueryParam.Add("AccountId", accInfo.AccountId, DbType.Int64);

            StringBuilder QueryCountSql = new StringBuilder();
            QueryCountSql.Append("SELECT COUNT(1) AS `Count` FROM ");
            QueryCountSql.Append(RecordTableName);
            QueryCountSql.Append(" WHERE `AccountId` = @AccountId ");

            StringBuilder QueryDataSql = new StringBuilder();
            QueryDataSql.Append("SELECT * FROM ");
            QueryDataSql.Append(RecordTableName);
            QueryDataSql.Append(" WHERE `AccountId` = @AccountId ");
            if (ModifyType != AccountModifyType.ALL)
            {
                QueryParam.Add("ModifyType", (Int32)ModifyType, DbType.Int32);
                QueryCountSql.Append("AND `ModifyType` = @ModifyType ");
                QueryDataSql.Append("AND `ModifyType` = @ModifyType ");
            }
            QueryCountSql.Append(";");
            QueryParam.Add("PageIndex", (PageIndex - 1) * PageSize, DbType.Int32);
            QueryParam.Add("PageSize", PageIndex * PageSize, DbType.Int32);
            QueryDataSql.Append("ORDER BY RecordId DESC LIMIT @PageIndex,@PageSize;");
            #endregion

            result.RecordCount = await context.Dapper.QueryFirstOrDefaultAsync<int>(QueryCountSql.ToString(), QueryParam);
            result.PageCount = (result.RecordCount + PageSize - 1) / PageSize;
            IEnumerable<UserAccountWalletRecord> Records = await context.Dapper.QueryAsync<UserAccountWalletRecord>(QueryDataSql.ToString(), QueryParam);

            result.Data = Records.ToList();
            return result;
        }

        /// <summary>
        /// 钱包账户余额冻结操作
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> FrozenWalletAmount(long userId, decimal Amount)
        {
            MyResult result = new MyResult { Data = false };
            CSRedisClientLock CacheLock = null;
            String UpdateSql = $"UPDATE `{AccountTableName}` SET `Frozen`=`Frozen`+{Amount} WHERE `UserId`={userId} AND (`Balance`-`Frozen`)>={Amount} AND (`Frozen`+{Amount})>=0";
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}Init_{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                int Row = await context.Dapper.ExecuteAsync(UpdateSql);
                if (Row != 1) { return result.SetStatus(ErrorCode.InvalidData, $"账户余额{(Amount > 0 ? "冻结" : "解冻")}操作失败"); }
                result.Data = true;
                return result;
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
        /// 钱包账户余额变更
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <param name="useFrozen">使用冻结金额，账户金额增加时，此参数无效</param>
        /// <param name="modifyType">账户变更类型</param>
        /// <param name="Desc">描述</param>
        /// <returns></returns>
        public async Task<MyResult<object>> ChangeWalletAmount(long userId, decimal Amount, AccountModifyType modifyType, bool useFrozen, params string[] Desc)
        {
            MyResult result = new MyResult { Data = false };
            if (Amount == 0) { return new MyResult { Data = true }; }   //账户无变动，直接返回成功
            if (Amount > 0 && useFrozen) { useFrozen = false; } //账户增加时，无法使用冻结金额
            CSRedisClientLock CacheLock = null;
            UserAccountWallet UserAccount;
            Int64 AccountId;
            String Field = String.Empty, EditSQl = String.Empty, RecordSql = String.Empty, PostChangeSql = String.Empty;
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}Init_{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }

                #region 验证账户信息
                String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {userId} LIMIT 1";
                UserAccount = await context.Dapper.QueryFirstOrDefaultAsync<UserAccountWallet>(SelectSql);
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
                TempRecordSql.Append("( `AccountId`, `PreChange`, `Incurred`, `PostChange`, `ModifyType`, `ModifyDesc`, `ModifyTime` ) ");
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
                using (IDbTransaction Tran = context.Dapper.BeginTransaction())
                {
                    try
                    {
                        Int32 EditRow = context.Dapper.Execute(EditSQl, null, Tran);
                        Int32 RecordId = context.Dapper.Execute(RecordSql, null, Tran);
                        if (EditRow == RecordId && EditRow == 1)
                        {
                            Tran.Commit();
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
                    finally { if (context.Dapper.State == ConnectionState.Open) { context.Dapper.Close(); } }
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

        /// <summary>
        /// 钱包充值
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> Recharge(int userId, string type, decimal Amount)
        {
            MyResult<Object> result = new MyResult<Object>();
            if (userId < 0) { return result.SetStatus(ErrorCode.InvalidData, "用户信息错误"); }
            String CacheKey = $"AliAction_Recharge:{userId}";
            if (RedisCache.Exists(CacheKey)) { return new MyResult<object> { Code = -1, Message = "请勿重复提交" }; }

            switch (type)
            {
                case "alipay":
                    return await AliPayUrl(userId, Amount, ActionType.CASH_RECHARGE);
                case "wepay":
                    return await WePayUrl(userId, Amount, ActionType.CASH_RECHARGE);
                default:
                    return result.SetStatus(ErrorCode.InvalidData, "支付类型错误");
            }
        }

        /// <summary>
        /// 支付宝支付
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Amount"></param>
        /// <param name="action"></param>
        /// <param name="Custom"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> AliPayUrl(int UserId, Decimal Amount, ActionType action, String Custom = "")
        {
            MyResult result = new MyResult();
            if (UserId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"CreatePayUrl:{UserId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//
                decimal Fee = Math.Ceiling(Amount * 0.006M * 100.00M) * 0.01M;
                PayInfo info = new PayInfo
                {
                    UserId = UserId,
                    Channel = PayChannel.AliPay,
                    Currency = Currency.Rmb,
                    Custom = String.IsNullOrWhiteSpace(Custom) ? String.Empty : Custom,
                    Amount = Amount,
                    Fee = Fee,
                    ActionType = action,
                    ChannelUID = String.Empty,
                    CreateTime = DateTime.Now,
                    ModifyTime = null,
                    PayStatus = PayStatus.UN_PAID,
                };

                String PayId = context.Dapper.ExecuteScalar<String>("INSERT INTO `pay_record` (`UserId`, `Channel`, `Currency`, `Amount`, `Fee`, `ActionType`, `Custom`, `PayStatus`, `ChannelUID`, `CreateTime`) VALUES (@UserId, @Channel, @Currency,@Amount, @Fee,@ActionType, @Custom, @PayStatus, @ChannelUID, NOW());SELECT @@IDENTITY", info);
                if (String.IsNullOrWhiteSpace(PayId)) { return result.SetStatus(ErrorCode.InvalidData, "生成支付订单失败"); }
                String AppUrl = await AlipaySub.GetSignStr(new Request.ReqAlipayAppSubmit() { OutTradeNo = PayId, TotalAmount = Amount.ToString("0.00"), Subject = action.GetDescription(), NotifyUrl = AppSetting.AlipayNotify, TimeOutExpress = "15m", PassbackParams = action.ToString() });
                result.Data = AppUrl;
            }
            catch (Exception)
            {
                return result.SetStatus(ErrorCode.InvalidData, "生成支付订单失败");
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
        /// 微信支付
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Amount"></param>
        /// <param name="action"></param>
        /// <param name="Custom"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> WePayUrl(int UserId, Decimal Amount, ActionType action, String Custom = "")
        {
            MyResult result = new MyResult();
            if (UserId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"CreatePayUrl:{UserId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//
                decimal Fee = Math.Ceiling(Amount * 0.006M * 100.00M) * 0.01M;
                PayInfo info = new PayInfo
                {
                    UserId = UserId,
                    Channel = PayChannel.WePay,
                    Currency = Currency.Rmb,
                    Custom = String.IsNullOrWhiteSpace(Custom) ? String.Empty : Custom,
                    Amount = Amount,
                    Fee = Fee,
                    ActionType = action,
                    ChannelUID = String.Empty,
                    CreateTime = DateTime.Now,
                    ModifyTime = null,
                    PayStatus = PayStatus.UN_PAID,
                };

                String PayId = context.Dapper.ExecuteScalar<String>("INSERT INTO `pay_record` (`UserId`, `Channel`, `Currency`, `Amount`, `Fee`, `ActionType`, `Custom`, `PayStatus`, `ChannelUID`, `CreateTime`) VALUES (@UserId, @Channel, @Currency,@Amount, @Fee,@ActionType, @Custom, @PayStatus, @ChannelUID, NOW());SELECT @@IDENTITY", info);
                if (String.IsNullOrWhiteSpace(PayId)) { return result.SetStatus(ErrorCode.InvalidData, "生成支付订单失败"); }

                var model = await WePaySub.Execute(new Request.ReqWepaySubmit()
                {
                    TradeNo = PayId,
                    Body = action.GetDescription(),
                    TotalFee = Math.Ceiling(Amount * 100.00M).ToInt(),
                    Attach = action.ToString(),
                    TradeType = "APP",
                    NotifyUrl = AppSetting.WePayNotify,
                });
                result.Data = new { TradeNo = PayId, PayStr = WePaySub.MakeSign(model.PrepayId) };
            }
            catch (Exception)
            {
                return result.SetStatus(ErrorCode.InvalidData, "生成支付订单失败");
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
        /// 提现
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <param name="TradePwd"></param>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> Withdraw(int userId, decimal Amount, string TradePwd, string TradeNo)
        {
            MyResult result = new MyResult();
            if (GetWeek(DateTime.Now) != 2 || DateTime.Now.Hour < 9 || DateTime.Now.Hour > 18)
            {
                return result.SetStatus(ErrorCode.InvalidData, "提现时间：每周二9:00-18:00");
            }

            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            if (string.IsNullOrWhiteSpace(TradeNo) || Amount <= 0) { return result.SetStatus(ErrorCode.InvalidData, "参数有误"); }
            var userInfo = context.Dapper.QueryFirstOrDefault<UserEntity>($"select * from user where id={userId}");
            if (!userInfo.TradePwd.Equals(SecurityUtil.MD5(TradePwd))) { return result.SetStatus(ErrorCode.InvalidData, "支付密码错误"); }
            string[] param = { TradeNo };
            if (userInfo == null)
            {
                return result.SetStatus(ErrorCode.InvalidData, "您是来搞笑的吗?");
            }
            Decimal WithdrawLimit = 50.00M;
            try
            {
                WithdrawLimit = AppSetting.Levels.FirstOrDefault(item => item.Level.Equals(userInfo.Level, StringComparison.OrdinalIgnoreCase)).WithdrawLimit;
            }
            catch { }

            if (Amount > WithdrawLimit) { return result.SetStatus(ErrorCode.InvalidData, $"当前等级限额{WithdrawLimit.ToString("0.##")}元"); }

            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"Withdraw:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                String DayLimitLock = $"DayLimitLock:{userId}";
                if (RedisCache.Exists(DayLimitLock))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "Yo帮试运行期间每日限提现一次");
                }

                Decimal WithdrawAndFee = Amount * AppSetting.WithdrawRate;
                if (WithdrawAndFee <= 0) { return result.SetStatus(ErrorCode.InvalidData, "系统维护中"); }
                var changeResult = await ChangeWalletAmount(userId, -WithdrawAndFee, AccountModifyType.CASH_WITH_DRAW, false, TradeNo);
                if (changeResult.Data == null || !bool.TryParse(changeResult.Data.ToString(), out bool changeSuccess)) { return changeResult; }
                if (changeSuccess != true) { return result; }
                #region 执行提现操作
                //result.Data = false;
                //var Push = RedisCache.Publish("YoYo_Wallet_Withdraw", new { TradeNo = TradeNo, ActionType = (int)ActionType.CASH_WITH_DRAW, UserId = userId, Amount = Amount, Desc = param.ToList() }.ToJson());
                //if (Push == 1) { result.Data = true; } else { SystemLog.Error($"用户提现=发送订阅失败:{TradeNo}-{userId}-{Amount}"); }
                #endregion
                RedisCache.Set(DayLimitLock, Amount, DateTime.Now.AddDays(1).Date - DateTime.Now);
                SystemLog.Error($"{userId}={Amount}={TradeNo}");

                return result.SetStatus(ErrorCode.InvalidData, "提现已受理,加急请联系客服");
            }
            catch (Exception ex)
            {
                SystemLog.Error("用户提现失败", ex);
                return result.SetStatus(ErrorCode.InvalidData, "用户提现失败");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }

        /// <summary>
        /// 获取星期几
        /// </summary>
        /// <param name="Dt"></param>
        /// <returns></returns>
        private Int32 GetWeek(DateTime? Dt = null)
        {
            if (Dt == null) { Dt = DateTime.Now; }
            Int32 Week = (Int32)Dt?.DayOfWeek;
            if (Week == 0) { Week = 7; }
            return Week;
        }

    }
}
