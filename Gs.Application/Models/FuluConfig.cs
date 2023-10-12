using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Models
{
    /// <summary>
    /// 福禄充值配置
    /// </summary>
    public class FuluConfig
    {
        /// <summary>
        /// 请求池名
        /// </summary>
        public String ClientName { get; set; }
        /// <summary>
        /// 请求地址
        /// </summary>
        public String ApiUrl { get; set; }
        /// <summary>
        /// 请求key
        /// </summary>
        public String AppKey { get; set; }
        /// <summary>
        /// 请求secret
        /// </summary>
        public String AppSecret { get; set; }
    }
}
