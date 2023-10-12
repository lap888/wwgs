using Gs.Application.Response;
using Gs.Application.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Request
{
    public class ReqWepayClose : IWePayRequest<RspWepayClose>
    {
        /// <summary>
        /// 请求地址
        /// </summary>
        /// <returns></returns>
        public string GetUrl()
        {
            return "https://api.mch.weixin.qq.com/pay/closeorder";
        }

        public bool UseCert()
        {
            return false;
        }

        /// <summary>
        /// 小程序ID
        /// </summary>
        public String AppId { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        public String MchId { get; set; }

        /// <summary>
        /// 商户订单号
        /// </summary>
        public String TradeNo { get; set; }

        /// <summary>
        /// 签名类型
        /// </summary>
        public String SignType { get; set; }

        /// <summary>
        /// 获取请求参数
        /// </summary>
        /// <returns></returns>
        public WeXmlDoc GetXmlDoc()
        {
            WeXmlDoc XmlDoc = new WeXmlDoc();
            XmlDoc.Add("appid", this.AppId);
            XmlDoc.Add("mch_id", this.MchId);
            XmlDoc.Add("out_trade_no", this.TradeNo);
            XmlDoc.Add("sign_type", this.SignType);

            return XmlDoc;
        }
    }
}
