using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Utils
{
    /// <summary>
    /// 福禄充值
    /// </summary>
    public interface IFuluRequest<out FuluResponse>
    {
        /// <summary>
        /// Method
        /// </summary>
        /// <returns></returns>
        String GetApiName();

        /// <summary>
        /// 请求参数
        /// </summary>
        /// <returns></returns>
        String GetBizContent();
    }
}
