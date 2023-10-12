using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// NW==贡献值
    /// </summary>
    public enum CottonModifyType
    {
        /// <summary>
        /// 系统默认 {0}
        /// </summary>
        [Description("系统默认 {0}")]
        ALL = 0,
        /// <summary>
        /// 兑换 彩色量化宝
        /// </summary>
        [Description("兑换 {0}")]
        EXCHANGE_MINER = 1,

        /// <summary>
        /// 量化宝产出 {0.0000} NW
        /// </summary>
        [Description("量化宝产出 {0} NW")]
        DIG_MINER = 2,

        /// <summary>
        /// 兑换 {0.0000} 积分
        /// </summary>
        [Description("兑换 {0} 积分")]
        EXCHANGE_INTEGRAL = 3,

        /// <summary>
        /// {0.0000} 福利
        /// </summary>
        [Description("{0} 福利")]
        TALENT_BONUS = 4,

        /// <summary>
        /// 消耗:{0}NW,兑换:{1}Gas,手续费:{2}
        /// </summary>
        [Description("消耗:{0}NW,兑换:{1}Gas,手续费:{2}")]
        EXCHANGE_CONCH = 5,

        /// <summary>
        /// 会员 {昵称} 认证
        /// </summary>
        [Description("会员 {0} 认证")]
        MEMBER_REAL_NAME = 6,
        /// <summary>
        /// 实名赠送
        /// </summary>
        [Description("实名赠送")]
        REAL_NAME = 7,
        

    }
}
