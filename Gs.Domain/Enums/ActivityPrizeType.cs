using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 奖品类型
    /// </summary>
    public enum ActivityPrizeType
    {
        /// <summary>
        /// 空奖
        /// </summary>
        None = 0,

        /// <summary>
        /// NW
        /// </summary>
        Candy = 1,

        /// <summary>
        /// 果皮
        /// </summary>
        Peel = 2,

        /// <summary>
        /// 量化宝
        /// </summary>
        Task = 3,

        /// <summary>
        /// 实物
        /// </summary>
        Stuff = 4,

        /// <summary>
        /// 券
        /// </summary>
        Coupon = 5,
    }
}
