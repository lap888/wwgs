using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Models.BuyBack
{
    /// <summary>
    /// 回购类型
    /// </summary>
    public enum RepoType
    {
        /// <summary>
        /// 夏装
        /// </summary>
        [Description("夏装")]
        SummerWear = 1,

        /// <summary>
        /// 旧衣服
        /// </summary>
        [Description("旧衣服")]
        OldClothes = 2,

        /// <summary>
        /// 闲置物
        /// </summary>
        [Description("闲置物")]
        IdleProperty = 3,

    }
}
