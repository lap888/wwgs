using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Gs.Application.Response
{
    /// <summary>
    /// 付款至零钱
    /// </summary>
    [Serializable]
    [XmlRoot("xml")]
    public class RspWepayTransfers : WeResponse
    {
        /// <summary>
        /// 申请商户号的appid或商户号绑定的appid（企业号corpid即为此appId）
        /// </summary>
        [XmlElement("mch_appid")]
        public String MchAppid { get; set; }
        /// <summary>
        /// 商户号
        /// </summary>
        [XmlElement("mch_id")]
        public String MchId { get; set; }
        /// <summary>
        /// 微信支付分配的终端设备号，
        /// </summary>
        [XmlElement("device_info")]
        public String DeviceInfo { get; set; }
        /// <summary>
        /// 商户订单号，需保持历史全局唯一性(只能是字母或者数字，不能包含有其它字符)
        /// </summary>
        [XmlElement("partner_trade_no")]
        public String PartnerTradeNo { get; set; }
        /// <summary>
        /// 企业付款成功，返回的微信付款单号
        /// </summary>
        [XmlElement("payment_no")]
        public String PaymentNo { get; set; }
        /// <summary>
        /// 企业付款成功时间
        /// </summary>
        [XmlElement("payment_time")]
        public String PaymentTime { get; set; }
    }
}
