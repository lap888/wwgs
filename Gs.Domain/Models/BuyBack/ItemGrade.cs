using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Models.BuyBack
{
    /// <summary>
    /// 物品等级
    /// </summary>
    public enum ItemGrade
    {
        /// <summary>
        /// 普通货
        /// </summary>
        [Description("普通类")]
        Ordinary = 1,

        /// <summary>
        /// 名牌货
        /// </summary>
        [Description("名牌货")]
        FamousBrand = 2,

    }
}
