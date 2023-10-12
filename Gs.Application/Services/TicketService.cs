using CSRedis;
using Dapper;
using Gs.Domain.Repository;
using Gs.Core;
using Gs.Core.Extensions;
using Gs.Core.Utils;
using Gs.Domain.Context;
using Gs.Domain.Entity;
using Gs.Domain.Enums;
using Gs.Domain.Models;
using Gs.Domain.Models.Ticket;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Gs.Application.Services
{
    /// <summary>
    /// 认证券
    /// </summary>
    public class TicketService : ITicketService
    {
        private readonly String AccountTableName = "user_account_ticket";
        private readonly String RecordTableName = "user_account_ticket_record";
        private readonly String CacheLockKey = "TicketAccount:";

        private readonly WwgsContext context;
        private readonly CSRedisClient RedisCache;
        private readonly Models.AppSetting AppConf;
        private readonly Models.TicketConfig TicketConf;

        public TicketService(WwgsContext mySql, CSRedisClient redisClient, IOptionsMonitor<Models.TicketConfig> monitor, IOptionsMonitor<Models.AppSetting> options)
        {
            context = mySql;
            RedisCache = redisClient;
            TicketConf = monitor.CurrentValue;
            AppConf = options.CurrentValue;
        }

        /// <summary>
        /// 初始化认证券账户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public async Task<MyResult<object>> InitAccount(long userId)
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
                SystemLog.Debug($"初始化股认证券账户发生错误\r\n插入语句：{InsertSql}\r\n判断语句：{SelectSql}", ex);
                return result.SetStatus(ErrorCode.InvalidData, "发生错误");
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
        }

        /// <summary>
        /// 认证券页面
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<TicketModel>> TicketPage(long userId)
        {
            MyResult<TicketModel> result = new MyResult<TicketModel>();
            String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {userId} LIMIT 1";
            TicketModel accInfo = await context.Dapper.QueryFirstOrDefaultAsync<TicketModel>(SelectSql);
            if (accInfo == null)
            {
                await InitAccount(userId);
                accInfo = new TicketModel();
                accInfo.WatchCunt = 0;
            }
            else
            {
                accInfo.WatchCunt = context.Dapper.QueryFirstOrDefault<Int32?>($"SELECT COUNT(RecordId) FROM {RecordTableName} WHERE ModifyType = 3 AND TO_DAYS(NOW()) = TO_DAYS(ModifyTime) AND AccountId = @AccountId;", new { accInfo.AccountId }) ?? 0;
            }
            accInfo.Package = TicketConf.Package;
            accInfo.Rules = TicketConf.Rules;
            result.Data = accInfo;
            return result;
        }

        /// <summary>
        /// 认证券账户
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<UserAccountTicket>> TicketInfo(long userId)
        {
            MyResult<UserAccountTicket> result = new MyResult<UserAccountTicket>();
            String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {userId} LIMIT 1";
            UserAccountTicket accInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserAccountTicket>(SelectSql);
            if (accInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "账户不存在"); }
            result.Data = accInfo;
            return result;
        }

        /// <summary>
        /// 认证券开关
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> TicketSwitch(Int64 UserId)
        {
            MyResult<Object> result = new MyResult<Object>();
            String TicketLockStr = $"TicketSwitch:{UserId}";
            if (RedisCache.Exists(TicketLockStr))
            {
                return result.SetStatus(ErrorCode.InvalidData, "操作过于频繁，请稍后重试");
            }
            RedisCache.Set(TicketLockStr, UserId, 5);

            String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {UserId} LIMIT 1";
            UserAccountTicket accInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserAccountTicket>(SelectSql);
            if (accInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "请先兑换优惠券"); }
            StringBuilder UpdateSql = new StringBuilder();
            UpdateSql.Append("UPDATE `user_account_ticket` SET `State` = @State WHERE `AccountId` = @AccountId;");
            DynamicParameters UpdateParam = new DynamicParameters();
            UpdateParam.Add("AccountId", accInfo.AccountId, DbType.Int64);
            if (accInfo.State == 0)
            {
                accInfo.State = 1;
                UpdateParam.Add("State", 1, DbType.Int32);
            }
            else
            {
                accInfo.State = 0;
                UpdateParam.Add("State", 0, DbType.Int32);
            }
            Int32 Rows = context.Dapper.Execute(UpdateSql.ToString(), UpdateParam);
            if (Rows > 0)
            {
                result.Data = accInfo;
                return result;
            }
            return result.SetStatus(ErrorCode.InvalidData, "操作过于频繁");
        }

        /// <summary>
        /// 兑换认证券
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> ExchangeTicketOld(TicketExchange exchange)
        {
            MyResult<Object> result = new MyResult<object>();
            if (TicketConf.SubscribeTotal == 0) { return result.SetStatus(ErrorCode.InvalidData, "认证券已兑完"); }
            Boolean IsSuccess = false;
            String CacheLock = $"Ticket:Exchange{exchange.UserId}";
            if (RedisCache.Exists(CacheLock)) { return result.SetStatus(ErrorCode.InvalidData, "您操作太快了~"); }
            else { RedisCache.Set(CacheLock, exchange.Shares, 5); }

            #region 基础验证
            if (exchange.UserId < 1) { return result.SetStatus(ErrorCode.ErrorSign, "请重新登陆后,重试"); }
            await InitAccount(exchange.UserId); //初始化账户

            Int32 TicketTotal = context.Dapper.QueryFirstOrDefault<Int32>($"SELECT SUM(Balance) FROM {AccountTableName};");
            if (TicketTotal >= TicketConf.SubscribeTotal) { return result.SetStatus(ErrorCode.InvalidData, "认证券已兑完~"); }

            Int32 DayTotal = context.Dapper.QueryFirstOrDefault<Int32>($"SELECT COUNT(RecordId) FROM {RecordTableName} WHERE ModifyType = 1 AND TO_DAYS(NOW()) = TO_DAYS(ModifyTime);");
            if (DayTotal >= TicketConf.DayShares) { return result.SetStatus(ErrorCode.InvalidData, "今日认证券已兑完,明天早点来哦~"); }
            if ((TicketConf.DayShares - DayTotal) < exchange.Shares)
            {
                return result.SetStatus(ErrorCode.InvalidData, $"本日可兑认证券，仅剩{TicketConf.DayShares - DayTotal}份~");
            }
            var UserInfo = context.Dapper.QueryFirstOrDefault<UserEntity>("SELECT * FROM `user` WHERE id = @UserId;", new { UserId = exchange.UserId });
            //if (!UserInfo.TradePwd.Equals(SecurityUtil.MD5(exchange.PayPwd))) { return result.SetStatus(ErrorCode.InvalidData, "支付密码错误"); }
            if (UserInfo.AuditState != 2) { return result.SetStatus(ErrorCode.NoAuth, "没有实名认证"); }
            if (UserInfo.Status == 2 || UserInfo.Status == 3 || UserInfo.Status == 5) { return result.SetStatus(ErrorCode.AccountDisabled, "账号异常 请联系管理员"); }

            #region 重新计算会员等级
            String UserLevel = UserInfo.Level.ToLower();
            Decimal UserDevote = context.Dapper.ExecuteScalar<Decimal>($"SELECT IFNULL(SUM(Devote),0) AS Devote FROM yoyo_member_devote WHERE UserId={UserInfo.Id}") + (UserInfo.Golds == null ? 0 : UserInfo.Golds.Value);
            Models.UserLevel tmpSysLevel = AppConf.Levels.FirstOrDefault(o => o.Level.ToLower().Equals(UserLevel));
            if (UserDevote < tmpSysLevel.Claim)
            {
                foreach (Models.UserLevel item in AppConf.Levels.OrderBy(o => o.Claim))
                {
                    if (UserDevote >= item.Claim) { UserLevel = item.Level; }
                }
                context.Dapper.Execute($"UPDATE `user` SET `level`='{UserLevel}' WHERE Id={UserInfo.Id}");
            }
            Models.UserLevel SysLevel = AppConf.Levels.FirstOrDefault(o => o.Level.ToLower().Equals(UserLevel));
            if (null == SysLevel) { return result.SetStatus(ErrorCode.InvalidData, "等级异常，请联系管理员"); }
            #endregion

            TicketPack PackInfo = TicketConf.Package.FirstOrDefault(item => item.Shares == exchange.Shares);
            if (PackInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "请求参数有误"); }

            Decimal CandyTotal = PackInfo.Candy * SysLevel.SellRate;
            Decimal PeelTotal = CandyTotal * 2;
            #endregion

            #region 拼装SQL 并扣款
            //扣除 账户 1
            StringBuilder DeductSql = new StringBuilder();
            DynamicParameters DeductParams = new DynamicParameters();
            DeductParams.Add("UserId", exchange.UserId, DbType.Int64);
            DeductParams.Add("PayCandy", CandyTotal, DbType.Decimal);
            DeductParams.Add("PayPeel", PeelTotal, DbType.Decimal);
            DeductSql.Append("UPDATE `user` SET candyNum = candyNum - @PayCandy, candyP = candyP - @PayPeel ");
            DeductSql.Append("WHERE id = @UserId AND candyNum >= @PayCandy AND candyP >= @PayPeel;");

            //写入 糖果扣除记录 1
            StringBuilder CandyRecordSql = new StringBuilder();
            DynamicParameters CandyRecordParams = new DynamicParameters();
            CandyRecordSql.Append("INSERT INTO `gem_records`(`userId`, `num`, `createdAt`, `updatedAt`, `description`, `gemSource`) ");
            CandyRecordSql.Append("VALUES (@UserId, -@PayCandy, NOW(), NOW(), @CandyDesc, @Source);");
            CandyRecordParams.Add("UserId", exchange.UserId, DbType.Int64);
            CandyRecordParams.Add("PayCandy", CandyTotal, DbType.Decimal);
            CandyRecordParams.Add("CandyDesc", $"兑换认证券: {exchange.Shares.ToString()}份", DbType.String);
            CandyRecordParams.Add("Source", 87, DbType.Int32);

            //写入 果皮扣除记录 1
            StringBuilder PeelRecordSql = new StringBuilder();
            DynamicParameters PeelRecordParams = new DynamicParameters();
            PeelRecordSql.Append("INSERT INTO `user_candyp`(`userId`, `candyP`, `content`, `source`, `createdAt`, `updatedAt`) ");
            PeelRecordSql.Append("VALUES (@UserId, -@PayPeel, @PeelDesc, @Source, NOW(), NOW());");
            PeelRecordParams.Add("UserId", exchange.UserId, DbType.Int64);
            PeelRecordParams.Add("PayPeel", PeelTotal, DbType.Decimal);
            PeelRecordParams.Add("PeelDesc", $"兑换认证券: {exchange.Shares.ToString()}份", DbType.String);
            PeelRecordParams.Add("Source", 87, DbType.Int32);

            try
            {
                if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
                using (IDbTransaction transaction = context.Dapper.BeginTransaction())
                {
                    try
                    {
                        Int32 Rows = 0;
                        Rows += context.Dapper.Execute(DeductSql.ToString(), DeductParams, transaction);
                        Rows += context.Dapper.Execute(CandyRecordSql.ToString(), CandyRecordParams, transaction);
                        Rows += context.Dapper.Execute(PeelRecordSql.ToString(), PeelRecordParams, transaction);
                        if (Rows != 3) { throw new Exception("扣款失败[S]"); }
                        transaction.Commit();
                        IsSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        SystemLog.Debug(exchange.GetJson(), ex);
                    }
                }
            }
            finally
            {
                if (context.Dapper.State == ConnectionState.Open) { context.Dapper.Close(); }
            }
            #endregion

            if (IsSuccess)
            {
                var rult = await ChangeAmount(exchange.UserId, exchange.Shares, TicketModifyType.TICKET_SUBSCRIBE, false, exchange.Shares.ToString());
                if (rult == null || !rult.Success) { return rult; }
                return result;
            }
            return result.SetStatus(ErrorCode.InvalidData, "兑换认证券失败");
        }

        /// <summary>
        /// 兑换认证券
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> ExchangeTicket(TicketExchange exchange)
        {
            MyResult<Object> result = new MyResult<object>();

            if (DateTime.Now.Hour >= 22 || DateTime.Now.Hour < 9)
            {
                return result.SetStatus(ErrorCode.TimeNoOpen, $"认证券购买开放时间为每天9:00-22:00...");
            }

            String CacheLock = $"Ticket:Exchange{exchange.UserId}";
            if (RedisCache.Exists(CacheLock)) { return result.SetStatus(ErrorCode.InvalidData, "您操作太快了~"); }
            else { RedisCache.Set(CacheLock, exchange.Shares, 5); }
            if (exchange.Shares < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "认证券类型有误...");
            }
            #region 基础验证
            if (exchange.UserId < 1) { return result.SetStatus(ErrorCode.ErrorSign, "请重新登陆后,重试"); }
            await InitAccount(exchange.UserId); //初始化账户

            var UserInfo = context.Dapper.QueryFirstOrDefault<UserEntity>("SELECT * FROM `user` WHERE id = @UserId;", new { UserId = exchange.UserId });
            if (UserInfo == null)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "用户信息不存在...");
            }
            if (UserInfo.Status != 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "暂无无权利购买...");
            }
            var SellerUserInfo = context.Dapper.QueryFirstOrDefault<UserEntity>("SELECT * FROM `user` WHERE id = @UserId;", new { UserId = 1 });

            var orderNum = Gen.NewGuid20();
            //发布订单
            var insertSql = $"insert into coin_trade(tradeNumber,buyerUid,sellerUid,sellerAlipay,amount,price,totalPrice,fee,trendSide,status,coinType,dealTime,dealEndTime)values('{orderNum}',{exchange.UserId},{SellerUserInfo.Id},'{SellerUserInfo.Alipay}',{exchange.Shares},{1.5},{(exchange.Shares * 1.5)},0,'BUY',2,'RZQ','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{DateTime.Now.AddMinutes(60).ToString("yyyy-MM-dd HH:mm:ss")}');SELECT @@IDENTITY";
            var res = context.Dapper.ExecuteScalar<long>(insertSql);
            #endregion
            if (res > 0)
            {
                return result;
            }
            return result.SetStatus(ErrorCode.InvalidData, "兑换认证券失败");
        }

        /// <summary>
        /// 使用认证券
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> UseTicket(long UserId)
        {
            MyResult<Object> result = new MyResult<object>();
            if (UserId < 1) { return result.SetStatus(ErrorCode.ErrorSign, "请重新登陆后,重试"); }

            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"RealNameTicket:{UserId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                #region 订单信息判断
                var Orders = await context.Dapper.QueryAsync<OrderGames>($"select * from `order_games` where gameAppid=1 and userId={UserId}");
                var payOrder = Orders.FirstOrDefault(o => o.Status == 1);
                if (payOrder != null) { return result; }
                #endregion

                var UserInfo = context.Dapper.QueryFirstOrDefault<UserEntity>("SELECT * FROM `user` WHERE id = @UserId;", new { UserId = UserId });
                if (UserInfo == null) { return result.SetStatus(ErrorCode.ErrorSign, "请重新登陆后,重试"); }
                var ReUser = context.Dapper.QueryFirstOrDefault<UserEntity>("SELECT * FROM `user` WHERE `mobile` = @InviterMobile;", new { InviterMobile = UserInfo.InviterMobile });
                //
                var flag = false;
                //
                String SelectSqlMy = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {UserId} LIMIT 1;";
                UserAccountTicket accInfoMy = await context.Dapper.QueryFirstOrDefaultAsync<UserAccountTicket>(SelectSqlMy);
                if (accInfoMy != null) { flag = true; }
                //
                String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {ReUser.Id} LIMIT 1;";
                UserAccountTicket accInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserAccountTicket>(SelectSql);
                if (!flag)
                {
                    if (accInfo == null || accInfo.State == 0) { return result.SetStatus(ErrorCode.InvalidData, "推荐人未开启认证券功能,请去购买认证券"); }
                }
                //
                var userWId = flag ? UserId : ReUser.Id;
                MyResult<object> DeductRult = await ChangeAmount(userWId, -1, TicketModifyType.TICKET_USED, false, UserInfo.Name, "1");
                if (!DeductRult.Success || !(Boolean)DeductRult.Data)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "使用认证券失败");
                }

                var orderGame = Orders.FirstOrDefault(o => o.Status == 0);
                var orderNum = Gen.NewGuid();
                Decimal payPrice = 1.50M;
                Int32 TotalWatch = 10;
                Int32 rows = 0;

                DynamicParameters SqlParam = new DynamicParameters();
                SqlParam.Add("OrderId", orderNum, DbType.String);
                SqlParam.Add("UserId", UserId, DbType.Int64);
                SqlParam.Add("Uuid", TotalWatch, DbType.String);
                SqlParam.Add("PayAmount", payPrice, DbType.Decimal);
                SqlParam.Add("UpdatedAt", DateTime.Now, DbType.DateTime);
                SqlParam.Add("Status", 1, DbType.Int32);

                if (orderGame == null)
                {
                    StringBuilder InsertSql = new StringBuilder();
                    InsertSql.Append("INSERT INTO `order_games` ( `gameAppid`, `orderId`, `userId`, `uuid`, `realAmount`, `status`, `createdAt`, `updatedAt` ) ");
                    InsertSql.Append("VALUES ( 1, @OrderId, @UserId, @Uuid, @PayAmount,	@Status, NOW(), NOW());");

                    rows = context.Dapper.Execute(InsertSql.ToString(), SqlParam);
                    result.Data = new { TotalWatch };
                }
                else
                {
                    SqlParam.Add("Id", orderGame.Id, DbType.Int32);
                    StringBuilder UpdateSql = new StringBuilder();
                    UpdateSql.Append("UPDATE `order_games` SET `status` = @Status, `orderId` = @OrderId, `uuid` = @Uuid , `updatedAt` = @UpdatedAt WHERE `id` = @Id");
                    rows = context.Dapper.Execute(UpdateSql.ToString(), SqlParam);
                    result.Data = new { TotalWatch = 10 };
                }
                return result;
            }
            catch (Exception ex)
            {
                SystemLog.Debug("认证券异常==>", ex);
                return result.SetStatus(ErrorCode.SystemError, "发生错误[AT]");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }

        /// <summary>
        /// 认证券量化宝
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> TicketTask(long UserId)
        {
            MyResult<Object> result = new MyResult<object>();
            if (UserId < 1) { return result.SetStatus(ErrorCode.ErrorSign, "请重新登陆后,重试"); }

            String TicketLockStr = $"TicketTask:{UserId}";
            if (RedisCache.Exists(TicketLockStr))
            {
                return result.SetStatus(ErrorCode.InvalidData, "操作过于频繁，请稍后重试");
            }
            RedisCache.Set(TicketLockStr, UserId, 15);

            String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {UserId} LIMIT 1";
            TicketModel accInfo = await context.Dapper.QueryFirstOrDefaultAsync<TicketModel>(SelectSql);
            if (accInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "账户不存在"); }

            Int32 DayTotal = context.Dapper.QueryFirstOrDefault<Int32>($"SELECT COUNT(RecordId) FROM {RecordTableName} WHERE ModifyType = 3 AND TO_DAYS(NOW()) = TO_DAYS(ModifyTime) AND AccountId = @AccountId;", new { accInfo.AccountId });
            if (DayTotal >= TicketConf.TaskCount) { return result.SetStatus(ErrorCode.InvalidData, "今日量化宝已完,明天再来哦~"); }

            var Rult = await ChangeAmount(UserId, 0.10M, TicketModifyType.TICKET_TASK, false, "0.10");

            if (Rult.Success && (Boolean)Rult.Data)
            {
                result.Data = new { WatchCunt = DayTotal + 1 };
                return result;
            }
            return result.SetStatus(ErrorCode.InvalidData, "量化宝失败~");
        }

        /// <summary>
        /// 认证券记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<UserAccountTicketRecord>>> TicketRecords(QueryModel query)
        {
            MyResult<List<UserAccountTicketRecord>> result = new MyResult<List<UserAccountTicketRecord>>();
            String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {query.UserId} LIMIT 1";
            UserAccountTicket accInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserAccountTicket>(SelectSql);
            if (accInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "账户不存在"); }
            result.RecordCount = await context.Dapper.QueryFirstOrDefaultAsync<int>($"SELECT COUNT(1) AS `Count` FROM {RecordTableName} WHERE `AccountId`={accInfo.AccountId}");
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            if (query.PageIndex < 1) { query.PageIndex = 1; }
            IEnumerable<UserAccountTicketRecord> Records = await context.Dapper.QueryAsync<UserAccountTicketRecord>($"SELECT * FROM {RecordTableName} WHERE `AccountId`={accInfo.AccountId} ORDER BY RecordId DESC LIMIT {(query.PageIndex - 1) * query.PageSize},{query.PageIndex * query.PageSize}");

            result.Data = new List<UserAccountTicketRecord>();
            foreach (var item in Records)
            {
                item.AccountId = 0;
                item.ModifyDesc = String.Format(item.ModifyType.GetDescription(), item.ModifyDesc.Split(","));
                result.Data.Add(item);
            }
            return result;
        }

        /// <summary>
        /// 认证券余额变更
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <param name="useFrozen">使用冻结金额，账户金额增加时，此参数无效</param>
        /// <param name="modifyType">账户变更类型</param>
        /// <param name="Desc">描述</param>
        /// <returns></returns>
        public async Task<MyResult<object>> ChangeAmount(long userId, decimal Amount, TicketModifyType modifyType, bool useFrozen, params string[] Desc)
        {
            MyResult result = new MyResult { Data = false };
            if (Amount == 0) { return new MyResult { Data = true }; }   //账户无变动，直接返回成功
            if (Amount > 0 && useFrozen) { useFrozen = false; } //账户增加时，无法使用冻结金额
            CSRedisClientLock CacheLock = null;
            UserAccountTicket UserAccount;
            Int64 AccountId;
            String Field = String.Empty, EditSQl = String.Empty, RecordSql = String.Empty, PostChangeSql = String.Empty;
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}InitTicket_{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }

                #region 验证账户信息
                String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {userId} LIMIT 1";
                UserAccount = await context.Dapper.QueryFirstOrDefaultAsync<UserAccountTicket>(SelectSql);
                if (UserAccount == null) { return result.SetStatus(ErrorCode.InvalidData, "认证券不存在"); }
                if (Amount < 0)
                {
                    if (useFrozen)
                    {
                        if (UserAccount.Frozen < Math.Abs(Amount) || UserAccount.Balance < Math.Abs(Amount)) { return result.SetStatus(ErrorCode.InvalidData, "认证券不足[F]"); }
                    }
                    else
                    {
                        if (UserAccount.Balance < Math.Abs(Amount)) { return result.SetStatus(ErrorCode.InvalidData, "认证券不足[B]"); }
                        if ((UserAccount.Balance - UserAccount.Frozen) < Math.Abs(Amount)) { return result.SetStatus(ErrorCode.InvalidData, "可用认证券不足[F]"); }
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
                        return result.SetStatus(ErrorCode.InvalidData, "认证券变更发生错误");
                    }
                    catch (Exception ex)
                    {
                        Tran.Rollback();
                        SystemLog.Debug($"认证券账户变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                        return result.SetStatus(ErrorCode.InvalidData, "发生错误");
                    }
                    finally { if (context.Dapper.State == ConnectionState.Open) { context.Dapper.Close(); } }
                }
                #endregion
            }
            catch (Exception ex)
            {
                SystemLog.Debug($"认证券变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                return result.SetStatus(ErrorCode.InvalidData, "发生错误");
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
        }
    }
}
