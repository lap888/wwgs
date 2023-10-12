using Gs.Application.Response;
using Gs.Application.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Request
{
    public class ReqWepayTransfers : IWePayRequest<RspWepayTransfers>
    {
        public string GetUrl()
        {
            return "https://api.mch.weixin.qq.com/mmpaymkttransfers/promotion/transfers";
        }

        public bool UseCert()
        {
            return true;
        }

        /// <summary>
        /// 商户账号appid
        /// </summary>
        public String AppId { get; set; }
        /// <summary>
        /// 商户号
        /// </summary>
        public String MchId { get; set; }
        /// <summary>
        /// 用户openid	
        /// </summary>
        public String OpenId { get; set; }
        /// <summary>
        /// NO_CHECK：不校验真实姓名
        /// FORCE_CHECK：强校验真实姓名
        /// </summary>
        public Boolean CheckName { get; set; }
        /// <summary>
        /// 收款用户姓名
        /// </summary>
        public String ReUserName { get; set; }
        /// <summary>
        /// 企业付款金额，单位为分
        /// </summary>
        public Int32 Amount { get; set; }
        /// <summary>
        /// 企业付款备注，必填。
        /// 注意：备注中的敏感词会被转成字符*
        /// </summary>
        public String Desc { get; set; }
        /// <summary>
        /// 商户订单号，需保持唯一性
        /// (只能是字母或者数字，不能包含有其它字符)
        /// </summary>
        public String PartnerTradeNo { get; set; }
        /// <summary>
        /// 微信支付分配的终端设备号
        /// </summary>
        public String DeviceInfo { get; set; }
        /// <summary>
        /// 该IP同在商户平台设置的IP白名单中的IP没有关联，该IP可传用户端或者服务端的IP。
        /// </summary>
        public String SpbillCreateIp { get; set; }

        public WeXmlDoc GetXmlDoc()
        {
            WeXmlDoc XmlDoc = new WeXmlDoc();
            XmlDoc.Add("mch_appid", this.AppId);
            XmlDoc.Add("mchid", this.MchId);
            XmlDoc.Add("openid", this.OpenId);
            XmlDoc.Add("check_name", this.CheckName ? "FORCE_CHECK" : "NO_CHECK");
            XmlDoc.Add("re_user_name", this.ReUserName);
            XmlDoc.Add("amount", this.Amount);
            XmlDoc.Add("desc", this.Desc);
            XmlDoc.Add("partner_trade_no", this.PartnerTradeNo);
            XmlDoc.Add("device_info", this.DeviceInfo);
            XmlDoc.Add("spbill_create_ip", this.SpbillCreateIp);
            return XmlDoc;
        }
    }
}
