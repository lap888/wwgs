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
using Newtonsoft.Json;
using Gs.Core.Utils;
using Gs.Domain.Entity;
using System.Linq;
using Gs.Domain.Models.Dto;
using Gs.Core.Extensions;

namespace Gs.Application.Services
{
    /// <summary>
    /// 活跃度
    /// </summary>
    public class AmbmService : IAmbmService
    {

        private readonly WwgsContext context;
        private readonly CSRedisClient RedisCache;
        private readonly ITicketService TicketService;
        private readonly IHonorService HonorSub;
        private readonly ICottonService CottonSub;
        private readonly IIntegralService IntegralSub;
        private readonly IWalletService WalletSub;
        private readonly IConchService ConchSub;

        public AmbmService(IHonorService honorService, ICottonService cottonService, IIntegralService integralService, IWalletService walletService, IConchService conchService, WwgsContext mysql, CSRedisClient redisClient, ITicketService ticketService)
        {
            context = mysql;
            RedisCache = redisClient;
            TicketService = ticketService;
            HonorSub = honorService;
            CottonSub = cottonService;
            IntegralSub = integralService;
            WalletSub = walletService;
            ConchSub = conchService;
        }

        public async Task<MyResult<object>> DoMbm(string projectId, string inviderCode, string mobile, string secretKey, string publicKey, string device)
        {
            MyResult result = new MyResult();
            if (!ProcessSqlStr(projectId))
            {
                return result.SetStatus(ErrorCode.InvalidData, "数据非法...");
            }
            if (!ProcessSqlStr(inviderCode))
            {
                return result.SetStatus(ErrorCode.InvalidData, "数据非法...");
            }
            if (!ProcessSqlStr(mobile))
            {
                return result.SetStatus(ErrorCode.InvalidData, "数据非法...");
            }
            if (!ProcessSqlStr(secretKey))
            {
                return result.SetStatus(ErrorCode.InvalidData, "数据非法...");
            }
            if (!ProcessSqlStr(publicKey))
            {
                return result.SetStatus(ErrorCode.InvalidData, "数据非法...");
            }
            if (!ProcessSqlStr(device))
            {
                return result.SetStatus(ErrorCode.InvalidData, "数据非法...");
            }
            if (projectId != "10001")
            {
                return result.SetStatus(ErrorCode.InvalidData, "项目ID非法...");
            }
            //查软件用户 设备 账户资产
            var userSql = $"select * from a_user where secretKey='{secretKey}' and `publicKey`='{publicKey}'";
            var userInfo = context.Dapper.QueryFirstOrDefault<Auser>(userSql);
            if (userInfo == null)
            {
                return result.SetStatus(ErrorCode.NoAuth, "请联系软件作者购买卡密...");
            }
            if (device != userInfo.Device)
            {
                return result.SetStatus(ErrorCode.NoAuth, "设备已被绑定...");
            }
            //短信剩余条数
            String MsgCacheLockKey = "MsgActiveAccount:";
            String MsgAccountTableName = "a_user_account_msg";
            String MsgRecordTableName = "a_user_account_msg_record";
            var info = await Info(userInfo.Id, MsgAccountTableName, MsgCacheLockKey);
            if (info.Balance <= 0)
            {
                return result.SetStatus(ErrorCode.NoAuth, "请联系软件作者购买短信...");
            }
            //实名剩余次数
            String AuthCacheLockKey = "AuthActiveAccount:";
            String AuthAccountTableName = "a_user_account_auth";
            String AuthRecordTableName = "a_user_account_auth_record";
            var infoAuth = await Info(userInfo.Id, AuthAccountTableName, AuthCacheLockKey);
            if (infoAuth.Balance <= 0)
            {
                return result.SetStatus(ErrorCode.NoAuth, "请联系软件作者购买实名...");
            }
            SignUpDto signUpDto = new SignUpDto();
            signUpDto.Mobile = mobile;
            signUpDto.InvitationCode = inviderCode;
            signUpDto.Password = "123456";

            try
            {
                //注册 实名
                var signUp = await SignUp(signUpDto);
                if (signUp.Code != 200)
                {
                    return result.SetStatus(ErrorCode.NoAuth, signUp.Message);
                }
                //扣软件短信次数
                await ChangeAmount(MsgAccountTableName, MsgRecordTableName, MsgCacheLockKey, userInfo.Id, -1, HonorModifyType.EXCHANGE_CONCH, false, "");
                //扣软件实名次数
                await ChangeAmount(AuthAccountTableName, AuthRecordTableName, AuthCacheLockKey, userInfo.Id, -1, HonorModifyType.EXCHANGE_MINER, false, "");
                result.Data = new { MesCount = info.Balance, AuthCount = info.Balance };
            }
            catch (System.Exception ex)
            {
                return result.SetStatus(ErrorCode.NoAuth, $"system error...{ex}");
            }
            return result;
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> SignUp(SignUpDto model)
        {
            MyResult result = new MyResult();

            if (string.IsNullOrEmpty(model.Mobile)) { return result.SetStatus(ErrorCode.InvalidData, "手机号不能为空"); }
            if (string.IsNullOrEmpty(model.InvitationCode)) { return result.SetStatus(ErrorCode.InvalidData, "邀请码不能为空"); }

            if (string.IsNullOrEmpty(model.Password)) { return result.SetStatus(ErrorCode.InvalidData, "密码不能为空"); }

            Int64 IsHave = context.UserEntity.Where(item => item.Mobile == model.Mobile).Select(item => item.Id).FirstOrDefault();
            if (IsHave > 0) { return result.SetStatus(ErrorCode.InvalidData, "手机号已注册"); }

            UserEntity InvitationUser = context.UserEntity.FirstOrDefault(item => item.Rcode == model.InvitationCode);
            if (InvitationUser == null) { return result.SetStatus(ErrorCode.InvalidData, "邀请码错误"); }
            //查邀请人是否有券
            var tick = await TicketService.TicketInfo(InvitationUser.Id);
            if (tick.Data.Balance <= 0)
            {
                return result.SetStatus(ErrorCode.NoAuth, "邀请人没有新人券...");
            }


            var enPassword = SecurityUtil.MD5(model.Password);

            int insertUser = 0;
            String InvitationCode = Gen.StrRandom(8, true);
            if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
            using (IDbTransaction transaction = context.Dapper.BeginTransaction())
            {
                try
                {
                    #region 写入会员信息
                    StringBuilder InsertSql = new StringBuilder();
                    InsertSql.Append("INSERT INTO `user`(`name`, `rcode`, `mobile`, `inviterMobile`, `password`, `tradePwd`, `passwordSalt`, `uuid`, `level`,`remark`) ");
                    InsertSql.Append("VALUES (@Nick, @Rcode, @Mobile, @InviterMobile, @Password, @TradePwd, @PasswordSalt, @Uuid, @Level,'0');select @@IDENTITY");

                    DynamicParameters InsertParam = new DynamicParameters();
                    InsertParam.Add("Nick", InvitationCode, DbType.String);
                    InsertParam.Add("Rcode", model.Mobile, DbType.String);
                    InsertParam.Add("Mobile", model.Mobile, DbType.String);
                    InsertParam.Add("InviterMobile", InvitationUser.Mobile, DbType.String);
                    InsertParam.Add("Password", SecurityUtil.MD5(model.Password), DbType.String);
                    InsertParam.Add("TradePwd", SecurityUtil.MD5("888888"), DbType.String);
                    InsertParam.Add("PasswordSalt", "", DbType.String);
                    InsertParam.Add("Uuid", Guid.NewGuid().ToString("N"), DbType.String);
                    InsertParam.Add("Level", "lv0", DbType.String);

                    insertUser = context.Dapper.ExecuteScalar<Int32>(InsertSql.ToString(), InsertParam, transaction);
                    if (insertUser <= 0)
                    {
                        transaction.Rollback();
                        return result.SetStatus(ErrorCode.InvalidData, "注册失败");
                    }
                    #endregion

                    var rows = context.Dapper.Execute($"INSERT INTO `user_ext`(`userId`) values({insertUser})", null, transaction);
                    if (rows != 1)
                    {
                        transaction.Rollback();
                        return result.SetStatus(ErrorCode.InvalidData, "注册失败");
                    }
                    transaction.Commit();
                    result.Data = insertUser;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    SystemLog.Debug("注册异常", ex);
                    return result.SetStatus(ErrorCode.InvalidData, "手机号异常 联系管理员");
                }
                finally { if (context.Dapper.State == ConnectionState.Open) { context.Dapper.Close(); } }
            }

            #region 发送认证信息
            long ParentId = InvitationUser.Id;
            if (ParentId <= 0) { ParentId = 0; }
            try
            {
                //使用新人券
                var useTick = await TicketService.UseTicket(insertUser);
                if (useTick.Code != 200)
                {
                    return result.SetStatus(ErrorCode.NoAuth, useTick.Message);
                }
                //初始化账户
                await WalletSub.InitWalletAccount(insertUser);
                await CottonSub.Init(insertUser);
                await ConchSub.Init(insertUser);
                await IntegralSub.Init(insertUser);
                await HonorSub.Init(insertUser);
                var c = RedisCache.Publish("MEMBER_REGISTER", JsonConvert.SerializeObject(new { MemberId = insertUser, ParentId = ParentId }));
                if (c == 0) { SystemLog.Warn("注册消息订阅失败"); }
                var authInfo = Authentication(insertUser, InvitationCode);
                if (authInfo.Code != 200)
                {
                    return result.SetStatus(ErrorCode.InvalidData, authInfo.Message);
                }
            }
            catch (Exception ex) { SystemLog.Debug("注册消息订阅失败", ex); }
            #endregion
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public MyResult<object> Authentication(int userId, string userName)
        {
            MyResult result = new MyResult();
            if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
            using (IDbTransaction transaction = context.Dapper.BeginTransaction())
            {
                try
                {
                    StringBuilder Sql = new StringBuilder();
                    //修改实名状态
                    Sql.AppendLine($"update `user` set `auditState`=2,`golds`=(`golds`+50),`level`='lv0',`alipayUid`='' where id = {userId};");
                    var SqlString = Sql.ToString();
                    context.Dapper.Execute(SqlString, null, transaction);

                    result.Data = new { Golds = 50, Level = "lv0", CandyP = 2 };
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    SystemLog.Debug("实名认证", ex);
                    transaction.Rollback();
                    return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                }
            }
            context.Dapper.Close();

            try
            {
                long c = RedisCache.Publish("MEMBER_CERTIFIED", JsonConvert.SerializeObject(new { MemberId = userId, Nick = userName }));
            }
            catch (Exception ex)
            {
                SystemLog.Debug("实名认证", ex);
                return result;
            }
            return result;
        }


        public MyResult<object> LoginMbm(string name, string pwd, string device)
        {
            MyResult result = new MyResult();
            if (!ProcessSqlStr(name))
            {
                return result.SetStatus(ErrorCode.InvalidData, "数据非法...");
            }
            if (!ProcessSqlStr(pwd))
            {
                return result.SetStatus(ErrorCode.InvalidData, "数据非法...");
            }
            if (!ProcessSqlStr(device))
            {
                return result.SetStatus(ErrorCode.InvalidData, "数据非法...");
            }
            //获取用户信息
            var userSql = $"select * from a_user where name='{name}' and `publicKey`='{pwd}'";
            var userInfo = context.Dapper.QueryFirstOrDefault<Auser>(userSql);
            if (userInfo == null)
            {
                return result.SetStatus(ErrorCode.NoAuth, "请联系软件作者购买卡密...");
            }
            if (!string.IsNullOrEmpty(userInfo.Device) && device != userInfo.Device)
            {
                return result.SetStatus(ErrorCode.NoAuth, "设备已被绑定...");
            }
            //更新设备绑定
            var updateSql = $"update `a_user` set `device`='{device}' where id={userInfo.Id}";
            context.Dapper.Execute(updateSql);
            result.Data = userInfo;
            return result;
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

        //操作
        /// <summary>
        /// 获取账户余额
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="AccountTableName"></param>
        /// <returns></returns>
        public async Task<AccountInfo> Info(Int64 Uid, string AccountTableName, string CacheLockKey)
        {
            String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {Uid} LIMIT 1";
            AccountInfo accInfo = await context.Dapper.QueryFirstOrDefaultAsync<AccountInfo>(SelectSql);
            if (accInfo == null)
            {
                await Init(Uid, AccountTableName, CacheLockKey);
                accInfo = await context.Dapper.QueryFirstOrDefaultAsync<AccountInfo>(SelectSql);
            }
            accInfo.Usable = accInfo.Balance - accInfo.Frozen;
            return accInfo;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="Uid"></param>
        /// <returns></returns>
        public async Task Init(Int64 Uid, string AccountTableName, string CacheLockKey)
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
                Core.SystemLog.Debug($"初始化账户发生错误\r\n插入语句：{InsertSql}\r\n判断语句：{SelectSql}", ex);
                return;
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
        }

        /// <summary>
        /// 冻结荣耀值
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public async Task<Boolean> Frozen(Int64 Uid, Decimal Amount, string AccountTableName, string CacheLockKey)
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
        /// 荣耀值变更
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="Amount"></param>
        /// <param name="UseFrozen">使用冻结金额，账户金额增加时，此参数无效</param>
        /// <param name="ModifyType">账户变更类型</param>
        /// <param name="Desc">描述</param>
        /// <returns></returns>
        public async Task<Boolean> ChangeAmount(string AccountTableName, string RecordTableName, string CacheLockKey, int Uid, Decimal Amount, HonorModifyType ModifyType, Boolean UseFrozen, params string[] Desc)
        {
            if (Amount == 0) { return false; }   //账户无变动，直接返回成功
            if (Amount > 0 && UseFrozen) { UseFrozen = false; } //账户增加时，无法使用冻结金额
            CSRedisClientLock CacheLock = null;
            AccountInfo UserAccount;
            Int64 AccountId;
            String Field = String.Empty, EditSQl = String.Empty, RecordSql = String.Empty, PostChangeSql = String.Empty;
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}HONOR_{Uid}", 30);
                if (CacheLock == null) { return false; }

                #region 验证账户信息
                UserAccount = await Info(Uid, AccountTableName, CacheLockKey);
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
                        Core.SystemLog.Debug($"账户变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                        return false;
                    }
                    finally { if (context.Dapper.State == ConnectionState.Open) { context.Dapper.Close(); } }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Core.SystemLog.Debug($"变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                return false;
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> List(QueryTradeOrder query)
        {
            MyResult result = new MyResult();
            var sql = $"select au.id,au.name,au.`publicKey`,au.`device`,aca.`Balance` authB,acm.`Balance` msgB from `a_user` au left join `a_user_account_auth` aca on au.`id`=aca.`UserId` left join `a_user_account_msg` acm on au.id=acm.`UserId`";
            var res = (await context.Dapper.QueryAsync(sql)).AsQueryable().Pages(query.PageIndex, query.PageSize, out int count, out int pageCount);
            result.PageCount = pageCount;
            result.RecordCount = count;
            result.Data = res;
            return result;
        }

        public async Task<MyResult<object>> AddUser(string name, string pubkey)
        {
            MyResult result = new MyResult();
            if (!ProcessSqlStr(name))
            {
                return result.SetStatus(ErrorCode.InvalidData, "数据非法...");
            }
            if (!ProcessSqlStr(pubkey))
            {
                return result.SetStatus(ErrorCode.InvalidData, "数据非法...");
            }
            var sKey = Gen.StrRandom(28, true);
            var sql = $"INSERT INTO `a_user` (`name`, `publicKey`, `secretKey`) VALUES ('{name}', '{pubkey}', '{sKey}');";
            await context.Dapper.ExecuteAsync(sql);
            return result;
        }

        public async Task<MyResult<object>> AddAuth(int userId, decimal amount)
        {
            MyResult result = new MyResult();
            String AuthCacheLockKey = "AuthActiveAccount:";
            String AuthAccountTableName = "a_user_account_auth";
            String AuthRecordTableName = "a_user_account_auth_record";
            await ChangeAmount(AuthAccountTableName, AuthRecordTableName, AuthCacheLockKey, userId, amount, HonorModifyType.MEMBER_REAL_NAME, false, "");
            return result;
        }

        public async Task<MyResult<object>> AddMsg(int userId, decimal amount)
        {
            MyResult result = new MyResult();
            String MsgCacheLockKey = "MsgActiveAccount:";
            String MsgAccountTableName = "a_user_account_msg";
            String MsgRecordTableName = "a_user_account_msg_record";

            await ChangeAmount(MsgAccountTableName, MsgRecordTableName, MsgCacheLockKey, userId, amount, HonorModifyType.MEMBER_REAL_NAME, false, "");
            return result;
        }
    }
    public class Auser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PublicKey { get; set; }
        public string SecretKey { get; set; }
        public string Device { get; set; }
    }
}
