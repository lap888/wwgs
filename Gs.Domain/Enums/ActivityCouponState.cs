using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 活动券
    /// </summary>
    public enum ActivityCouponState
    {
        /// <summary>
        /// 全部
        /// </summary>
        [Description("未知")]
        All = 0,

        /// <summary>
        /// 未使用
        /// </summary>
        [Description("未使用")]
        Unused = 1,

        /// <summary>
        /// 使用中
        /// </summary>
        [Description("使用中")]
        Using = 2,

        /// <summary>
        /// 已使用
        /// </summary>
        [Description("已使用")]
        Used = 3,

        /// <summary>
        /// 已过期
        /// </summary>
        [Description("已过期")]
        Expired = 4
    }
}
