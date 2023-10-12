using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Models
{
    /// <summary>
    /// 实名认证配置
    /// </summary>
    public class RealVerifyConfig
    {
        /// <summary>
        /// Client标示
        /// </summary>
        public String ClientName { get; set; }

        /// <summary>
        /// 请求地址
        /// </summary>
        public String ApiUrl { get; set; }

        /// <summary>
        /// KeyID
        /// </summary>
        public String AccessKey { get; set; }

        /// <summary>
        /// 秘钥
        /// </summary>
        public String AccessSecret { get; set; }

        /// <summary>
        /// 认证场景ID，在控制台创建认证场景后自动生成。
        /// </summary>
        public String SceneId { get; set; }
    }
}
