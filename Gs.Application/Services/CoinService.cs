using System;
using System.Linq;
using Gs.Domain.Models;
using Gs.Domain.Configs;
using Gs.Domain.Context;
using Gs.Domain.Repository;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Gs.Core;
using Gs.Domain.Enums;
using CSRedis;
using System.Linq.Expressions;
using Gs.Domain.Entity;
using Gs.Domain.Models.Admin;
using Dapper;
using Gs.Core.Utils;
using System.Data;
using System.Text;
using Gs.Domain.Models.Dto;

namespace Gs.Application.Services
{
    public class CoinService : ICoinService
    {
        private readonly String CacheCottonLockKey = "CottonAccount:";
        private readonly String CacheCottonCoinLockKey = "CottonCoinAccount:";

        private readonly String HonorLockKey = "HonorAccount:";

        private readonly String CottonTableName = "user_account_cotton";
        private readonly String CottonRecordTableName = "user_account_cotton_record";

        private readonly String CottonCoinTableName = "user_account_cotton_coin";
        private readonly String CottonCoinRecordTableName = "user_account_cotton_coin_record";

        private readonly String HonorTableName = "user_account_honor";
        private readonly String HonorRecordTableName = "user_account_honor_record";

        private readonly WwgsContext context;
        private readonly CSRedisClient RedisCache;
        public CoinService(WwgsContext mySql, CSRedisClient cSRedis, IHonorService honor)
        {
            context = mySql;
            RedisCache = cSRedis;
        }

        public async Task<MyResult<object>> CottonExCoin(int userId, decimal cotton, string passward)
        {
            MyResult result = new MyResult();
            if (userId <= 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "sign error");
            }
            if (cotton < 0)
            {
                return result.SetStatus(ErrorCode.NoAuth, "输入异常...");
            }
            if (string.IsNullOrEmpty(passward)) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码不能为空"); }

            //用户信息
            var user = await context.Dapper.QueryFirstOrDefaultAsync<UserEntity>($"select * from user where id={userId}");
            if (SecurityUtil.MD5(passward) != user.TradePwd) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码有误"); }
            if (user == null)
            {
                return result.SetStatus(ErrorCode.NoAuth, "该用户状态异常 禁止兑换...");
            }
            if (user.Status == 2)
            {
                return result.SetStatus(ErrorCode.NoAuth, "该账户已被封禁 禁止兑换...");
            }
            // if (user.AuditState == 0)
            // {
            //     return result.SetStatus(ErrorCode.NoAuth, "该账户未实名 禁止兑换...");
            // }
            var CottonCoin = Math.Round(cotton, 4);

            // GetExchangeRate(user.Level, out decimal exRate);
            // var fee = 0;

            var totalCotton = cotton;

            if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
            using (IDbTransaction transaction = context.Dapper.BeginTransaction())
            {
                try
                {
                    //-NW
                    // var res1 = await ChangeWalletAmount(CacheCottonLockKey, CottonTableName, CottonRecordTableName, transaction, true, userId, -totalCotton, (int)CottonModifyType.EXCHANGE_CONCH, false, totalCotton.ToString(), CottonCoin.ToString(), fee.ToString());
                    var res1 = await ChangeWalletAmount(CacheCottonLockKey, CottonCoinTableName, CottonCoinRecordTableName, transaction, true, userId, -totalCotton, (int)ConchModifyType.EXCHANGE_CONCH, false, totalCotton.ToString());
                    if (res1.Code != 200)
                    {
                        transaction.Rollback();
                        SystemLog.Debug($"{userId}兑换NW失败{cotton}");
                        return result.SetStatus(ErrorCode.InvalidData, $"兑换失败..NW..{res1.Message}");
                    }

                    //+Gas
                    var res = await ChangeWalletAmount(HonorLockKey, HonorTableName, HonorRecordTableName, transaction, true, userId, CottonCoin, (int)HonorModifyType.EXCHANGE_CONCH, false, $"{CottonCoin.ToString()}");
                    if (res.Code != 200)
                    {
                        transaction.Rollback();
                        SystemLog.Debug($"{userId}兑换NW失败{cotton}");
                        return result.SetStatus(ErrorCode.InvalidData, $"兑换失败..MBE..{res1.Message}");
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    SystemLog.Debug($"{userId}兑换子币失败{cotton}_{ex.Message}");
                    return result.SetStatus(ErrorCode.InvalidData, "转出失败");
                }
                finally
                {
                    if (context.Dapper.State == ConnectionState.Open) { context.Dapper.Close(); }
                }
            }
            return result;
        }

