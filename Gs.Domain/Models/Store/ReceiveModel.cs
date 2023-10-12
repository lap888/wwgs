using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Store
{
    /// <summary>
    /// 收货模型
    /// </summary>
    public class ReceiveModel
    {
        /// <summary>
        /// 会员编号
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }
    }
}
