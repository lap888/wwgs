using Gs.Application.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gs.Application
{
    /// <summary>
    /// 福禄充值
    /// </summary>
    public interface IFuluRecharge
    {
        /// <summary>
        /// 请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<FuluResponse<T>> Execute<T>(IFuluRequest<FuluResponse<T>> request);
    }
}
