using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Utils
{
    public interface IWePayRequest<out T> where T : Response.WeResponse, new()
    {
        /// <summary>
        /// 获取请求地址
        /// </summary>
        /// <returns></returns>
        String GetUrl();

        /// <summary>
        /// 使用证书
        /// </summary>
        /// <returns></returns>
        Boolean UseCert();

        /// <summary>
        /// 商户号的appid
        /// </summary>
        String AppId { get; set; }

        /// <summary>
        /// 微信支付分配的商户号
        /// </summary>
        String MchId { get; set; }

        /// <summary>
        /// 获取请求参数
        /// </summary>
        /// <returns></returns>
        WeXmlDoc GetXmlDoc();
    }
}
