namespace Gs.Domain.Models.Dto
{
    /// <summary>
    /// 发送验真码
    /// </summary>
    public class SendVcode
    {
        /// <summary>
        /// 手机号
        /// </summary>
        /// <value></value>
        public string Mobile { get; set; }

        /// <summary>
        /// type="signIn"注册 
        /// type="update"修改交易密码 
        /// type="resetPassword" 重置密码
        /// type="unbind" 解绑设备
        /// </summary>
        /// <value></value>
        public string Type { get; set; }
    }
}