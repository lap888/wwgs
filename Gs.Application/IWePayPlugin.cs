using Gs.Application.Response;
using Gs.Application.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gs.Application
{
    public interface IWePayPlugin
    {
        /// <summary>
        /// 请求
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<T> Execute<T>(IWePayRequest<T> request) where T : WeResponse, new();

        /// <summary>
        /// 生成支付串
        /// </summary>
        /// <param name="PrepayId"></param>
        /// <returns></returns>
        SortedDictionary<String, String> MakeSign(String PrepayId);

        /// <summary>
        /// 解析通知
        /// </summary>
        /// <param name="notify"></param>
        /// <returns></returns>
        RspWepayNotify Notify(String notify);
    }
}
