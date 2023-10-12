using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 
    /// </summary>
    public enum MiningSource
    {
        /// <summary>
        /// 新人赠送
        /// </summary>
        [Description("新人赠送")]
        NEW_PEOPLE_GIVE = 1,

        /// <summary>
        /// 兑换量化宝
        /// </summary>
        [Description("兑换量化宝")]
        EXCHANGE_MINER = 2,

        /// <summary>
        /// 系统奖励
        /// </summary>
        [Description("系统奖励")]
        SYSTEM_AWARD = 3,
    }
}
