using CSRedis;
using Dapper;
using Gs.Application.Utils;
using Gs.Core;
using Gs.Core.Extensions;
using Gs.Core.Utils;
using Gs.Domain.Configs;
using Gs.Domain.Context;
using Gs.Domain.Entity;
using Gs.Domain.Entitys;
using Gs.Domain.Enums;
using Gs.Domain.Models;
using Gs.Domain.Models.Admin;
using Gs.Domain.Models.Dto;
using Gs.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gs.Application.Services
{
    public class UserSerivce : IUserSerivce
    {
        private readonly string CacheCottonCoinLockKey = "CacheCottonCoinLockKey:";
        private readonly String CottonCoinTableName = "user_account_cotton_coin";
        private readonly String CottonCoinRecordTableName = "user_account_cotton_coin_record";

        private readonly string CacheHonorLockKey = "CacheHonorLockKey:";
        private readonly String HonorTableName = "user_account_honor";
        private readonly String HonorRecordTableName = "user_account_honor_record";
        private readonly HttpClient Client;
        private readonly IAlipay AlipaySub;
        private readonly IWePayPlugin WePaySub;
        private readonly IHonorService HonorSub;
        private readonly ICottonService CottonSub;
        private readonly IIntegralService IntegralSub;
        private readonly IWalletService WalletSub;
        private readonly IConchService ConchSub;
        private readonly WwgsContext context;
        private readonly CSRedisClient RedisCache;
        private readonly Models.AppSetting AppSettings;
        private readonly ITicketService TicketSub;
        public UserSerivce(ITicketService ticketSub, WwgsContext mySql, CSRedisClient redisClient, IHttpClientFactory factory, IAlipay alipay, IHonorService honorService, IConchService conchService,
            ICottonService cottonService, IIntegralService integralService, IWalletService walletService, IWePayPlugin wePay, IOptionsMonitor<Models.AppSetting> monitor)
        {
            AlipaySub = alipay;
            WePaySub = wePay;
            context = mySql;
            RedisCache = redisClient;
            HonorSub = honorService;
            CottonSub = cottonService;
            IntegralSub = integralService;
            WalletSub = walletService;
            ConchSub = conchService;
            AppSettings = monitor.CurrentValue;
            TicketSub = ticketSub;
            Client = factory.CreateClient("JPushSMS");
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> SignUp(SignUpDto model)
        {
            MyResult result = new MyResult();

            MyResult<MsgDto> VerifyRult = await CheckVcode(new ConfirmVcode() { Mobile = model.Mobile, MsgId = model.MsgId, Vcode = model.VerifyCode });

            if (!VerifyRult.Data.Is_Valid) { return result.SetStatus(ErrorCode.InvalidData, "验证码错误"); }

            if (string.IsNullOrEmpty(model.Mobile)) { return result.SetStatus(ErrorCode.InvalidData, "手机号不能为空"); }
            if (string.IsNullOrEmpty(model.InvitationCode)) { return result.SetStatus(ErrorCode.InvalidData, "邀请码不能为空"); }

            if (string.IsNullOrEmpty(model.Password)) { return result.SetStatus(ErrorCode.InvalidData, "密码不能为空"); }

            Int64 IsHave = context.UserEntity.Where(item => item.Mobile == model.Mobile).Select(item => item.Id).FirstOrDefault();
            if (IsHave > 0) { return result.SetStatus(ErrorCode.InvalidData, "手机号已注册"); }

            UserEntity InvitationUser = context.UserEntity.FirstOrDefault(item => item.Rcode == model.InvitationCode);
            if (InvitationUser == null) { return result.SetStatus(ErrorCode.InvalidData, "邀请码错误"); }

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
                    InsertSql.Append("INSERT INTO `user`(`name`, `rcode`, `mobile`, `inviterMobile`, `password`, `tradePwd`, `passwordSalt`, `uuid`, `level`) ");
                    InsertSql.Append("VALUES (@Nick, @Rcode, @Mobile, @InviterMobile, @Password, @TradePwd, @PasswordSalt, @Uuid, @Level);select @@IDENTITY");

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
                    result.Data = true;
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
                //初始化账户
                await WalletSub.InitWalletAccount(insertUser);
                await CottonSub.Init(insertUser);
                await ConchSub.Init(insertUser);
                await IntegralSub.Init(insertUser);
                await HonorSub.Init(insertUser);
                var c = RedisCache.Publish("MEMBER_REGISTER", JsonConvert.SerializeObject(new { MemberId = insertUser, ParentId = ParentId }));
                if (c == 0) { SystemLog.Warn("注册消息订阅失败"); }
            }
            catch (Exception ex) { SystemLog.Debug("注册消息订阅失败", ex); }
            #endregion
            return result;
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MyResult<object> Login(YoyoUserDto model)
        {
            MyResult result = new MyResult();
            if (model == null) { return result.SetStatus(ErrorCode.NotFound, "输入非法"); }
            if (string.IsNullOrEmpty(model.Mobile)) { return result.SetStatus(ErrorCode.NotFound, "手机号不能为空"); }
            if (string.IsNullOrEmpty(model.Password)) { return result.SetStatus(ErrorCode.NotFound, "密码不能为空"); }
            if (!ProcessSqlStr(model.Mobile)) { return result.SetStatus(ErrorCode.InvalidData, "非法操作"); }

            var userSql = $"select u.*,ai.`trueName` from (SELECT user.id,user.auditState,ctime,name,mobile,user.status,password,avatarUrl,golds,inviterMobile,user.uuid,level,alipay,IFNULL(og.status,0) isPay FROM user left join order_games og on user.id=og.userId and og.gameAppid=1 WHERE mobile='{model.Mobile}') u left join `authentication_infos` ai on u.`id`=ai.`userId`";
            var user = context.Dapper.QueryFirstOrDefault<UserEntity>(userSql);

            if (user == null) { return result.SetStatus(ErrorCode.NotFound, "该账户未注册"); }
            if (user.Status != 0) { return result.SetStatus(ErrorCode.Forbidden, "该账户已被封禁,请联系管理员"); }
            var enPassword = SecurityUtil.MD5(model.Password);
            if (enPassword != user.Password) { return result.SetStatus(ErrorCode.InvalidPassword, "密码错误"); }

            model.DeviceName = "";
            //登陆设备绑定
            // var deviceInfo = context.Dapper.QueryFirstOrDefault<LoginHistory>($"SELECT * FROM login_history WHERE uniqueId='{model.UniqueID}' or `mobile`='{model.Mobile}' LIMIT 1");
            // if (deviceInfo == null)
            // {
            //     context.Dapper.Execute("INSERT INTO login_history(userId, mobile, uniqueId, systemName, systemVersion, deviceName, appVersion, ctime, utime) VALUES(@Id,@Mobile,@UniqueID,@SystemName,@SystemVersion,@DeviceName,@Version,NOW(),NOW())", new { user.Id, model.Mobile, model.UniqueID, model.SystemName, model.SystemVersion, model.DeviceName, model.Version });
            // }
            // else
            // {
            //     if (model.Mobile == deviceInfo.Mobile && (model.UniqueID == deviceInfo.UniqueId || deviceInfo.UniqueId == "0"))
            //     {
            //         //更新登陆时间
            //         context.Dapper.Execute($"UPDATE login_history SET utime=NOW(),uniqueId=@UniqueID WHERE mobile=@Mobile", new { model.UniqueID, model.Mobile });
            //     }
            //     else
            //     {
            //         SystemLog.Info(model.GetJson());
            //         return result.SetStatus(ErrorCode.InvalidData, "此设备已被其他用户绑定");
            //     }
            // }
            #region 更新地理位置
            if (model.Lat > 0 && model.Lng > 0)
            {
                try
                {
                    UserLocations UserLocation = context.Dapper.QueryFirstOrDefault<UserLocations>("SELECT * FROM user_locations WHERE UserId = @UserId;", new { UserId = user.Id });
                    DynamicParameters LocationParam = new DynamicParameters();
                    LocationParam.Add("UserId", user.Id, DbType.Int64);
                    LocationParam.Add("Latitude", model.Lat, DbType.Decimal);
                    LocationParam.Add("Longitude", model.Lng, DbType.Decimal);
                    LocationParam.Add("Province", model.Province, DbType.String);
                    LocationParam.Add("ProvinceCode", model.ProvinceCode, DbType.String);
                    LocationParam.Add("City", model.City, DbType.String);
                    LocationParam.Add("CityCode", model.CityCode, DbType.String);
                    LocationParam.Add("Area", model.Area, DbType.String);
                    LocationParam.Add("AreaCode", model.AreaCode, DbType.String);
                    LocationParam.Add("CreateAt", DateTime.Now, DbType.DateTime);
                    LocationParam.Add("UpdateAt", DateTime.Now, DbType.DateTime);
                    if (UserLocation == null)
                    {
                        StringBuilder InsertSql = new StringBuilder();
                        InsertSql.Append("INSERT INTO `user_locations`(`userId`, `latitude`, `longitude`, `province`, `provinceCode`, `city`, `cityCode`, `area`, `areaCode`, `createdAt`, `updatedAt`) ");
                        InsertSql.Append("VALUES (@UserId, @Latitude, @Longitude, @Province, @ProvinceCode, @City, @CityCode, @Area, @AreaCode, @CreateAt, @UpdateAt);");
                        context.Dapper.Execute(InsertSql.ToString(), LocationParam);
                        //更新 城内人数
                        context.Dapper.Execute("UPDATE city_earnings SET People = People + 1 WHERE CityNo = @CityNo;", new { CityNo = model.CityCode });
                    }
                    else
                    {
                        LocationParam.Add("Id", UserLocation.Id, DbType.Int64);
                        StringBuilder UpdateSql = new StringBuilder();
                        UpdateSql.Append("UPDATE `user_locations` SET `latitude` = @Latitude, `longitude` = @Longitude, `province` = @Province, ");
                        UpdateSql.Append("`provinceCode` = @ProvinceCode, `city` = @City, `cityCode` = @CityCode, `area` = @Area, `areaCode` = @AreaCode, `updatedAt` = @UpdateAt ");
                        UpdateSql.Append("WHERE `id` = @Id;");
                        context.Dapper.Execute(UpdateSql.ToString(), LocationParam);
                    }
                }
                catch (Exception ex)
                {
                    SystemLog.Debug(model.GetJson(), ex);
                }
            }
            #endregion
            TokenModel tokenModel = new TokenModel();
            tokenModel.Id = (int)user.Id;
            tokenModel.Mobile = user.Mobile;
            tokenModel.Code = "";
            tokenModel.Source = SourceType.Android;

            var enToken = AuthToken.SetToken(tokenModel);
            user.Rcode = user.Rcode == null ? "0" : user.Rcode;

            user.AvatarUrl = Constants.CosUrl + user.AvatarUrl ?? "images/avatar/default/1.png";

            result.Data = new
            {
                User = user,
                Token = enToken
            };

            return result;
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> SendVcode(SendVcode model)
        {
            MyResult result = new MyResult();
            try
            {
                if (model == null)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "参数异常");
                }
                if (!DataValidUtil.IsMobile(model.Mobile))
                {
                    return result.SetStatus(ErrorCode.NotFound, "手机号无效");
                }
                if (string.IsNullOrEmpty(model.Type))
                {
                    return result.SetStatus(ErrorCode.NotFound, "类型异常");
                }

                DynamicParameters QueryParam = new DynamicParameters();
                QueryParam.Add("Mobile", model.Mobile, DbType.String);
                UserEntity UserInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserEntity>("SELECT status FROM user WHERE mobile= @Mobile;", QueryParam);

                switch (model.Type)
                {
                    case "signIn":
                        if (UserInfo != null) { return result.SetStatus(ErrorCode.NotFound, "该账户已经注册"); }
                        break;
                    case "update":
                        if (UserInfo == null) { return result.SetStatus(ErrorCode.NotFound, "该账户未注册"); }
                        if (UserInfo.Status == 2) { return result.SetStatus(ErrorCode.Forbidden, "该账号违规,请联系管理员"); }
                        break;
                    case "resetPassword":
                        if (UserInfo == null) { return result.SetStatus(ErrorCode.NotFound, "该账户未注册"); }
                        if (UserInfo.Status == 2) { return result.SetStatus(ErrorCode.Forbidden, "该账号违规,请联系管理员"); }
                        break;
                    case "unbind":
                        if (UserInfo == null) { return result.SetStatus(ErrorCode.NotFound, "该账户未注册"); }
                        if (UserInfo.Status == 2) { return result.SetStatus(ErrorCode.Forbidden, "该账号违规,请联系管理员"); }
                        break;
                    default:
                        break;
                }

                UserVcodes code = context.Dapper.QueryFirstOrDefault<UserVcodes>($"SELECT createdAt FROM user_vcodes WHERE mobile = @Mobile ORDER BY id DESC LIMIT 1;", QueryParam);

                if (code != null && code.CreatedAt > DateTime.Now.AddMinutes(-5)) { return result.SetStatus(ErrorCode.InvalidData, "验证码有效时长为10分钟，无需重新发送"); }

                MyResult<MsgDto> res = await CommonSendVcode2(model);
                if (res.Data.Msg_Id == null) { return result.SetStatus(ErrorCode.SystemError, res.Data.Error.Message); }
                result.Data = new { msgId = res.Data.Msg_Id };

            }
            catch (Exception ex)
            {
                SystemLog.Debug($"录入信息非法: userId={model.GetJson()}\r\n error={ex.Message}", ex);
                return result.SetStatus(ErrorCode.NotFound, "录入信息非法");
            }
            return result;
        }

        /// <summary>
        /// 刷脸认证效验
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> IsFaceAuth(AuthenticationDto model, int UserId)
        {
            MyResult<object> Rult = new MyResult<object>();
            StringBuilder SelectSql = new StringBuilder();
            SelectSql.Append("SELECT `auditState` FROM `user` WHERE `id` = @UserId;");
            UserEntity UserInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserEntity>(SelectSql.ToString(), new { UserId = UserId });
            if (UserInfo != null && UserInfo.AuditState == 2) { return new MyResult<object>(-1, "您的认证已完成，请务重复操作~"); }

            // StringBuilder QueryAlipaySql = new StringBuilder();
            // QueryAlipaySql.Append("SELECT `id` FROM `user` WHERE `alipay` = @Alipay AND auditState = 2;");
            // UserEntity UserAlipay = await context.Dapper.QueryFirstOrDefaultAsync<UserEntity>(QueryAlipaySql.ToString(), new { Alipay = model.Alipay });
            // if (UserAlipay != null) { return new MyResult<object>(-1, "支付宝已被使用,请更换其它支付宝号"); }

            StringBuilder QueryPaySql = new StringBuilder();
            QueryPaySql.Append("SELECT `id` FROM `order_games` WHERE `userId` = @UserId AND `status` = 1;");
            OrderGames PayOrder = await context.Dapper.QueryFirstOrDefaultAsync<OrderGames>(QueryPaySql.ToString(), new { UserId = UserId });
            if (PayOrder == null) { return new MyResult<object>(-1, "请完成支付后，再进行认证~"); }

            StringBuilder QuerySqlStr = new StringBuilder();
            QuerySqlStr.Append("SELECT `id` FROM `authentication_infos` WHERE `idNum`= @IdNum;");
            AuthenticationInfos AuthInfo = await context.Dapper.QueryFirstOrDefaultAsync<AuthenticationInfos>(QuerySqlStr.ToString(), new { IdNum = model.IdNum });
            if (AuthInfo != null) { return new MyResult<object>(-1, "身份证号已被使用~"); }

            // StringBuilder InitSqlStr = new StringBuilder();
            // InitSqlStr.Append("SELECT `id`, `CertifyId`, `CertifyUrl`, `Alipay`, `IsUsed`, `CreateTime` FROM `face_init_record` WHERE `IDCardNum` = @IDCardNum AND `TrueName` = @TrueName ORDER BY id DESC;");
            // FaceInitRecord InitRecord = await context.Dapper.QueryFirstOrDefaultAsync<FaceInitRecord>(InitSqlStr.ToString(), new { TrueName = model.TrueName, IDCardNum = model.IdNum });

            // if (InitRecord != null && !string.IsNullOrWhiteSpace(InitRecord.CertifyUrl) && InitRecord.CreateTime > DateTime.Now.AddHours(-24))
            // {
            //     if (!InitRecord.Alipay.Equals(model.Alipay))
            //     {
            //         StringBuilder UpInitSql = new StringBuilder();
            //         DynamicParameters UpInitParam = new DynamicParameters();
            //         UpInitSql.Append("UPDATE `face_init_record` SET `Alipay` = @Alipay WHERE `Id` = @RecordId");
            //         UpInitParam.Add("Alipay", model.Alipay, DbType.String);
            //         UpInitParam.Add("RecordId", InitRecord.Id, DbType.Int32);
            //         context.Dapper.Execute(UpInitSql.ToString(), UpInitParam);
            //     }

            //     Rult.Data = new FaceModel()
            //     {
            //         CertifyId = InitRecord.CertifyId,
            //         CertifyUrl = InitRecord.CertifyUrl
            //     };
            //     return Rult;
            // }

            return null;
        }

        /// <summary>
        /// 扫脸认证【未起用】
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> ScanFaceInit(AuthenticationDto model, int userId)
        {
            MyResult<object> Rult = new MyResult<object>();
            StringBuilder SelectSql = new StringBuilder();
            SelectSql.Append("SELECT `auditState` FROM `user` WHERE `id` = @UserId;");
            UserEntity UserInfo = context.Dapper.Query<UserEntity>(SelectSql.ToString(), new { UserId = userId }).FirstOrDefault();

            if (UserInfo == null)
            {
                return new MyResult<object>() { Code = -1, Message = "您的操作好像出了点问题~" };
            }
            if (UserInfo.AuditState == 2)
            {
                return new MyResult<object>() { Code = -1, Message = "您的认证已完成，请务重复操作" };
            }

            StringBuilder QuerySqlStr = new StringBuilder();
            QuerySqlStr.Append("SELECT `id` FROM `authentication_infos` WHERE `userId`= @UserId;");
            AuthenticationInfos AuthInfo = context.Dapper.Query<AuthenticationInfos>(QuerySqlStr.ToString(), new { Userid = userId }).FirstOrDefault();

            if (AuthInfo == null)
            {
                StringBuilder InsertSqlStr = new StringBuilder();
                InsertSqlStr.Append("INSERT INTO `authentication_infos` ( `userId`, `trueName`, `idNum`, `authType`, `certifyId` ) VALUES( ");
                InsertSqlStr.Append("@UserId, @TrueName, @CertNo, @AuthType, @CertifyId);");
                await context.Dapper.ExecuteAsync(InsertSqlStr.ToString(), new { UserId = userId, TrueName = model.TrueName, CertNo = model.IdNum, AuthType = model.AuthType, CertifyId = model.CertifyId });
            }
            else
            {
                StringBuilder UpdateSqlStr = new StringBuilder();
                UpdateSqlStr.Append("UPDATE `authentication_infos` SET `trueName` = @TrueName, `idNum` = @CertNo, `authType` = @AuthType, `certifyId` = @CertifyId ");
                UpdateSqlStr.Append(" WHERE `userId` = @UserId");
                await context.Dapper.ExecuteAsync(UpdateSqlStr.ToString(), new { TrueName = model.TrueName, CertNo = model.IdNum, AuthType = model.AuthType, CertifyId = model.CertifyId, UserId = userId });
            }
            return new MyResult<object>() { Code = 0 };
        }

        /// <summary>
        /// 写入扫脸认证记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task WriteInitRecord(AuthenticationDto model)
        {
            StringBuilder InsertSqlStr = new StringBuilder();
            InsertSqlStr.Append("INSERT INTO `face_init_record` ( `CertifyId`, `CertifyUrl`, `TrueName`, `IDCardNum`, `IsUsed`, `CreateTime` ) VALUES( ");
            InsertSqlStr.Append("@CertifyId, @CertifyUrl, @TrueName, @IDCardNum, @IsUsed, @CreateTime);");
            await context.Dapper.ExecuteAsync(InsertSqlStr.ToString(), new { CertifyId = model.CertifyId, CertifyUrl = model.CharacterUrl, TrueName = model.TrueName, IDCardNum = model.IdNum, IsUsed = 0, CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
        }

        /// <summary>
        /// 扫脸认证记录校验
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<FaceInitRecord> VerfyFaceInit(AuthenticationDto model)
        {
            StringBuilder QuerySqlStr = new StringBuilder();
            QuerySqlStr.Append("SELECT `id`, `CertifyId`, `IDCardNum`, `Alipay` FROM `face_init_record` WHERE `CertifyId` = @CertifyId;");
            FaceInitRecord InitRecord = await context.Dapper.QueryFirstOrDefaultAsync<FaceInitRecord>(QuerySqlStr.ToString(), new { CertifyId = model.CertifyId, IDCardNum = model.IdNum });
            return InitRecord;
        }

        /// <summary>
        /// 获取会员信息 根据手机号
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> GetUserByMobile(string mobile)
        {
            MyResult<object> result = new MyResult<object>();
            if (string.IsNullOrWhiteSpace(mobile))
            {
                return result.SetStatus(ErrorCode.InvalidData, "会员不存在");
            }
            try
            {
                UserEntity UserInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserEntity>("SELECT `status`, auditState, `name`, avatarUrl FROM `user` WHERE mobile = @Mobile;", new { Mobile = mobile });

                if (UserInfo == null)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "会员不存在");
                }

                if (UserInfo.Status != 0 || UserInfo.AuditState != 2)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "此会员已被锁定");
                }
                if (!string.IsNullOrWhiteSpace(UserInfo.AvatarUrl))
                {
                    UserInfo.AvatarUrl = "https://file.yoyoba.cn/" + UserInfo.AvatarUrl;
                }
                else
                {
                    UserInfo.AvatarUrl = "https://file.yoyoba.cn/default.png";
                }
                result.Data = UserInfo;
                return result;
            }
            catch (Exception ex)
            {
                SystemLog.Debug("获取会员信息 根据手机号", ex);
                return result.SetStatus(ErrorCode.InvalidData, "会员不存在");
            }
        }


        /// <summary>
        /// 发送验证码公共方法
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<MsgDto>> CommonSendVcode2(SendVcode model)
        {
            MyResult<MsgDto> result = new MyResult<MsgDto>();
            try
            {
                var MsgId = RedisCache.Get($"VCode:{ model.Mobile}");
                if (!String.IsNullOrWhiteSpace(MsgId))
                {
                    return new MyResult<MsgDto> { Data = new MsgDto { Is_Valid = false, Msg_Id = null, Error = new ErrorDto { Code = "-1", Message = "请稍后重试" } } };
                }

                StringContent content = new StringContent(new { mobile = model.Mobile, temp_id = "198014" }.GetJson());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await this.Client.PostAsync("https://api.sms.jpush.cn/v1/codes", content);
                String res = await response.Content.ReadAsStringAsync();
                result.Data = res.GetModel<MsgDto>();
                RedisCache.Set($"VCode:{ model.Mobile}", result.Data.Msg_Id, 600);
            }
            catch (Exception ex)
            {
                SystemLog.Debug("发送短信", ex);
                var res = "{\"error\":{\"code\":50013,\"message\":\"invalid temp_id\"}}";
                var resModel = res.GetModel<MsgDto>();
                result.Data = resModel;
                return result;
            }
            if (result.Data != null && !string.IsNullOrEmpty(result.Data.Msg_Id))
            {
                #region 写入数据库
                StringBuilder InsertSql = new StringBuilder();
                DynamicParameters Param = new DynamicParameters();
                InsertSql.Append("INSERT INTO `user_vcodes`(`mobile`, `msgId`, `createdAt`) VALUES (@Mobile, @MsgId , NOW());");
                Param.Add("Mobile", model.Mobile, DbType.String);
                Param.Add("MsgId", result.Data.Msg_Id, DbType.String);
                await context.Dapper.ExecuteAsync(InsertSql.ToString(), Param);
                #endregion
            }
            return result;
        }

        /// <summary>
        /// 校验验证码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<MsgDto>> CheckVcode(ConfirmVcode model)
        {
            MyResult<MsgDto> result = new MyResult<MsgDto>();
            try
            {
                //var base64Str = SecurityUtil.Base64Encode($"{Constants.JpushKey}:{Constants.JpushSecret}");
                //var Authorization = $"Basic {base64Str}";
                //var res = HttpUtil.PostString($"https://api.sms.jpush.cn/v1/codes/{model.MsgId}/valid", new { code = model.Vcode }.GetJson(), "application/json", null, Authorization);

                var MsgId = RedisCache.Get($"VCode:{ model.Mobile}");
                if (String.IsNullOrWhiteSpace(MsgId))
                {
                    return new MyResult<MsgDto> { Data = new MsgDto { Is_Valid = false, Msg_Id = null, Error = new ErrorDto { Code = "-1", Message = "验证码非法" } } };
                }
                if (MsgId.Length == 6)
                {
                    if (MsgId.Equals(model.Vcode))
                    {
                        RedisCache.Del($"VCode:{ model.Mobile}");
                        return new MyResult<MsgDto> { Data = new MsgDto { Is_Valid = true, Msg_Id = MsgId, Error = new ErrorDto { Code = "0", Message = "验证码正确" } } };
                    }
                    else
                    {
                        return new MyResult<MsgDto> { Data = new MsgDto { Is_Valid = false, Msg_Id = null, Error = new ErrorDto { Code = "-1", Message = "验证码错误" } } };
                    }
                }

                if (String.IsNullOrWhiteSpace(MsgId) || MsgId != model.MsgId)
                {
                    return new MyResult<MsgDto> { Data = new MsgDto { Is_Valid = false, Msg_Id = null, Error = new ErrorDto { Code = "-1", Message = "验证码非法" } } };
                }

                StringContent content = new StringContent(new { code = model.Vcode }.GetJson());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await this.Client.PostAsync($"https://api.sms.jpush.cn/v1/codes/{model.MsgId}/valid", content);
                String res = await response.Content.ReadAsStringAsync();

                var resModel = res.GetModel<MsgDto>();

                if (resModel.Is_Valid) { RedisCache.Del($"VCode:{ model.Mobile}"); }

                result.Data = resModel;
            }
            catch (System.Exception)
            {
                var res = "{\"is_valid\":false,\"error\":{\"code\":50026,\"message\":\"wrong msg_id\"}}";
                var resModel = res.GetModel<MsgDto>();
                result.Data = resModel;
                return result;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public MyResult<object> GetNameByMobile(string mobile)
        {
            MyResult result = new MyResult();
            try
            {
                if (string.IsNullOrEmpty(mobile)) { return result.SetStatus(ErrorCode.InvalidData, "邀请码不能为空"); }
                var user = context.Dapper.QueryFirstOrDefault<UserEntity>($"SELECT `name`, avatarUrl FROM `user` WHERE mobile = @mobile OR rcode = @mobile;", new { mobile });
                if (user == null) { return result.SetStatus(ErrorCode.NotFound, "邀请码有误 请联系推荐人"); }
                result.Data = new { user.Name, Avatar = Constants.CosUrl + user.AvatarUrl };
            }
            catch (Exception ex)
            {
                SystemLog.Debug("验证邀请码", ex);
                return result.SetStatus(ErrorCode.InvalidData, "系统异常 请联系管理员");
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> GenerateAppUrl(int userId, string type)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"GenerateAppUrl:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                #region 订单信息判断
                var Orders = context.Dapper.Query<OrderGames>($"select * from `order_games` where gameAppid=1 and userId={userId}").ToList();
                var payOrder = Orders.FirstOrDefault(o => o.Status == 1);
                if (payOrder != null) { return result.SetStatus(ErrorCode.InvalidData, "您已经支付，无需重复支付！"); }
                #endregion

                var orderGame = Orders.FirstOrDefault(o => o.Status == 0);
                var orderNum = Gen.NewGuid();
#if DEBUG
                Decimal payPrice = 0.01M;
#else
                Decimal payPrice = 1.50M;
#endif
                var res = 0;
                if (orderGame == null)
                {
                    res = context.Dapper.Execute($"insert into `order_games`(`gameAppid`,`orderId`,`userId`,`uuid`,`realAmount`,`status`,`createdAt`) values(1,'{orderNum}',{userId},0,{payPrice},0,now())");
                }
                else if (orderGame.CreatedAt == null || orderGame.CreatedAt?.AddMinutes(15) < DateTime.Now)
                {
                    StringBuilder UpdateSql = new StringBuilder();
                    DynamicParameters UpdateParam = new DynamicParameters();
                    UpdateSql.Append("UPDATE `order_games` SET `orderId` = @OrderId, `createdAt` = @CreateTime WHERE `id` = @Id");
                    UpdateParam.Add("OrderId", orderNum, DbType.String);
                    UpdateParam.Add("CreateTime", DateTime.Now, DbType.DateTime);
                    UpdateParam.Add("Id", orderGame.Id, DbType.Int32);

                    res = context.Dapper.Execute(UpdateSql.ToString(), UpdateParam);
                    payPrice = orderGame.RealAmount ?? 1.50M;
                }
                else
                {
                    orderNum = orderGame.OrderId;
                    payPrice = orderGame.RealAmount ?? 1.50M;
                    res = 1;
                }

                if (res != 0 && type == "alipay")
                {
                    String AppUrl = await AlipaySub.GetSignStr(new Request.ReqAlipayAppSubmit() { OutTradeNo = orderNum, TotalAmount = payPrice.ToString("0.00"), Subject = "实名认证", NotifyUrl = AppSettings.AlipayNotify, TimeOutExpress = "15m", PassbackParams = "AUTH_REAL_NAME" });
                    result.Data = AppUrl;
                    return result;
                }
                if (res != 0 && type == "wepay")
                {
                    var model = await WePaySub.Execute(new Request.ReqWepaySubmit()
                    {
                        TradeNo = orderNum,
                        Body = "实名认证",
                        TotalFee = Math.Ceiling(payPrice * 100.00M).ToInt(),
                        Attach = "AUTH_REAL_NAME",
                        TradeType = "APP",
                        NotifyUrl = AppSettings.WePayNotify,
                    });
                    result.Data = new { TradeNo = orderNum, PayStr = WePaySub.MakeSign(model.PrepayId) };
                    return result;
                }
                return result.SetStatus(ErrorCode.InvalidData, "生成支付订单失败");
            }
            catch (Exception ex)
            {
                SystemLog.Debug("生成支付订单失败", ex);
                return result.SetStatus(ErrorCode.SystemError, "生成支付订单失败");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }

        /// <summary>
        /// 实名广告
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> RealNameAd(Int64 UserId)
        {
            MyResult result = new MyResult();
            if (UserId < 0) { return result.SetStatus(ErrorCode.ErrorSign, "Error Sign"); }

            String CacheKey = $"RealAd_Lock:{UserId}";
            if (RedisCache.Exists(CacheKey))
            {
                return result.SetStatus(ErrorCode.InvalidData, "请操作太快了");
            }
            else { RedisCache.Set(CacheKey, UserId, 10); }

            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"RealNameAd:{UserId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                #region 订单信息判断
                var Orders = await context.Dapper.QueryAsync<OrderGames>($"select * from `order_games` where gameAppid=1 and userId={UserId}");
                var payOrder = Orders.FirstOrDefault(o => o.Status == 1);
                if (payOrder != null) { return result.SetStatus(ErrorCode.InvalidData, "您已观看完成，快去填写认证信息吧"); }
                #endregion
                var orderGame = Orders.FirstOrDefault(o => o.Status == 0);
                var orderNum = Gen.NewGuid();
                Decimal payPrice = 1.50M;
                Int32 TotalWatch = 1;
                Int32 rows = 0;
                if (orderGame == null)
                {
                    rows = context.Dapper.Execute($"insert into `order_games`(`gameAppid`,`orderId`,`userId`,`uuid`,`realAmount`,`status`,`createdAt`) values(1,'{orderNum}',{UserId},1,{payPrice},0,now());");
                    result.Data = new { TotalWatch };
                }
                else
                {
                    Int32.TryParse(orderGame.Uuid, out TotalWatch);

                    TotalWatch = TotalWatch + 1;

                    StringBuilder UpdateSql = new StringBuilder();
                    DynamicParameters UpdateParam = new DynamicParameters();
                    UpdateSql.Append("UPDATE `order_games` SET `status` = @Status, `orderId` = @OrderId, `uuid` = @Uuid , `updatedAt` = @UpdatedAt WHERE `id` = @Id");
                    UpdateParam.Add("Id", orderGame.Id, DbType.Int32);
                    UpdateParam.Add("OrderId", orderNum, DbType.String);
                    UpdateParam.Add("Uuid", TotalWatch, DbType.String);
                    UpdateParam.Add("UpdatedAt", DateTime.Now, DbType.DateTime);
                    if (TotalWatch >= 10)
                    {
                        UpdateParam.Add("Status", 1, DbType.Int32);
                    }
                    else
                    {
                        UpdateParam.Add("Status", 0, DbType.Int32);
                    }
                    rows = context.Dapper.Execute(UpdateSql.ToString(), UpdateParam);
                    result.Data = new { TotalWatch };
                }
                return result;
            }
            catch (Exception ex)
            {
                SystemLog.Debug("实名观看广告异常==>", ex);
                return result.SetStatus(ErrorCode.SystemError, "发生错误[AD]");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }

        }

        /// <summary>
        /// 支付宝退款信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> PayRefund(int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"PayRefund:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                var User = context.Dapper.QueryFirstOrDefault<int>($"SELECT COUNT(id) FROM `user` WHERE auditState=2 AND id={userId}");
                if (User != 0)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "您已完成实名认证，无法退款!");
                }
                var orderGame = context.Dapper.QueryFirstOrDefault<OrderGames>($"select * from `order_games` where gameAppid=1 and userId={userId} and status=1");
                if (orderGame == null)
                {
                    return result.SetStatus(ErrorCode.NotFound, "没有找到您的认证订单，无法退款！");
                }
                if ((DateTime.Now - orderGame.CreatedAt.Value).TotalDays > 7)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "订单已超过7天，无法退款！");
                }
                if ((DateTime.Now - orderGame.CreatedAt.Value).TotalMinutes < 5)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "支付成功后，5分钟内不可退款！");
                }
                if (orderGame.CreatedAt < DateTime.Parse("2020-03-16"))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "公测期订单，请求联系客服~");
                }
                var aliResult = await AlipaySub.Execute(new Request.ReqAlipayTradeRefund
                {
                    OutTradeNo = orderGame.OrderId,
                    RefundReason = "实名认证失败退款",
                    RefundAmount = orderGame.RealAmount.Value
                });
                if (aliResult.IsError)
                {
                    return result.SetStatus(ErrorCode.SystemError, aliResult.ErrMsg);
                }
                StringBuilder UpdateSql = new StringBuilder();
                DynamicParameters Param = new DynamicParameters();
                UpdateSql.Append("UPDATE `order_games` SET `status` = 5,`updatedAt`=NOW() WHERE `orderId` = @OrderId");
                Param.Add("OrderId", orderGame.OrderId, DbType.String);
                context.Dapper.Execute(UpdateSql.ToString(), Param);

                result.Data = true;
                result.Message = "退款成功";
                return result;
            }
            catch (Exception ex)
            {
                SystemLog.Debug("退款发生错误", ex);
                return result.SetStatus(ErrorCode.SystemError, "退款发生错误");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        public async Task<String> AliNotify(String TradeNo)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(TradeNo)) { return "fail"; }
                AlipayResult<Response.RspAlipayTradeQuery> PayRult = await AlipaySub.Execute(new Request.ReqAlipayTradeQuery { OutTradeNo = TradeNo });
                if (PayRult.IsError || !PayRult.Result.TradeStatus.Equals("TRADE_SUCCESS")) { return "fail"; }
                context.Dapper.Execute($"update `order_games` set `status`=1,updatedAt=NOW() where orderId='{TradeNo}' limit 1");
                return "success";
            }
            catch
            {
                return "fail";
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> HavePayOrder(int userId)
        {
            MyResult result = new MyResult();
            var count = await context.Dapper.QueryFirstOrDefaultAsync<int>($"select count(id) count from `order_games` where userId={userId} and gameAppid=1 and status=1 limit 1");
            if (count == 0)
            {
                return result.SetStatus(ErrorCode.NotFound, "未支付");
            }
            result.Data = true;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="outTradeNo"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> PayFlag(int userId, string outTradeNo)
        {
            MyResult result = new MyResult();
            if (!ProcessSqlStr(outTradeNo))
            {
                return result.SetStatus(ErrorCode.InvalidData, "非法操作");
            }
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            AlipayResult<Response.RspAlipayTradeQuery> PayRult = await AlipaySub.Execute(new Request.ReqAlipayTradeQuery()
            {
                OutTradeNo = outTradeNo
            });
            if (PayRult.IsError || !PayRult.Result.TradeStatus.Equals("TRADE_SUCCESS"))
            {
                return result.SetStatus(ErrorCode.InvalidToken, "支付未完成");
            }

            context.Dapper.Execute($"update `order_games` set `status`=1,updatedAt=NOW() where orderId='{outTradeNo}' and userId={userId}");
            result.Data = true;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<object> Authentication(AuthenticationDto model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            // Regex reg = new Regex(@"[\u4e00-\u9fa5]");
            // if (reg.IsMatch(model.Alipay))
            // {
            //     return result.SetStatus(ErrorCode.InvalidData, "支付宝号不能包含中文");
            // }
            // if (!ProcessSqlStr(model.Alipay))
            // {
            //     return result.SetStatus(ErrorCode.InvalidData, "支付宝号禁止使用特殊符号");
            // }
            //订单是否支付
            var order = context.Dapper.QueryFirstOrDefault($"select * from `order_games` where gameAppid=1 and userId={userId} and status=1");
            if (order == null)
            {
                return result.SetStatus(ErrorCode.InvalidData, "订单未支付 不能实名认证");
            }

            //身份证号检验
            var authInfo = context.Dapper.QueryFirstOrDefault<int>($"select id from `authentication_infos` where `idNum`='{model.IdNum}'");
            if (authInfo != 0)
            {
                return result.SetStatus(ErrorCode.InvalidData, "身份证号已存在");
            }
            //写入实名信息 更改实名状态 支付宝号

            //邀请人===从事务中取出，减少锁时间
            var user = context.Dapper.QueryFirstOrDefault<UserEntity>($"select `inviterMobile`,`name` from user where id={userId}");
            if (user == null) { return result.SetStatus(ErrorCode.InvalidData, "账号不存在"); }
            var inviterUser = context.Dapper.QueryFirstOrDefault<UserEntity>($"select id,golds from user where mobile='{user.InviterMobile}'");
            if (inviterUser == null) { return result.SetStatus(ErrorCode.InvalidData, "信息有误，请联系管理员"); }
            //贡献值
            var InviterDevote = 0;
            var gold = (int)inviterUser.Golds + 50;
            var level = CaculatorGolds(gold + InviterDevote);
            //计算量化宝时效
            var effectiveBiginTime = DateTime.Now.Date.ToLocalTime().ToString("yyyy-MM-dd");
            var effectiveEndTime = DateTime.Now.Date.AddDays(60).ToLocalTime().ToString("yyyy-MM-dd");
            if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
            using (IDbTransaction transaction = context.Dapper.BeginTransaction())
            {
                try
                {
                    StringBuilder Sql = new StringBuilder();
                    //修改实名状态
                    Sql.AppendLine($"update `user` set `auditState`=2,`golds`=(`golds`+50),`level`='lv0',`alipayUid`='' where id = {userId};");
                    //修改实名记录
                    Sql.AppendLine($"insert into `authentication_infos`(`userId`,`trueName`,`idNum`,`authType`, `certifyId`) values({userId},'{model.TrueName}','{model.IdNum}',{model.AuthType},'{model.CertifyId}');");

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
                StringBuilder UpdateSqlStr = new StringBuilder();
                UpdateSqlStr.Append("UPDATE `face_init_record` SET `IsUsed` = 1 WHERE `CertifyId` = @CertifyId AND `IDCardNum` = @IDCardNum;");
                context.Dapper.Execute(UpdateSqlStr.ToString(), new { CertifyId = model.CertifyId, IDCardNum = model.IdNum });

                long c = RedisCache.Publish("MEMBER_CERTIFIED", JsonConvert.SerializeObject(new { MemberId = userId, Nick = user.Name }));
                if (c == 0)
                {
                    SystemLog.Warn($"{userId} 实名认证:{model.ToJson()}");
                }
            }
            catch (Exception ex)
            {
                SystemLog.Debug("实名认证", ex);
                return result;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="golds"></param>
        /// <returns></returns>
        private string CaculatorGolds(decimal golds)
        {
            string UserLevel = String.Empty;
            foreach (var item in AppSettings.Levels.OrderBy(o => o.Claim))
            {
                if (golds >= item.Claim) { UserLevel = item.Level; }
            }
            return UserLevel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<object> ModifyUserPic(YoyoUserDto model, int userId)
        {
            MyResult result = new MyResult();
            if (string.IsNullOrEmpty(model.UserPic))
            {
                return result.SetStatus(ErrorCode.InvalidData, "头像不能为空");
            }
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            if (!ProcessSqlStr(model.UserPic))
            {
                return result.SetStatus(ErrorCode.InvalidData, "头像路径非法");
            }
            //更新头像
            context.Dapper.Execute($"update user set `avatarUrl`='{model.UserPic}' where id={userId}");
            result.Data = Constants.CosUrl + model.UserPic;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<object> ModifyUserName(string name, int userId)
        {
            MyResult result = new MyResult();

            if (!ProcessSqlStr(name))
            {
                return result.SetStatus(ErrorCode.InvalidData, "昵称禁止使用特殊符号");
            }
            if (string.IsNullOrEmpty(name))
            {
                return result.SetStatus(ErrorCode.InvalidData, "昵称不能为空");
            }
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            //更新昵称
            context.Dapper.Execute($"update user set `name`='{name}' where id={userId}");
            result.Data = name;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<object> ModifyUserInviterCode(string name, int userId)
        {
            MyResult result = new MyResult();
            Regex reg = new Regex(@"[\u4e00-\u9fa5]");
            if (reg.IsMatch(name))
            {
                return result.SetStatus(ErrorCode.InvalidData, "邀请码不能包含中文");
            }
            if (!ProcessSqlStr(name))
            {
                return result.SetStatus(ErrorCode.InvalidData, "邀请码禁止使用特殊符号");
            }
            if (name.Length > 8)
            {
                return result.SetStatus(ErrorCode.InvalidData, "邀请码最大长度为8位");
            }
            if (string.IsNullOrEmpty(name))
            {
                return result.SetStatus(ErrorCode.InvalidData, "邀请码不能为空");
            }
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            var keyFlag = MemoryCacheUtil.Get($"ModifyUserInviterCode{userId}");
            try
            {
                if (keyFlag == null)
                {
                    MemoryCacheUtil.Set($"ModifyUserInviterCode{userId}", 300, 1);
                    //查邀请码是否存在
                    var inviter = context.Dapper.QueryFirstOrDefault<int>($"select count(1) count from user where rcode='{name}' or mobile='{name}'");
                    if (inviter != 0)
                    {
                        return result.SetStatus(ErrorCode.InvalidData, "邀请码已存在");
                    }
                    //移除缓存
                    var oldInviter = context.Dapper.QueryFirstOrDefault<string>($"select rcode from user where id={userId}");
                    if (!string.IsNullOrWhiteSpace(oldInviter)) { RedisCache.Del($"UserRcode:{oldInviter}"); }

                    // //扣糖果
                    // var candyNum = context.Dapper.Execute($"update user set `candyNum`=(`candyNum`-{0.1}) where id={userId} AND `candyNum`>={0.1}");
                    // if (candyNum != 1)
                    // {
                    //     return result.SetStatus(ErrorCode.InvalidData, "糖果不足，无法设置邀请码!");
                    // }
                    // //写记录
                    // context.Dapper.Execute($"insert into `gem_records`(`userId`,`num`,`description`,gemSource) values({userId},-{0.1},'修改邀请码扣除0.1糖果',3)");
                    //更新昵称
                    context.Dapper.Execute($"update user set `rcode`='{name}' where id={userId}");
                    result.Data = name;
                }
                else
                {
                    return result.SetStatus(ErrorCode.InvalidData, "更换太快了，请稍后重试...");
                }
            }
            catch (Exception ex)
            {
                SystemLog.Debug("修改邀请码", ex);
                return result.SetStatus(ErrorCode.SystemError, "系统错误 请稍后再试");
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldPwd"></param>
        /// <param name="newPwd"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<object> ModifyLoginPwd(string oldPwd, string newPwd, int userId)
        {
            MyResult result = new MyResult();
            if (string.IsNullOrEmpty(oldPwd) || string.IsNullOrEmpty(newPwd))
            {
                return result.SetStatus(ErrorCode.InvalidData, "密码不能为空");
            }
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            //查询oldPwd
            var userOldPwd = context.Dapper.QueryFirstOrDefault<string>($"select `password` from user where id={userId}");
            var _oldPwd = SecurityUtil.MD5(oldPwd);
            if (userOldPwd != _oldPwd)
            {
                return result.SetStatus(ErrorCode.InvalidPassword, "旧密码输入错误");
            }
            var _newPwd = SecurityUtil.MD5(newPwd);
            //更新旧密码
            context.Dapper.Execute($"update user set `password`='{_newPwd}' where id={userId}");
            result.Data = true;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> ModifyOtcPwd(ModifyOtcPwdDto model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            if (string.IsNullOrEmpty(model.NewTradePwd))
            {
                return result.SetStatus(ErrorCode.InvalidData, "交易密码不能为空");
            }
            var Key = $"ModifyOtcPwd:{userId}";
            if (RedisCache.Exists(Key))
            {
                return result.SetStatus(ErrorCode.InvalidData, "暂时无法修改密码,稍后再试");
            }

            ConfirmVcode confirmVcode = new ConfirmVcode
            {
                Mobile = model.Mobile,
                Vcode = model.VCode,
                MsgId = model.MsgId
            };

            #region 修改交易密码验证码验证方式
            MyResult<MsgDto> VerifyRult = await CheckVcode(new ConfirmVcode() { Mobile = model.Mobile, MsgId = model.MsgId, Vcode = model.VCode });

            if (!VerifyRult.Data.Is_Valid)
            {
                return result.SetStatus(ErrorCode.NotFound, "验证码错误");
            }
            #endregion

            //修改密码
            var newTradePwd = SecurityUtil.MD5(model.NewTradePwd);
            context.Dapper.Execute($"update user set `tradePwd`='{newTradePwd}' where id={userId}");
            result.Data = true;
            RedisCache.Set(Key, userId, 300);
            return result;
        }

        /// <summary>
        /// 修改支付宝账号
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Alipay"></param>
        /// <param name="PayPwd"></param>
        /// <param name="AlipayPic"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> ModifyAlipay(Int64 UserId, String Alipay, String PayPwd, String Name = "", string AlipayPic = "0")
        {
            MyResult result = new MyResult();

            if (UserId < 1) { return result.SetStatus(ErrorCode.ErrorSign, "Sign Error"); }
            UserEntity userInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserEntity>($"select * from user where id={UserId}");
            if (userInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "用户信息不存在..."); }
            if (SecurityUtil.MD5(PayPwd) != userInfo.TradePwd) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码有误"); }
            if (userInfo.Status == 2 || userInfo.Status == 3 || userInfo.Status == 5) { return result.SetStatus(ErrorCode.AccountDisabled, "账号异常 请联系管理员"); }

            #region 拼装SQL 并扣款
            //扣除 账户 1
            StringBuilder DeductSql = new StringBuilder();
            DynamicParameters DeductParams = new DynamicParameters();
            DeductParams.Add("UserId", UserId, DbType.Int64);
            DeductParams.Add("Alipay", Alipay, DbType.String);
            DeductParams.Add("AlipayPic", AlipayPic, DbType.String);
            DeductParams.Add("AliPayName", Name, DbType.String);

            DeductSql.Append("UPDATE `user` SET alipay = @Alipay,aliPayName=@AliPayName,alipayPic=@AlipayPic ");
            DeductSql.Append("WHERE id = @UserId");
            if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }

            using (IDbTransaction transaction = context.Dapper.BeginTransaction())
            {
                try
                {
                    Int32 Rows = context.Dapper.Execute(DeductSql.ToString(), DeductParams, transaction);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    SystemLog.Debug(new { UserId, PayPwd, Alipay }, ex);
                }
                finally
                {
                    if (context.Dapper.State == ConnectionState.Open) { context.Dapper.Close(); }
                }
            }
            #endregion
            return result;
        }

        /// <summary>
        /// 录入人工审核信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<object> AdminAuth(AuthenticationDto model, int userId)
        {
            MyResult result = new MyResult();
            var keyFlag = MemoryCacheUtil.Get("AdminAuth");
            if (keyFlag == null)
            {
                MemoryCacheUtil.Set("AdminAuth", 3000, 3);
                try
                {
                    var userInfo = context.Dapper.Execute($"select count(id) count from `authentication_infos` where `idNum`={model.IdNum}");
                    if (userInfo > 0)
                    {
                        return result.SetStatus(ErrorCode.InvalidData, "身份证号已存在");
                    }
                    if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
                    using (IDbTransaction transaction = context.Dapper.BeginTransaction())
                    {
                        try
                        {
                            var res = context.Dapper.Execute($"insert into `authentication_infos`(userId,trueName,pic,pic1,pic2,idNum,authType) values({userId},'{model.TrueName}','{model.PositiveUrl}','{model.NegativeUrl}','{model.CharacterUrl}','{model.IdNum}',1)", null, transaction);
                            //更新用户审核状态
                            context.Dapper.Execute($"update user set `auditState`=1 where id={userId}", null, transaction);
                            if (res <= 0)
                            {
                                return result.SetStatus(ErrorCode.InvalidData, "系统错误请重试");
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            SystemLog.Debug("录入人工审核信息", ex);
                            transaction.Rollback();
                            return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                        }
                    }
                    context.Dapper.Close();
                    return result;
                }
                catch (System.Exception)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "请联系管理");
                }
            }
            else
            {
                return result.SetStatus(ErrorCode.InvalidData, "点评率太高...");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AdId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<object> LookAdGetCandyP(int AdId, int userId)
        {
            MyResult result = new MyResult();
            if (userId <= 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            var CacheKey = $"ClickHaveCandyP:{AdId}";
            Decimal HaveCandP = 500.00M;
            if (RedisCache.Exists(CacheKey))
            {
                HaveCandP = RedisCache.Get<Decimal>(CacheKey);
                if (HaveCandP <= 0) { HaveCandP = 0; }
            }

            return result.SetStatus(ErrorCode.HasValued, $"分享到朋友圈得果皮,剩余:{HaveCandP.ToString("F2")}个");
            #region 源代码注释
            //var keyFlag = MemoryCacheUtil.Get($"LookAdGetCandyP{userId}");
            //try
            //{
            //    if (keyFlag == null)
            //    {
            //        MemoryCacheUtil.Set($"LookAdGetCandyP{userId}", 3000, 1);
            //        var hasCandyP = context.Dapper.QueryFirstOrDefault<int>($"select count(1) count from `user_candyp` where userId={userId} and `source`=11 and TO_DAYS(now())=TO_DAYS(`createdAt`)");
            //        if (hasCandyP != 0)
            //        {
            //            return result.SetStatus(ErrorCode.InvalidData, "分享到朋友圈可继续得 果皮 奖励"); //
            //        }
            //        context.Dapper.Execute($"update user set `candyP`=(`candyP`+{0.1}) where id={userId}");
            //        context.Dapper.Execute($"insert into `user_candyp`(`userId`,`candyP`,`content`,`source`,`createdAt`,`updatedAt`) values({userId},{0.1},'浏览广告,赠送{0.1}果皮',11,now(),now())");
            //        result.Data = true;
            //    }
            //    else
            //    {
            //        return result.SetStatus(ErrorCode.InvalidData, "分享到朋友圈可继续得 果皮 奖励"); //
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    LogUtil<YoyoUserSerivce>.Error($"LookAdGetCandyP_{userId}_{ex.Message}");
            //    return result.SetStatus(ErrorCode.SystemError, "系统错误 请稍后再试");
            //}
            //return result;
            #endregion

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> ForgetLoginPwd(ConfirmVcode model, int userId)
        {
            MyResult result = new MyResult();
            if (model == null)
            {
                return result.SetStatus(ErrorCode.InvalidData, "非法数据");
            }
            if (string.IsNullOrEmpty(model.Password))
            {
                return result.SetStatus(ErrorCode.InvalidData, "密码不能为空");
            }
            if (!ProcessSqlStr(model.Mobile))
            {
                return result.SetStatus(ErrorCode.InvalidData, "非法操作");
            }
            try
            {
                MyResult<MsgDto> VerifyRult = await CheckVcode(new ConfirmVcode() { Mobile = model.Mobile, MsgId = model.MsgId, Vcode = model.Vcode });
                if (!VerifyRult.Data.Is_Valid)
                {
                    return result.SetStatus(ErrorCode.NotFound, "验证码错误");
                }

                var enChangePassword = SecurityUtil.MD5(model.Password);
                StringBuilder UpdateSql = new StringBuilder();
                UpdateSql.Append("UPDATE `user` SET `password` = @Password, utime = now() WHERE `mobile` = @Mobile;");
                DynamicParameters UpdateParam = new DynamicParameters();
                UpdateParam.Add("Password", enChangePassword, DbType.String);
                UpdateParam.Add("Mobile", model.Mobile, DbType.String);
                await context.Dapper.ExecuteAsync(UpdateSql.ToString(), UpdateParam);
            }
            catch (Exception ex)
            {
                SystemLog.Debug($"修改密码：{model.ToJson()}", ex);
                return result.SetStatus(ErrorCode.SystemError, "系统错误 请稍后再试");
            }
            return result;
        }

        #region 后台管理
        /// <summary>
        /// 会员列表
        /// </summary>
        /// <returns></returns>
        public async Task<ListModel<UserDto>> UserList(QueryUser query)
        {
            ListModel<UserDto> Rult = new ListModel<UserDto>()
            {
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            StringBuilder CuntSql = new StringBuilder();
            CuntSql.Append("SELECT COUNT(id) FROM `user` WHERE password <> '1234567' ");

            StringBuilder QuerySql = new StringBuilder();
            QuerySql.Append("SELECT u.id, u.`name`, u.rcode, u.mobile, u.`level`, u.inviterMobile, u.alipay, u.alipayUid,w.Balance coinBalance,h.Balance honorBalance,u.remark, ");
            QuerySql.Append("u.passwordSalt, u.`status`, u.auditState, u.ctime FROM `user` AS u LEFT JOIN user_account_cotton_coin AS w ON u.id = w.UserId LEFT JOIN user_account_honor AS h ON u.id = h.UserId ");
            QuerySql.Append("WHERE u.`password` <> '1234567' ");

            DynamicParameters QueryParam = new DynamicParameters();
            QueryParam.Add("UserId", query.UserId, DbType.String);
            QueryParam.Add("Mobile", query.Mobile, DbType.String);
            QueryParam.Add("Alipay", query.Alipay, DbType.String);
            QueryParam.Add("InviterMobile", query.InviterMobile, DbType.String);
            QueryParam.Add("Status", (Int32)query.Status, DbType.Int32);
            QueryParam.Add("AuditState", (Int32)query.AuditState, DbType.Int32);
            QueryParam.Add("PageIndex", (query.PageIndex - 1) * query.PageSize, DbType.Int32);
            QueryParam.Add("PageSize", query.PageSize, DbType.Int32);

            if (query.UserId > 0) { QuerySql.Append("AND u.id = @UserId "); CuntSql.Append("AND id = @UserId "); }
            if (!string.IsNullOrWhiteSpace(query.Mobile)) { QuerySql.Append("AND u.mobile = @Mobile "); CuntSql.Append("AND mobile = @Mobile "); }
            if (!string.IsNullOrWhiteSpace(query.Alipay)) { QuerySql.Append("AND u.alipay = @Alipay "); CuntSql.Append("AND alipay = @Alipay "); }
            if (!string.IsNullOrWhiteSpace(query.InviterMobile)) { QuerySql.Append("AND u.inviterMobile = @InviterMobile "); CuntSql.Append("AND inviterMobile = @InviterMobile "); }
            if (query.Status != UserState.All) { QuerySql.Append("AND u.`status` = @Status "); CuntSql.Append("AND status = @Status "); }
            if (query.AuditState != UserAuthState.All) { QuerySql.Append("AND u.`auditState` = @AuditState "); CuntSql.Append("AND auditState = @AuditState "); }

            Rult.Total = await context.Dapper.QueryFirstOrDefaultAsync<Int32>(CuntSql.ToString(), QueryParam);

            QuerySql.Append("ORDER BY u.id DESC LIMIT @PageIndex, @PageSize;");

            IEnumerable<UserDto> UserList = await context.Dapper.QueryAsync<UserDto>(QuerySql.ToString(), QueryParam);
            Rult.List = UserList.ToList();

            return Rult;
        }

        /// <summary>
        /// 会员账户信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> AccountInfo(QueryModel query)
        {
            MyResult<Object> result = new MyResult<Object>();
            result.Data = new
            {
                Honor = await HonorSub.Info(query.UserId),
                Cotton = await CottonSub.Info(query.UserId),
                Integral = await IntegralSub.Info(query.UserId),
                Wallet = (await WalletSub.WalletAccountInfo(query.UserId)).Data,
                Conch = await ConchSub.Info(query.UserId)
            };
            return result;
        }

        /// <summary>
        /// 会员账户流水
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<AdminAccountRecord>>> AccountRecord(QueryModel query)
        {
            MyResult<List<AdminAccountRecord>> result = new MyResult<List<AdminAccountRecord>>();
            MyResult<List<AccountRecord>> rult = new MyResult<List<AccountRecord>>();
            UserEntity UserInfo = null;
            if (!string.IsNullOrWhiteSpace(query.Mobile))
            {
                UserInfo = await context.UserEntity.Where(item => item.Mobile == query.Mobile).FirstOrDefaultAsync();
                query.UserId = UserInfo?.Id ?? 0;
            }
            else
            {
                UserInfo = await context.UserEntity.Where(item => item.Id == query.UserId).FirstOrDefaultAsync();
            }
            switch (query.Keyword)
            {
                case "honor":
                    rult = await HonorSub.Records(query);
                    break;
                case "cotton":
                    rult = await CottonSub.Records(query);
                    break;
                case "integral":
                    rult = await IntegralSub.Records(query);
                    break;
                case "conch":
                    rult = await ConchSub.Records(query);
                    break;
                case "wallet":
                    var data = await WalletSub.WalletAccountRecord(query.UserId, query.PageIndex, query.PageSize);
                    result.PageCount = data.PageCount;
                    result.RecordCount = data.RecordCount;
                    result.Data = new List<AdminAccountRecord>();
                    foreach (var item in data.Data)
                    {
                        result.Data.Add(new AdminAccountRecord()
                        {
                            RecordId = item.RecordId,
                            UserId = UserInfo?.Id ?? 0,
                            Nick = UserInfo?.Name ?? "",
                            Mobile = UserInfo?.Mobile ?? "",
                            AccountId = item.AccountId,
                            PostChange = item.PostChange,
                            PreChange = item.PreChange,
                            Incurred = item.Incurred,
                            ModifyDesc = String.Format(item.ModifyType.GetDescription(), item.ModifyDesc.Split(",")),
                            ModifyType = (int)item.ModifyType,
                            ModifyTime = item.ModifyTime
                        });
                    }
                    break;
                default:
                    result.Data = new List<AdminAccountRecord>();
                    break;
            }
            if (query.Keyword == "wallet") { return result; }

            result.PageCount = rult.PageCount;
            result.RecordCount = rult.RecordCount;
            result.Data = new List<AdminAccountRecord>();
            if (rult.Data == null) { return result; }
            foreach (var item in rult.Data)
            {
                result.Data.Add(new AdminAccountRecord()
                {
                    RecordId = item.RecordId,
                    UserId = UserInfo?.Id ?? 0,
                    Nick = UserInfo?.Name ?? "",
                    Mobile = UserInfo?.Mobile ?? "",
                    AccountId = item.AccountId,
                    PostChange = item.PostChange,
                    PreChange = item.PreChange,
                    Incurred = item.Incurred,
                    ModifyDesc = item.ModifyDesc,
                    ModifyType = (int)item.ModifyType,
                    ModifyTime = item.ModifyTime
                });
            }
            return result;
        }

        /// <summary>
        /// 冻结会员
        /// </summary>
        /// <returns></returns>
        public async Task<UserDto> Freeze(UserDto model)
        {
            DynamicParameters Param = new DynamicParameters();
            Param.Add("UserId", model.Id, DbType.Int64);
            Param.Add("Remark", model.Remark, DbType.String);
            Int32 Rows = await context.Dapper.ExecuteAsync("UPDATE `user` SET `status` = 2, `remark` = @Remark WHERE id = @UserId;", Param);
            if (Rows > 0)
            {
                model.Status = 2;
                return model;
            }
            return null;
        }

        /// <summary>
        /// 解冻会员
        /// </summary>
        /// <returns></returns>
        public async Task<UserDto> Unfreeze(UserDto model)
        {
            DynamicParameters Param = new DynamicParameters();
            Param.Add("UserId", model.Id, DbType.Int64);
            Int32 Rows = await context.Dapper.ExecuteAsync("UPDATE `user` SET `status` = 0 WHERE id = @UserId;", Param);
            if (Rows > 0)
            {
                model.Status = 0;
                return model;
            }
            return null;
        }

        /// <summary>
        /// 修改会员信息
        /// </summary>
        /// <returns></returns>
        public async Task<UserDto> Modify(UserDto model)
        {
            DynamicParameters Param = new DynamicParameters();
            Param.Add("UserId", model.Id, DbType.Int64);
            UserDto UserInfo = context.Dapper.QueryFirstOrDefault<UserDto>("SELECT * FROM `user` WHERE id = @UserId;", Param);
            if (UserInfo == null) { return null; }

            Int32 Rows = 0;

            StringBuilder SqlStr = new StringBuilder();
            SqlStr.Append("UPDATE `user` SET ");

            if (!string.IsNullOrEmpty(model.Alipay) && UserInfo.Alipay != model.Alipay)
            {
                SqlStr.Append("`alipay` = @Alipay ");
                Param.Add("Alipay", model.Alipay, DbType.String);
                Rows++;
            }

            if (UserInfo.AlipayUid != model.AlipayUid)
            {
                SqlStr.Append("`alipayUid` = @AlipayUid ");
                Param.Add("AlipayUid", model.AlipayUid, DbType.String);
                Rows++;
            }

            if (Rows < 1) { return null; }
            SqlStr.Append("WHERE id = @UserId;");
            Int32 row = await context.Dapper.ExecuteAsync(SqlStr.ToString(), Param);

            if (Rows == row) { return UserInfo; }
            return null;
        }

        /// <summary>
        /// 查询认证信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AuthDto> AuthInfo(UserDto model)
        {
            DynamicParameters Param = new DynamicParameters();
            Param.Add("UserId", model.Id, DbType.Int64);
            return await context.Dapper.QueryFirstOrDefaultAsync<AuthDto>("SELECT * FROM authentication_infos WHERE userId = @UserId;", Param);
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
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //用户信息
                // var userInfo = await context.Dapper.QueryFirstOrDefaultAsync<UserEntity>($"select * from user where id={userId}");
                // if (userInfo == null)
                // {
                //     return result.SetStatus(ErrorCode.InvalidData, "用户信息不能为空...");
                // }
                //订单信息
                var orderInfo = context.Dapper.QueryFirstOrDefault<CoinTrade>($"SELECT `status`, amount, fee, buyerUid, sellerUid, trendSide,coinType FROM coin_trade WHERE tradeNumber = '{model.OrderNum}' AND `status` =3;");
                if (orderInfo == null)
                {
                    return result.SetStatus(ErrorCode.SystemError, "订单状态异常 请联系管理员");
                }
                if (orderInfo.Status != 3)
                {
                    return result.SetStatus(ErrorCode.SystemError, "订单状态异常");
                }

                if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
                if (orderInfo.CoinType == "RZQ")
                {
                    context.Dapper.Execute($"update coin_trade set status=4 where tradeNumber = '{model.OrderNum}'");
                    //发放认证券
                    var rult = await TicketSub.ChangeAmount((long)orderInfo.BuyerUid, (decimal)orderInfo.Amount, TicketModifyType.TICKET_SUBSCRIBE, false, orderInfo.Amount.ToString());
                    if (rult == null || !rult.Success) { return rult; }
                    return result;
                }
                else
                {
                    using (IDbTransaction transaction = context.Dapper.BeginTransaction())
                    {
                        try
                        {
                            var systemUserId = 1;
                            var TotalCandy = orderInfo.Amount + orderInfo.Fee;

                            //减掉卖家用户的冻结账户中的冻结余额并添加流水
                            var res1 = await ChangeWalletAmount(CacheCottonCoinLockKey, CottonCoinTableName, CottonCoinRecordTableName, transaction, true, (long)orderInfo.SellerUid, -(decimal)orderInfo.Amount.Value, (int)ConchModifyType.Coin_Sell_Coin, true, Math.Round((decimal)orderInfo.Amount, 4).ToString());
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

                            //Gas 减掉卖家用户的冻结账户中的冻结余额并添加流水
                            var res4 = await ChangeWalletAmount(CacheHonorLockKey, HonorTableName, HonorRecordTableName, transaction, true, (long)orderInfo.SellerUid, -(decimal)orderInfo.Fee.Value, (int)HonorModifyType.Seller_Sub_Honor, true, Math.Round((decimal)orderInfo.Amount, 4).ToString(), Math.Round((decimal)orderInfo.Fee.Value, 4).ToString());
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
                            context.Dapper.Execute($"update coin_trade set status=4 where tradeNumber = {model.OrderNum}", null, transaction);
                            transaction.Commit();
                            return result;
                        }
                        catch (System.Exception ex)
                        {
                            SystemLog.Debug(ex);
                            transaction.Rollback();
                            return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                        }
                        finally
                        {
                            context.Dapper.Close();
                        }

                    }
                }
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
        #endregion

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
    }
}