using Gs.Application.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gs.Application
{
    /// <summary>
    /// 支付宝支付
    /// </summary>
    public interface IAlipay
    {
        /// <summary>
        /// 执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AlipayResult<T>> Execute<T>(IAlipayRequest<AlipayResult<T>> request) where T : AlipayResponse, new();

        /// <summary>
        /// 获取APP签名串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<String> GetSignStr<T>(IAlipayRequest<AlipayResult<T>> request) where T : AlipayResponse, new();

    }
}
