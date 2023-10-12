using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Response
{
    /// <summary>
    /// 账户信息
    /// </summary>
    public class RspFuluBalance
    {
        /// <summary>
        /// 商户名称
        /// </summary>
        [JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// 商户余额（单位：元）
        /// </summary>
        [JsonProperty("balance")]
        public Decimal Balance { get; set; }

        /// <summary>
        /// 商户状态：1启用，0禁用
        /// </summary>
        [JsonProperty("is_open")]
        public Boolean IsOpen { get; set; }

    }
}
