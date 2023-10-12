using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Models.BuyBack
{
    /// <summary>
    /// 成色
    /// </summary>
    public enum Condition
    {
        /// <summary>
        /// 全新
        /// </summary>
        [Description("全新")]
        BrandNew = 10,
        /// <summary>
        /// 九成新
        /// </summary>
        [Description("九成新")]
        AlmostNew = 9,
        /// <summary>
        /// 八成新
        /// </summary>
        [Description("八成新")]
        PracticallyNew = 8,
        /// <summary>
        /// 七成新及以下
        /// </summary>
        [Description("七成新及以下")]
        Other = 7
    }
}
