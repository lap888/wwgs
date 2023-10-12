using CSRedis;
using Dapper;
using Gs.Core;
using System;
using System.Data;
using System.Text;
using Gs.Domain.Enums;
using Gs.Domain.Models;
using Gs.Domain.Context;
using Gs.Domain.Repository;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gs.Application.Services
{
    /// <summary>
    /// NW
    /// </summary>
    public class CottonService : ICottonService
    {
        private readonly String CacheLockKey = "CottonAccount:";
        private readonly String AccountTableName = "user_account_cotton";
        private readonly String RecordTableName = "user_account_cotton_record";

        private readonly WwgsContext context;
        private readonly CSRedisClient RedisCache;
        public CottonService(WwgsContext mysql, CSRedisClient redisClient)
        {
            context = mysql;
            RedisCache = redisClient;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="Uid"></param>
        /// <returns></returns>
        public async Task Init(Int64 Uid)
        {
            CSRedisClientLock CacheLock = null;
            String InsertSql = $"INSERT INTO `{AccountTableName}` (`UserId`, `Revenue`, `Expenses`, `Balance`, `Frozen`, `ModifyTime`) VALUES ({Uid}, '0', '0', '0', '0', NOW())";
            String SelectSql = $"SELECT COUNT(1) AS `Have` FROM `{AccountTableName}` WHERE `UserId` = {Uid} LIMIT 1";
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}Init_{Uid}", 30);
                if (CacheLock == null) { return; }
                int Have = await context.Dapper.QueryFirstOrDefaultAsync<int>(SelectSql);
                if (Have != 0) { return; }
                int Row = await context.Dapper.ExecuteAsync(InsertSql);
                if (Row != 1) { return; }
                return;
            }
            catch (Exception ex)
            {
                Core.SystemLog.Debug($"初始化股权账户发生错误\r\n插入语句：{InsertSql}\r\n判断语句：{SelectSql}", ex);
                return;
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
        }

        /// <summary>
        /// MBM信息
        /// </summary>
        /// <param name="Uid"></param>
        /// <returns></returns>
        public async Task<AccountInfo> Info(Int64 Uid)
        {
            String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {Uid} LIMIT 1";
            AccountInfo accInfo = await context.Dapper.QueryFirstOrDefaultAsync<AccountInfo>(SelectSql);
            if (accInfo == null)
            {
                await Init(Uid);
                accInfo = await context.Dapper.QueryFirstOrDefaultAsync<AccountInfo>(SelectSql);
            }
            accInfo.Usable = accInfo.Balance - accInfo.Frozen;
            return accInfo;
        }

        /// <summary>
        /// NW
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public async Task<Boolean> Frozen(Int64 Uid, Decimal Amount)
        {
            CSRedisClientLock CacheLock = null;
            String UpdateSql = $"UPDATE `{AccountTableName}` SET `Frozen`=`Frozen`+{Amount} WHERE `UserId`={Uid} AND (`Balance`-`Frozen`)>={Amount} AND (`Frozen`+{Amount})>=0";
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}Init_{Uid}", 30);
                if (CacheLock == null) { return false; }
                int Row = await context.Dapper.ExecuteAsync(UpdateSql);
                if (Row != 1) { return false; }
                return true;
            }
            catch (Exception ex)
            {
                SystemLog.Debug($"账户余额冻结操作发生错误,\r\n{UpdateSql}", ex);
                return false;
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
        }

        /// <summary>
        /// MBM记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<AccountRecord>>> Records(QueryModel query)
        {
            MyResult<List<AccountRecord>> result = new MyResult<List<AccountRecord>>()
            {
                Data = new List<AccountRecord>()
            };
            String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {query.UserId} LIMIT 1";
            AccountInfo accInfo = await context.Dapper.QueryFirstOrDefaultAsync<AccountInfo>(SelectSql);
            if (accInfo == null) { return result; }
            result.RecordCount = await context.Dapper.QueryFirstOrDefaultAsync<int>($"SELECT COUNT(1) AS `Count` FROM {RecordTableName} WHERE `AccountId`={accInfo.AccountId}");
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            if (query.PageIndex < 1) { query.PageIndex = 1; }
            IEnumerable<AccountRecord> Records = await context.Dapper.QueryAsync<AccountRecord>($"SELECT * FROM {RecordTableName} WHERE `AccountId`={accInfo.AccountId} ORDER BY RecordId DESC LIMIT {(query.PageIndex - 1) * query.PageSize},{query.PageIndex * query.PageSize}");

            result.Data = new List<AccountRecord>();
            foreach (var item in Records)
            {
                CottonModifyType ModifyType = (CottonModifyType)item.ModifyType;
                item.ModifyDesc = String.Format(ModifyType.GetDescription(), item.ModifyDesc.Split(","));
                result.Data.Add(item);
            }
            return result;
        }

        /// <summary>
        /// MBM变更
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="Amount"></param>
        /// <param name="UseFrozen">使用冻结金额，账户金额增加时，此参数无效</param>
        /// <param name="ModifyType">账户变更类型</param>
        /// <param name="Desc">描述</param>
        /// <returns></returns>
        public async Task<Boolean> ChangeAmount(Int64 Uid, Decimal Amount, CottonModifyType ModifyType, Boolean UseFrozen, params string[] Desc)
        {
            if (Amount == 0) { return false; }   //账户无变动，直接返回成功
            if (Amount > 0 && UseFrozen) { UseFrozen = false; } //账户增加时，无法使用冻结金额
            CSRedisClientLock CacheLock = null;
            AccountInfo UserAccount;
            Int64 AccountId;
            String Field = String.Empty, EditSQl = String.Empty, RecordSql = String.Empty, PostChangeSql = String.Empty;
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}COTTON_{Uid}", 30);
                if (CacheLock == null) { return false; }

                #region 验证账户信息
                UserAccount = await Info(Uid);
                if (UserAccount == null) { return false; }
                if (Amount < 0)
                {
                    if (UseFrozen)
                    {
                        if (UserAccount.Frozen < Math.Abs(Amount) || UserAccount.Balance < Math.Abs(Amount)) { return false; }
                    }
                    else
                    {
                        if (UserAccount.Balance < Math.Abs(Amount)) { return false; }
                        if ((UserAccount.Balance - UserAccount.Frozen) < Math.Abs(Amount)) { return false; }
                    }
                }
                #endregion

                AccountId = UserAccount.AccountId;
                Field = Amount > 0 ? "Revenue" : "Expenses";

                EditSQl = $"UPDATE `{AccountTableName}` SET `Balance`=`Balance`+{Amount},{(UseFrozen ? $"`Frozen`=`Frozen`+{Amount}," : "")}`{Field}`=`{Field}`+{Math.Abs(Amount)},`ModifyTime`=NOW() WHERE `AccountId`={AccountId} {(UseFrozen ? $"AND (`Frozen`+{Amount})>=0;" : $"AND(`Balance`-`Frozen`+{Amount}) >= 0;")}";

                PostChangeSql = $"IFNULL((SELECT `PostChange` FROM `{RecordTableName}` WHERE `AccountId`={AccountId} ORDER BY `RecordId` DESC LIMIT 1),0)";
                StringBuilder TempRecordSql = new StringBuilder($"INSERT INTO `{RecordTableName}` ");
                TempRecordSql.Append("( `AccountId`, `PreChange`, `Incurred`, `PostChange`, `ModifyType`, `ModifyDesc`, `ModifyTime` ) ");
                TempRecordSql.Append($"SELECT {AccountId} AS `AccountId`, ");
                TempRecordSql.Append($"{PostChangeSql} AS `PreChange`, ");
                TempRecordSql.Append($"{Amount} AS `Incurred`, ");
                TempRecordSql.Append($"{PostChangeSql}+{Amount} AS `PostChange`, ");
                TempRecordSql.Append($"{(int)ModifyType} AS `ModifyType`, ");
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
                            return true;
                        }
                        Tran.Rollback();
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Tran.Rollback();
                        Core.SystemLog.Debug($"股权账户变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                        return false;
                    }
                    finally { if (context.Dapper.State == ConnectionState.Open) { context.Dapper.Close(); } }
                }
                #endregion
            }
            catch (Exception ex)
            {
                SystemLog.Debug($"股权变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                return false;
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
        }

    }
}