        /// <summary>
        /// 更具等级算汇率
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="exRate"></param>
        public void GetExchangeRate(string lv, out decimal exRate)
        {
            lv = lv.ToLower().Replace("lv", "");
            switch (lv)
            {
                case "0": exRate = 0.5M; break;
                case "1": exRate = 0.28M; break;
                case "2": exRate = 0.26M; break;
                case "3": exRate = 0.23M; break;
                case "4": exRate = 0.20M; break;
                default: exRate = 0.5M; break;
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
                TempRecordSql.Append($"{modifyType} AS `ModifyType`, ");
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

        public async Task<MyResult<CoinUserAccountWallet>> FindCoinAmount(long userId)
        {
            MyResult<CoinUserAccountWallet> result = new MyResult<CoinUserAccountWallet>();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.NoAuth, "非法授权");
            }
            List<UserAccountWalletModel> userAccountWallets = new List<UserAccountWalletModel>();
            UserAccountWalletModel userAccountWalletModel;
            CoinUserAccountWallet coinUserAccountWallet = new CoinUserAccountWallet();

            var cottonSql = $"SELECT * from user_account_cotton where userId={userId}";//贡献
            var cottonCoinSql = $"SELECT * from user_account_cotton_coin where userId={userId}";
            var honorSql = $"SELECT * from user_account_honor where userId={userId}";

            // var cottonInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserAccountWallet>(cottonSql);
            var cottonCoinInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserAccountWallet>(cottonCoinSql);
            var honorInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserAccountWallet>(honorSql);
            if (cottonCoinInfo == null)
            {
                coinUserAccountWallet.Lists = userAccountWallets;
                result.Data = coinUserAccountWallet;
                return result;
            }
            int i = 1;
            if (cottonCoinInfo != null)
            {
                userAccountWalletModel = new UserAccountWalletModel();
                userAccountWalletModel.Balance = cottonCoinInfo.Balance;
                userAccountWalletModel.AccountId = cottonCoinInfo.AccountId;
                userAccountWalletModel.Frozen = cottonCoinInfo.Frozen;
                userAccountWalletModel.CoinType = "NW";
                userAccountWalletModel.Id = i;
                userAccountWallets.Add(userAccountWalletModel);
                i++;
            }
            // if (cottonInfo != null)
            // {
            //     userAccountWalletModel = new UserAccountWalletModel();
            //     userAccountWalletModel.Balance = cottonInfo.Balance;
            //     userAccountWalletModel.AccountId = cottonInfo.AccountId;
            //     userAccountWalletModel.Frozen = cottonInfo.Frozen;
            //     userAccountWalletModel.CoinType = "贡献值";
            //     userAccountWalletModel.Id = i;
            //     userAccountWallets.Add(userAccountWalletModel);
            //     i++;
            // }
            if (honorInfo != null)
            {
                userAccountWalletModel = new UserAccountWalletModel();
                userAccountWalletModel.Balance = honorInfo.Balance;
                userAccountWalletModel.AccountId = honorInfo.AccountId;
                userAccountWalletModel.Frozen = honorInfo.Frozen;
                userAccountWalletModel.CoinType = "Gas";
                userAccountWalletModel.Id = i;
                userAccountWallets.Add(userAccountWalletModel);
            }
            coinUserAccountWallet.BalanceCottonCoin = cottonCoinInfo.Balance;
            coinUserAccountWallet.FrozenCottonCoin = cottonCoinInfo.Frozen;
            coinUserAccountWallet.Lists = userAccountWallets;
            result.Data = coinUserAccountWallet;
            return result;
        }

