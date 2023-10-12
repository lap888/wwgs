using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 加成活跃度
    /// </summary>
    public enum AdditionModifyType
    {
        /// <summary>
        /// 下级兑换 {彩色量化宝}
        /// </summary>
        [Description("下级兑换 {0}")]
        SUBORDINATE_ADDITION = 1,

        /// <summary>
        /// 下级 {彩色量化宝} 到期
        /// </summary>
        [Description("下级 {0} 到期")]
        SUBORDINATE_ADDITION_EXPIRED = 2,

        /// <summary>
        /// 区县会员兑换 {彩色量化宝}
        /// </summary>
        [Description("区县会员兑换 {0}")]
        DISTRICT_ADDITION = 3,

        /// <summary>
        /// 区县会员 {彩色量化宝} 到期
        /// </summary>
        [Description("区县会员 {0} 到期")]
        DISTRICT_ADDITION_EXPIRED = 4,

        /// <summary>
        /// 城内会员兑换 {彩色量化宝}
        /// </summary>
        [Description("城内会员兑换 {0}")]
        CITY_ADDITION = 5,

        /// <summary>
        /// 城内会员 {彩色量化宝} 到期
        /// </summary>
        [Description("城内会员 {0} 到期")]
        CITY_ADDITION_EXPIRED = 6,
    }
}
