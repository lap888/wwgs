using Gs.Domain.Entity;
using Gs.Domain.Models;
using Gs.Domain.Models.Admin;
using Gs.Domain.Models.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gs.Domain.Repository
{
    public interface IUserSerivce
    {
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<object>> SignUp(SignUpDto model);

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        MyResult<object> Login(YoyoUserDto model);

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<object>> SendVcode(SendVcode model);

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        MyResult<object> GetNameByMobile(string mobile);

        /// <summary>
        /// 根据手机号 获取会员信息
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        Task<MyResult<object>> GetUserByMobile(string mobile);

        /// <summary>
        /// 生成支付订单签名
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<MyResult<object>> GenerateAppUrl(int userId, string type);

        /// <summary>
        /// 实名广告
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        Task<MyResult<object>> RealNameAd(Int64 UserId);

        /// <summary>
        /// 实名认证失败，退款
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> PayRefund(int userId);

        /// <summary>
        /// 阿里支付异步通知
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        Task<string> AliNotify(string TradeNo);

        /// <summary>
        /// 标志支付成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="outTradeNo"></param>
        /// <returns></returns>
        Task<MyResult<object>> PayFlag(int userId, string outTradeNo);

        /// <summary>
        /// 认证
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MyResult<object> Authentication(AuthenticationDto model, int userId);

        /// <summary>
        /// 刷脸认证效验
        /// </summary>
        /// <param name="model"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        Task<MyResult<object>> IsFaceAuth(AuthenticationDto model, int UserId);

        /// <summary>
        /// 扫脸认证【未起用】
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> ScanFaceInit(AuthenticationDto model, int userId);

        /// <summary>
        /// 扫脸认证记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task WriteInitRecord(AuthenticationDto model);

        /// <summary>
        /// 扫脸认证记录校验
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<FaceInitRecord> VerfyFaceInit(AuthenticationDto model);

        /// <summary>
        /// 修改头像
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MyResult<object> ModifyUserPic(YoyoUserDto model, int userId);

        /// <summary>
        /// 修改昵称
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MyResult<object> ModifyUserName(string name, int userId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MyResult<object> ModifyUserInviterCode(string name, int userId);

        /// <summary>
        /// 修改登陆密码
        /// </summary>
        /// <param name="oldPwd"></param>
        /// <param name="newPwd"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MyResult<object> ModifyLoginPwd(string oldPwd, string newPwd, int userId);

        /// <summary>
        /// 修改交易密码
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> ModifyOtcPwd(ModifyOtcPwdDto model, int userId);

        /// <summary>
        /// 修改支付宝账号
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Alipay"></param>
        /// <param name="PayPwd"></param>
        /// <param name="AlipayPic"></param>
        /// <returns></returns>
        Task<MyResult<Object>> ModifyAlipay(Int64 UserId, String Alipay, String PayPwd, String Name, String AlipayPic);

        /// <summary>
        /// 人工审核
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MyResult<object> AdminAuth(AuthenticationDto model, int userId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AdId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MyResult<object> LookAdGetCandyP(int AdId, int userId);

        /// <summary>
        /// 忘记密码
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> ForgetLoginPwd(ConfirmVcode model, int userId);

        /// <summary>
        /// 查看是否存在订单
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> HavePayOrder(int userId);

        #region 后台管理
        /// <summary>
        /// 会员列表
        /// </summary>
        /// <returns></returns>
        Task<ListModel<UserDto>> UserList(QueryUser query);

        /// <summary>
        /// 会员账户信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<Object>> AccountInfo(QueryModel query);

        /// <summary>
        /// 会员账户信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<AdminAccountRecord>>> AccountRecord(QueryModel query);

        /// <summary>
        /// 冻结会员
        /// </summary>
        /// <returns></returns>
        Task<UserDto> Freeze(UserDto query);

        /// <summary>
        /// 解冻会员
        /// </summary>
        /// <returns></returns>
        Task<UserDto> Unfreeze(UserDto query);

        /// <summary>
        /// 修改会员信息
        /// </summary>
        /// <returns></returns>
        Task<UserDto> Modify(UserDto query);

        /// <summary>
        /// 查询认证信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<AuthDto> AuthInfo(UserDto model);

        /// <summary>
        /// 后台发送新人券
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> PaidCoin(PaidDto model, int userId);
        #endregion

    }
}