using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Models
{
    public class WepayConfig
    {
        /// <summary>
        /// 请求标识
        /// </summary>
        public String ClientName { get; set; }
        /// <summary>
        /// 证书请求标识
        /// </summary>
        public String CertClient { get; set; }

        /// <summary>
        /// 应用标示
        /// </summary>
        public String AppId { get; set; }

        /// <summary>
        /// 应用秘钥
        /// </summary>
        public String AppSecret { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        public String MchId { get; set; }

        /// <summary>
        /// 秘钥
        /// </summary>
        public String ApiV3Key { get; set; }

        /// <summary>
        /// 证书路径
        /// </summary>
        public String CertPath { get; set; }

        /// <summary>
        /// 证书密码
        /// </summary>
        public String CertPass { get; set; }

        /// <summary>
        /// 异步通知地址
        /// </summary>
        public String NotifyUrl { get; set; }
    }
}
