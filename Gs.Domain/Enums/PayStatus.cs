using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 支付状态
    /// </summary>
    public enum PayStatus
    {
        /// <summary>
        /// 无效
        /// </summary>
        [Description("无效")]
        INVALID = -1,
        /// <summary>
        /// 未支付
        /// </summary>
        [Description("未支付")]
        UN_PAID = 0,
        /// <summary>
        /// 已支付
        /// </summary>
        [Description("已支付")]
        PAID = 1,
        /// <summary>
        /// 已退款
        /// </summary>
        [Description("已退款")]
        REFUND = 2
    }
}
