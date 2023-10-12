using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Ticket
{
    public class TicketModel
    {
        public long AccountId { get; set; }
        /// <summary>
        /// 余额
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public Int32 State { get; set; }
        /// <summary>
        /// 观看次数
        /// </summary>
        public Int32 WatchCunt { get; set; }
        /// <summary>
        /// 套餐包
        /// </summary>
        public List<TicketPack> Package { get; set; }
        /// <summary>
        /// 规则
        /// </summary>
        public String Rules { get; set; }
    }
}
