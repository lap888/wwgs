using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;

namespace Gs.Domain.Models.BuyBack
{
    /// <summary>
    /// 回购订单
    /// </summary>
    public enum BackStauts
    {
        /// <summary>
        /// 已寄送
        /// </summary>
        [Description("已寄送")]
        Sending = 2,

        /// <summary>
        /// 已收货
        /// </summary>
        [Description("已收货")]
        Received = 3,

        /// <summary>
        /// 已完成
        /// </summary>
        [Description("已完成")]
        Completed = 4,
    }
}
