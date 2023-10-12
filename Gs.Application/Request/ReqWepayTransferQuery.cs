using Gs.Application.Response;
using Gs.Application.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Request
{
    public class ReqWepayTransferQuery : IWePayRequest<RspWepayTransferQuery>
    {
        public string GetUrl()
        {
            return "https://api.mch.weixin.qq.com/mmpaymkttransfers/gettransferinfo";
        }

        public bool UseCert()
        {
            return true;
        }

        /// <summary>
        /// 商户号的appid
        /// </summary>
        public String AppId { get; set; }

        /// <summary>
        /// 微信支付分配的商户号
        /// </summary>
        public String MchId { get; set; }

        /// <summary>
        /// 商户调用企业付款API时使用的商户订单号
        /// </summary>
        public String PartnerTradeNo { get; set; }

        public WeXmlDoc GetXmlDoc()
        {
            WeXmlDoc XmlDoc = new WeXmlDoc();
            XmlDoc.Add("appid", this.AppId);
            XmlDoc.Add("mch_id", this.MchId);
            XmlDoc.Add("partner_trade_no", this.PartnerTradeNo);

            return XmlDoc;
        }
    }
}
