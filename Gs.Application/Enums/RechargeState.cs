using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Enums
{
    /// <summary>
    /// 充值状态
    /// </summary>
    public enum RechargeState
    {
        /// <summary>
        /// 未知
        /// </summary>
        unknown = 0,

        /// <summary>
        /// 成功
        /// </summary>
        success = 1000,

        /// <summary>
        /// 处理中
        /// </summary>
        processing = 1001,

        /// <summary>
        /// 失败
        /// </summary>
        failed = 1005,

        /// <summary>
        /// 未处理
        /// </summary>
        untreated = 1007,
    }
}
