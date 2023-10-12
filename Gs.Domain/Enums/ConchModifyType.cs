using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// NW
    /// </summary>
    public enum ConchModifyType
    {
        /// <summary>
        /// 跨链转移:{0}NW
        /// </summary>
        [Description("跨链转移:{0}NW")]
        Sys_Ex = 0,
        /// <summary>
        /// 兑换:{0}NW
        /// </summary>
        [Description("兑换Gas消耗:{0}")]
        EXCHANGE_CONCH = 1,
        [Description("出售{0}个NW")]
        Coin_Sell_Coin = 2,
        [Description("购买{0}个NW")]
        Coin_buy_Coin = 3,
        [Description("订单号{0}的交易手续费{1}个USDT")]
        Coin_Sell_Sys_Fee = 4,

        /// <summary>
        /// 量化宝产出 {0.0000} NW
        /// </summary>
        [Description("量化宝产出 {0} NW")]
        DIG_MINER = 5,
        [Description("下级加成 {0} NW")]
        Inviter_DIG_MINER = 7,
        /// <summary>
        /// 兑换 彩色量化宝
        /// </summary>
        [Description("兑换 {0}")]
        EXCHANGE_MINER = 6,
        /// <summary>
        /// {0.0000} 福利
        /// </summary>
        [Description("{0} 福利")]
        TALENT_BONUS = 8,

    }
}
