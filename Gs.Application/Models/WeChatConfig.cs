using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Models
{
    /// <summary>
    /// 微信公众号配置
    /// </summary>
    public class WeChatConfig
    {
        /// <summary>
        /// 客户端名称
        /// </summary>
        public String ClientName { get; set; }
        /// <summary>
        /// 平台id
        /// </summary>
        public String MPlatformId { get; set; }
        /// <summary>
        /// AppID
        /// </summary>
        public String AppId { get; set; }
        /// <summary>
        /// App密钥
        /// </summary>
        public String AppSecret { get; set; }
        /// <summary>
        /// 令牌
        /// </summary>
        public String Token { get; set; }
        /// <summary>
        /// 密文解密密钥
        /// </summary>
        public String AESKey { get; set; }
    }
}
