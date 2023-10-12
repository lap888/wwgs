using CSRedis;
using Gs.Core;
using Gs.Core.Utils;
using Gs.Application;
using Gs.Domain.Enums;
using Gs.Domain.Entity;
using Gs.Domain.Models.Dto;
using Gs.Domain.Repository;
using Gs.Application.Request;
using Gs.Application.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net.Http.Headers;
using Gs.Core.Extensions;

namespace Gs.WebApi.Controllers
{
    /// <summary>
    /// 会员
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class UserController : BaseController
    {
        private readonly IRealVerify RealVerify;
        private readonly IUserSerivce UserSerivce;
        private readonly IQCloudPlugin QCloudSub;
        private readonly CSRedisClient RedisCache;
        private readonly IWalletService WalletAccount;
        private readonly HttpClient client;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="userSerivce"></param>
        /// <param name="realVerify"></param>
        /// <param name="qCloud"></param>
        /// <param name="redisClient"></param>
        /// <param name="userWallet"></param>
        public UserController(IHttpClientFactory factory, IUserSerivce userSerivce, IRealVerify realVerify, IQCloudPlugin qCloud, CSRedisClient redisClient, IWalletService userWallet)
        {
            QCloudSub = qCloud;
            RealVerify = realVerify;
            RedisCache = redisClient;
            UserSerivce = userSerivce;
            WalletAccount = userWallet;
            this.client = factory.CreateClient("Face");
        }

        /// <summary>
        /// 用户充值接口
        /// </summary>
        /// <param name="type"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [HttpGet("{type}")]
        public async Task<MyResult<object>> WallerRecharge(string type, decimal amount)
        {
            return await WalletAccount.Recharge(this.TokenModel.Id, type, amount);
        }

        /// <summary>
        /// 生成支付签名
        /// </summary>
        /// <returns></returns>
        [HttpGet("{type}")]
        public async Task<MyResult<Object>> GenerateAppUrl(String type)
        {
            return await UserSerivce.GenerateAppUrl(base.TokenModel.Id, type);
        }

        /// <summary>
        /// 认证失败退款接口
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> PayRefund()
        {
            return await UserSerivce.PayRefund(base.TokenModel.Id);
        }

        /// <summary>
        /// 检查是否有订单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> HavePayOrder()
        {
            return await UserSerivce.HavePayOrder(base.TokenModel.Id);
        }

        /// <summary>
        /// 支付查询
        /// </summary>
        /// <param name="outTradeNo"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> PayFlag(string outTradeNo)
        {
            return await UserSerivce.PayFlag(base.TokenModel.Id, outTradeNo);
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MyResult<object>> SendVcode([FromBody] SendVcode model)
        {
            return await UserSerivce.SendVcode(model);
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MyResult<object>> SignUp([FromBody] SignUpDto model)
        {
            return await UserSerivce.SignUp(model);
        }

        /// <summary>
        /// 忘记密码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MyResult<object>> ForgetLoginPwd([FromBody] ConfirmVcode model)
        {
            return await UserSerivce.ForgetLoginPwd(model, base.TokenModel.Id);
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public MyResult<object> Login([FromBody] YoyoUserDto model)
        {
            return UserSerivce.Login(model);
        }

        /// <summary>
        /// 扫脸
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> FaceInit([FromBody] FaceDto model)
        {
            MyResult result = new MyResult();
            #region 验证请求参数
            if (base.TokenModel == null) { return new MyResult<object>(-1, "请重新登录"); }
            if (string.IsNullOrEmpty(model.CertName)) { return new MyResult<object>(-1, "姓名不能为空"); }
            if (string.IsNullOrEmpty(model.CertNo)) { return new MyResult<object>(-1, "身份证号不能为空！"); }
            var isV = DataValidUtil.IsIDCard(model.CertNo);
            if (!isV) { return new MyResult<object>(-1, "身份证号不合法"); }
            // if (!DataValidUtil.IsEMail(model.Alipay) && !DataValidUtil.IsMobile(model.Alipay))
            // {
            //     return new MyResult<object>(-1, "请求输入正常的支付宝号");
            // }
            #endregion

            MyResult<object> VerfiyRult = await UserSerivce.IsFaceAuth(new AuthenticationDto()
            {
                TrueName = model.CertName,
                IdNum = model.CertNo
            }, base.TokenModel.Id);
            if (VerfiyRult != null) { return VerfiyRult; }

            #region 防重复操作
            String TicketLockStr = $"FaceInit{model.CertNo}";
            if (RedisCache.Exists(TicketLockStr))
            {
                return result.SetStatus(ErrorCode.InvalidData, "操作过于频繁，请稍后重试");
            }
            RedisCache.Set(TicketLockStr, this.TokenModel.Id, 5);

            #endregion

            try
            {
                var rult2 = RealVerify.ExecuteNew(Gen.NewGuid(), model.CertName, model.CertNo, model.Metainfo, this.TokenModel.Id.ToString());
                if (rult2.IsError)
                {
                    result.Code = -1;
                    result.Message = rult2.ErrMsg;
                    return result;
                }
                result.Data = new FaceModel()
                {
                    CertifyId = rult2.Data.CertifyId,
                    CertifyUrl = rult2.Data.CertifyUrl
                };
                await UserSerivce.WriteInitRecord(new AuthenticationDto()
                {
                    AuthType = 0,
                    CertifyId = rult2.Data.CertifyId,
                    CharacterUrl = rult2.Data.CertifyUrl,
                    TrueName = model.CertName,
                    IdNum = model.CertNo.ToUpper()
                });
                return result;
            }
            catch (Exception ex)
            {
                SystemLog.Debug("实名认证失败", ex);
                return new MyResult<object>(-1, "系统错误 请联系管理员");
            }
        }


        /// <summary>
        /// 认证信息记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Authentication([FromBody] AuthenticationDto model)
        {
            MyResult result = new MyResult();
            if (String.IsNullOrWhiteSpace(model.CertifyId))
            {
                return new MyResult<object>(-1, "请下载最新版APP进行实名认证！");
            }
            FaceInitRecord InitRecord = await UserSerivce.VerfyFaceInit(model);
            if (InitRecord == null)
            {
                SystemLog.Error("实名认证信息过期日志记录:\r\n" + model.ToJson());
                return new MyResult<object>(-1, "认证已过期");
            }
            if (!InitRecord.IdcardNum.Equals(model.IdNum, StringComparison.OrdinalIgnoreCase))
            {
                return new MyResult<object>(-1, "您提交的认证信息不匹配");
            }
            if (InitRecord.IsUsed != 0)
            {
                return new MyResult<object>(-1, "您已完成实名认证");
            }

            RspRealVerifyInitiate rult = RealVerify.DescribeFaceVerify(1000001375, model.CertifyId);

            if (rult.IsError)
            {
                result.Code = -1;
                result.Message = rult.ErrMsg;
                return result;
            }
            return UserSerivce.Authentication(model, base.TokenModel.Id);
        }

        /// <summary>
        /// 修改用户头像
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> ModifyUserPic([FromBody] YoyoUserDto model)
        {
            if (!string.IsNullOrEmpty(model.UserPic) && model.UserPic.Length > 1000)
            {
                try
                {
                    String BasePic = model.UserPic;
                    String FilePath = PathUtil.Combine("HeadImg", SecurityUtil.MD5(base.TokenModel.Id.ToString()).ToLower() + ".png");
                    Regex reg1 = new Regex("%2B", RegexOptions.IgnoreCase);
                    Regex reg2 = new Regex("%2F", RegexOptions.IgnoreCase);
                    Regex reg3 = new Regex("%3D", RegexOptions.IgnoreCase);
                    Regex reg4 = new Regex("(data:([^;]*);base64,)", RegexOptions.IgnoreCase);

                    var newBase64 = reg1.Replace(BasePic, "+");
                    newBase64 = reg2.Replace(newBase64, "/");
                    newBase64 = reg3.Replace(newBase64, "=");
                    BasePic = reg4.Replace(newBase64, "");

                    byte[] bt = Convert.FromBase64String(BasePic);
                    await QCloudSub.PutObject(FilePath, new System.IO.MemoryStream(bt));
                    model.UserPic = FilePath + "?v" + DateTime.Now.ToString("MMddHHmmss");
                }
                catch (Exception ex)
                {
                    SystemLog.Debug("头像上传失败:", ex);
                    return new MyResult<object>() { Code = -1, Message = "头像上传失败" };
                }
            }
            return UserSerivce.ModifyUserPic(model, base.TokenModel.Id);
        }

        /// <summary>
        /// 修改用户昵称
        /// </summary>
        /// <param name="name">昵称</param>
        /// <returns></returns>
        [HttpGet]
        public MyResult<object> ModifyUserName(string name)
        {
            return UserSerivce.ModifyUserName(name, base.TokenModel.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        public MyResult<object> ModifyUserInviterCode(string name)
        {
            return UserSerivce.ModifyUserInviterCode(name, base.TokenModel.Id);
        }


        /// <summary>
        /// 修改登陆密码
        /// </summary>
        /// <param name="oldPwd"></param>
        /// <param name="newPwd"></param>
        /// <returns></returns>
        [HttpGet]
        public MyResult<object> ModifyLoginPwd(string oldPwd, string newPwd)
        {
            return UserSerivce.ModifyLoginPwd(oldPwd, newPwd, base.TokenModel.Id);
        }

        /// <summary>
        /// 修改交易密码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> ModifyOtcPwd([FromBody] ModifyOtcPwdDto model)
        {
            return await UserSerivce.ModifyOtcPwd(model, base.TokenModel.Id);
        }
        /// <summary>
        /// 更换支付宝
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Change([FromBody] AliInfo req)
        {
            if (!DataValidUtil.IsEMail(req.Alipay) && !DataValidUtil.IsMobile(req.Alipay))
            {
                return new MyResult<object>(-1, "请求输入正常的支付宝号");
            }

            if (!string.IsNullOrEmpty(req.AlipayPic) && req.AlipayPic.Length > 1000)
            {
                try
                {
                    String BasePic = req.AlipayPic;
                    String FilePath = PathUtil.Combine("AlipayPic", SecurityUtil.MD5(base.TokenModel.Id.ToString()).ToLower() + ".png");
                    Regex reg1 = new Regex("%2B", RegexOptions.IgnoreCase);
                    Regex reg2 = new Regex("%2F", RegexOptions.IgnoreCase);
                    Regex reg3 = new Regex("%3D", RegexOptions.IgnoreCase);
                    Regex reg4 = new Regex("(data:([^;]*);base64,)", RegexOptions.IgnoreCase);

                    var newBase64 = reg1.Replace(BasePic, "+");
                    newBase64 = reg2.Replace(newBase64, "/");
                    newBase64 = reg3.Replace(newBase64, "=");
                    BasePic = reg4.Replace(newBase64, "");

                    byte[] bt = Convert.FromBase64String(BasePic);
                    await QCloudSub.PutObject(FilePath, new System.IO.MemoryStream(bt));
                    req.AlipayPic = FilePath + "?v" + DateTime.Now.ToString("MMddHHmmss");
                }
                catch (Exception ex)
                {
                    SystemLog.Debug(ex);
                    return new MyResult<object>() { Code = -1, Message = "头像上传失败" };
                }
            }
            return await UserSerivce.ModifyAlipay(base.TokenModel.Id, req.Alipay, req.PayPwd, req.Name, req.AlipayPic);
        }

    }
    /// <summary>
    /// 
    /// </summary>
    public class AliInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public String Alipay { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>

        public String PayPwd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>

        public string AlipayPic { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        /// <value></value>
        public string Name { get; set; }
    }
}