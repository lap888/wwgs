using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Models.BuyBack
{
    /// <summary>
    /// 送达方式
    /// </summary>
    public enum ShippingMethod
    {
        /// <summary>
        /// 个人邮寄
        /// </summary>
        [Description("个人邮寄")]
        PersonalPost = 1,

        /// <summary>
        /// 上门取件
        /// </summary>
        [Description("上门取件")]
        PickUp = 2,

    }
}
