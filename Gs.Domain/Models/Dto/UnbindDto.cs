using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Dto
{
    /// <summary>
    /// 解绑设置
    /// </summary>
    public class UnbindDto
    {
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 消息编号
        /// </summary>
        public string MsgId { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        public string VerifyCode { get; set; }

        /// <summary>
        /// 设置标识
        /// </summary>
        public string DeviceId { get; set; }
    }
}
