using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 交易状态
    /// </summary>
    public enum TradeState
    {
        /// <summary>
        /// 全部
        /// </summary>
        All = -1,

        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled = 0,

        /// <summary>
        /// 正常
        /// </summary>
        Normal = 1,

        /// <summary>
        /// 待付款
        /// </summary>
        WaitPay = 2,

        /// <summary>
        /// 已付款
        /// </summary>
        AlreadyPay = 3,

        /// <summary>
        /// 正常
        /// </summary>
        Completed = 4,

        /// <summary>
        /// 申诉
        /// </summary>
        Appeal = 5,

        /// <summary>
        /// 正常
        /// </summary>
        TimeOut = 6,

    }
}