        public async Task<MyResult<List<UserCoinRecordDto>>> CoinAccountRecord(string coinType, long accountId, long userId, int PageIndex = 1, int PageSize = 20, CottonModifyType ModifyType = CottonModifyType.ALL)
        {
            MyResult<List<UserCoinRecordDto>> result = new MyResult<List<UserCoinRecordDto>>();
            if (PageIndex < 1) { PageIndex = 1; }
            var AccountTableName = "";
            var RecordTableName = "";
            if (coinType.Equals("NW"))
            {
                AccountTableName = CottonTableName;
                RecordTableName = CottonRecordTableName;
            }
            else if (coinType.Equals("NW"))
            {

                AccountTableName = CottonCoinTableName;
                RecordTableName = CottonCoinRecordTableName;
            }
            else if (coinType.Equals("荣誉值"))
            {
                AccountTableName = HonorTableName;
                RecordTableName = HonorRecordTableName;
            }
            else
            {
                return result.SetStatus(ErrorCode.NoAuth, "类型有误...");
            }
            String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {userId} and `AccountId`={accountId} LIMIT 1";
            UserAccountWallet accInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserAccountWallet>(SelectSql);
            if (accInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "Coin不存在"); }
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
            if (ModifyType != CottonModifyType.ALL)
            {
                QueryParam.Add("ModifyType", (Int32)ModifyType, DbType.Int32);
                QueryCountSql.Append("AND `ModifyType` = @ModifyType ");
                QueryDataSql.Append("AND `ModifyType` = @ModifyType ");
            }
            QueryCountSql.Append(";");
            QueryParam.Add("PageIndex", (PageIndex - 1) * PageSize, DbType.Int32);
            QueryParam.Add("PageSize", PageSize, DbType.Int32);
            QueryDataSql.Append("ORDER BY RecordId DESC LIMIT @PageIndex,@PageSize;");
            #endregion

            result.RecordCount = await context.Dapper.QueryFirstOrDefaultAsync<int>(QueryCountSql.ToString(), QueryParam);
            result.PageCount = (result.RecordCount + PageSize - 1) / PageSize;
            var Records = await context.Dapper.QueryAsync<UserCoinRecordDto>(QueryDataSql.ToString(), QueryParam);

            result.Data = Records.ToList();
            return result;
        }
        /// <summary>
        /// NW换Gas记录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>

        public async Task<MyResult<object>> ExchangeRecord(long userId, int PageIndex = 1, int PageSize = 20, ConchModifyType ModifyType = ConchModifyType.Sys_Ex)
        {
            MyResult<object> result = new MyResult<object>();
            if (PageIndex < 1) { PageIndex = 1; }
            string AccountTableName = CottonCoinTableName;
            string RecordTableName = CottonCoinRecordTableName;

            String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {userId} LIMIT 1";
            UserAccountWallet accInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserAccountWallet>(SelectSql);
            if (accInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "Coin不存在"); }
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
            QueryParam.Add("ModifyType", (Int32)ModifyType, DbType.Int32);
            QueryCountSql.Append("AND `ModifyType` = @ModifyType ");
            QueryDataSql.Append("AND `ModifyType` = @ModifyType ");
            QueryCountSql.Append(";");
            QueryParam.Add("PageIndex", (PageIndex - 1) * PageSize, DbType.Int32);
            QueryParam.Add("PageSize", PageSize, DbType.Int32);
            QueryDataSql.Append("ORDER BY RecordId DESC LIMIT @PageIndex,@PageSize;");
            #endregion

            result.RecordCount = await context.Dapper.QueryFirstOrDefaultAsync<int>(QueryCountSql.ToString(), QueryParam);
            result.PageCount = (result.RecordCount + PageSize - 1) / PageSize;
            var Records = await context.Dapper.QueryAsync<UserCoinRecordDto>(QueryDataSql.ToString(), QueryParam);
            foreach (var item in Records)
            {
                item.ModifyDesc = String.Format(ModifyType.GetDescription(), item.ModifyDesc.Split(","));
            }
            result.Data = Records.ToList();
            return result;
        }
    }
}
