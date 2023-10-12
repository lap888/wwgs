using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 积分
    /// </summary>
    public enum IntegralModifyType
    {
        /// <summary>
        /// {0}评估,获得
        /// </summary>
        [Description("{0}评估,获得")]
        OLD_ASSESS = 1,

        /// <summary>
        /// 积分购物
        /// </summary>
        [Description("积分购物")]
        SHOPPING = 2,

        /// <summary>
        /// 兑换 {0.0000} 积分
        /// </summary>
        [Description("兑换 {0} 积分")]
        EXCHANGE_INTEGRAL = 3,
    }
}
