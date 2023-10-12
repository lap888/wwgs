using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Gs.Application.Response
{
    /// <summary>
    /// 关闭订单
    /// </summary>
    [Serializable]
    [XmlRoot("xml")]
    public class RspWepayClose : WeResponse
    {
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
    }
}
