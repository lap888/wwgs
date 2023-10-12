using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 奖品领取状态
    /// </summary>
    public enum ActivityReceiveState
    {
        /// <summary>
        /// 未领取
        /// </summary>
        NotReceived = 0,

        /// <summary>
        /// 待发货
        /// </summary>
        Received = 1,

        /// <summary>
        /// 待发货
        /// </summary>
        BeShipped = 2,

        /// <summary>
        /// 已发货
        /// </summary>
        Shipped = 3,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed = 4
    }
}
