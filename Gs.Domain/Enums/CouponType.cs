using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 活动券类型
    /// </summary>
    public enum CouponType
    {
        /// <summary>
        /// 全部
        /// </summary>
        [Description("未知")]
        All = 0,

        /// <summary>
        /// 哟帮量化宝体验券
        /// </summary>
        [Description("哟帮量化宝体验券")]
        YoBangExperience = 1,

        /// <summary>
        /// 哟帮免手续费券
        /// </summary>
        [Description("哟帮免手续费券")]
        YoBangAbsolveFee = 2,

        /// <summary>
        /// 一星达人分红体验券
        /// </summary>
        [Description("一星达人分红体验券")]
        OneStarDividend = 3,

        /// <summary>
        /// 二星达人分红体验券
        /// </summary>
        [Description("二星达人分红体验券")]
        TwoStarDividend = 4,

        /// <summary>
        /// 三星达人分红体验券
        /// </summary>
        [Description("三星达人分红体验券")]
        ThreeStarDividend = 5,

        /// <summary>
        /// 现金分红体验券
        /// </summary>
        [Description("现金分红体验券")]
        CashDividend = 6,

        /// <summary>
        /// 量化宝突破券
        /// </summary>
        [Description("量化宝突破券")]
        TaskOverfulfil = 7
    }
}
