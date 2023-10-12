using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models
{
    /// <summary>
    /// 订单申诉
    /// </summary>
    public class TradeAppeals
    {
        /// <summary>
        /// 编号
        /// </summary>
        public Int64 Id { get; set; }

        /// <summary>
        /// 订单Id
        /// </summary>
        public Int64 OrderId { get; set; }

        /// <summary>
        /// 申诉图
        /// </summary>
        public String PicUrl { get; set; }

        /// <summary>
        /// 申诉原因
        /// </summary>
        public String Description { get; set; }
    }
}
