using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 荣誉值
    /// </summary>
    public enum HonorModifyType
    {
        /// <summary>
        /// 会员 {昵称} 认证
        /// </summary>
        [Description("会员 {0} 认证")]
        MEMBER_REAL_NAME = 1,

        /// <summary>
        /// 兑换 {彩色量化宝}
        /// </summary>
        [Description("兑换 {0}")]
        EXCHANGE_MINER = 2,

        /// <summary>
        /// {0}NW兑换
        /// </summary>
        [Description("{0}NW兑换")]
        EXCHANGE_CONCH = 3,

        /// <summary>
        /// 购买:{0}NW,奖励:{1}荣誉值
        /// </summary>
        [Description("购买:{0}NW")]
        Buy_Reward_Honor = 4,

        [Description("卖掉:{0}NW 消耗:{1}Gas")]
        Seller_Sub_Honor = 5,
    }
}
