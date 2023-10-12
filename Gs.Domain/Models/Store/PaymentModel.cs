using Gs.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Store
{
    /// <summary>
    /// 支付模型
    /// </summary>
    public class PaymentModel
    {
        /// <summary>
        /// 会员编号
        /// </summary>
        public Int64 UserId { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public String OrderNo { get; set; }

        /// <summary>
        /// 支付类型
        /// </summary>
        public PayChannel PayType { get; set; }
    }
}
