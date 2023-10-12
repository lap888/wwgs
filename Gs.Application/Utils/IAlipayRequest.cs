using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Utils
{
    /// <summary>
    /// 支付宝支付
    /// </summary>
    /// <typeparam name="AlipayResult"></typeparam>
    public interface IAlipayRequest<out AlipayResult>
    {
        /// <summary>
        /// Method
        /// </summary>
        /// <returns></returns>
        String GetApiName();

        /// <summary>
        /// 公共请求参数
        /// </summary>
        /// <returns></returns>
        AliDictionary GetPublicParam();

        /// <summary>
        /// 请求参数
        /// </summary>
        /// <returns></returns>
        AliDictionary GetParam();
    }
}
