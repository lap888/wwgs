using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 活跃度
    /// </summary>
    public enum ActiveModifyType
    {
        /// <summary>
        /// 兑换 {彩色量化宝}
        /// </summary>
        [Description("兑换 {0}")]
        EXCHANGE_MINER = 1,

        /// <summary>
        /// {彩色量化宝} 到期
        /// </summary>
        [Description("{0} 到期")]
        MINING_EXPIRED = 2,
    }
}
