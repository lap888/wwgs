using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Models.Store
{
    /// <summary>
    /// 订单状态
    /// </summary>
    public enum OrderStatus
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
        /// 待收货
        /// </summary>
        [Description("待收货")]
        DELIVERED = 2,

        /// <summary>
        /// 已完成
        /// </summary>
        [Description("已完成")]
        COMPLETED = 3,
    }
}
