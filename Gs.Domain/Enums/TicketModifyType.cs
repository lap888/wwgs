using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 账户变更类型
    /// </summary>
    public enum TicketModifyType
    {
        /// <summary>
        /// 兑换新人券 {张数}
        /// </summary>
        [Description("兑换新人券 {0}张")]
        TICKET_SUBSCRIBE = 1,

        /// <summary>
        /// {会员昵称}使用了{1}张新人券
        /// </summary>
        [Description("{0}使用了{1}张新人券")]
        TICKET_USED = 2,

        /// <summary>
        /// 参与每日视频量化宝,获得{0}张新人券
        /// </summary>
        [Description("参与每日视频量化宝,获得{0}张新人券")]
        TICKET_TASK = 3,
    }
}
