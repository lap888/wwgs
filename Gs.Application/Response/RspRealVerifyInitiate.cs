using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Response
{
    /// <summary>
    /// 发起实名认证
    /// </summary>
    public class RspRealVerifyInitiate : Utils.RealVerifyResponse
    {
        /// <summary>
        /// 响应内容
        /// </summary>
        [JsonProperty("data")]
        public RealVerifyContent Data { get; set; }=new RealVerifyContent();
    }

    /// <summary>
    /// 响应主题
    /// </summary>
    public class RealVerifyContent
    {
        /// <summary>
        /// 认证ID，刷脸认证唯一标识。
        /// </summary>
        [JsonProperty("certifyId")]
        public String CertifyId { get; set; }

        /// <summary>
        /// 认证流程入口 URL。
        /// </summary>
        [JsonProperty("certifyUrl")]
        public String CertifyUrl { get; set; }
    }
}
