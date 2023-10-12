using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Gs.Application.Response
{
    /// <summary>
    /// 下单
    /// </summary>
    [Serializable]
    [XmlRoot("xml")]
    public class RspWepaySubmit : WeResponse
    {
        /// <summary>
        /// 小程序ID
        /// </summary>
        [XmlElement("appid")]
        public string AppId { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        [XmlElement("mch_id")]
        public string MchId { get; set; }

        /// <summary>
        /// 调用接口提交的交易类型，取值如下：JSAPI，NATIVE，APP，详细说明见
        /// </summary>
        [XmlElement("trade_type")]
        public string TradeType { get; set; }

        /// <summary>
        /// 微信生成的预支付回话标识，用于后续接口调用中使用，该值有效期为2小时
        /// </summary>
        [XmlElement("prepay_id")]
        public string PrepayId { get; set; }

        /// <summary>
        /// 设备号
        /// </summary>
        [XmlElement("device_info")]
        public string DeviceInfo { get; set; }
    }
}
