using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Response
{
    /// <summary>
    /// 手机号归属地
    /// </summary>
    public class RspMobileInfo
    {
        /// <summary>
        /// 运营商名称
        /// </summary>
        [JsonProperty("sp")]
        public String Operator { get; set; }

        /// <summary>
        /// 城市编码
        /// </summary>
        [JsonProperty("city_code")]
        public String CityCode { get; set; }

        /// <summary>
        /// 可充值面值
        /// </summary>
        [JsonProperty("face_value")]
        public List<Decimal> FaceValue { get; set; }

        /// <summary>
        /// 城市名称
        /// </summary>
        [JsonProperty("city")]
        public String City { get; set; }

        /// <summary>
        /// 省份名称
        /// </summary>
        [JsonProperty("province")]
        public String Province { get; set; }

        /// <summary>
        /// 运营商类型
        /// </summary>
        [JsonProperty("sp_type")]
        public String SpType { get; set; }

    }
}
