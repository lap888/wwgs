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
    public class RspWepayTransferQuery : WeResponse
    {
        /// <summary>
        /// 商户订单号，需保持历史全局唯一性(只能是字母或者数字，不能包含有其它字符)
        /// </summary>
        [XmlElement("partner_trade_no")]
        public String PartnerTradeNo { get; set; }

        /// <summary>
        /// 申请商户号的appid或商户号绑定的appid（企业号corpid即为此appId）
        /// </summary>
        [XmlElement("appid")]
        public String Appid { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        [XmlElement("mch_id")]
        public String MchId { get; set; }

        /// <summary>
        /// 转账的openid
        /// </summary>
        [XmlElement("openid")]
        public String Openid { get; set; }

        /// <summary>
        /// SUCCESS:转账成功
        /// FAILED:转账失败
        /// PROCESSING:处理中
        /// </summary>
        [XmlElement("detail_id")]
        public String ChannelNo { get; set; }

        /// <summary>
        /// 失败原因
        /// </summary>
        [XmlElement("reason")]
        public String Reason { get; set; }

        /// <summary>
        /// 调用企业付款API时，微信系统内部产生的单号
        /// </summary>
        [XmlElement("status")]
        public String Status { get; set; }

        /// <summary>
        /// 收款用户姓名
        /// </summary>
        [XmlElement("transfer_name")]
        public String TransferName { get; set; }

        /// <summary>
        /// 付款金额单位为“分”
        /// </summary>
        [XmlElement("payment_amount")]
        public Int32 PaymentAmount { get; set; }

        /// <summary>
        /// 发起转账的时间
        /// </summary>
        [XmlElement("transfer_time")]
        public String TransferTime { get; set; }

        /// <summary>
        /// 企业付款成功时间
        /// </summary>
        [XmlElement("payment_time")]
        public String PaymentTime { get; set; }
    }
}
