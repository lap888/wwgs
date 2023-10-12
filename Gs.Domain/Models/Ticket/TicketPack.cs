using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Ticket
{

    /// <summary>
    /// 新人券包
    /// </summary>
    public class TicketPack
    {
        /// <summary>
        /// 份数
        /// </summary>
        public Int32 Shares { get; set; }

        /// <summary>
        /// MBM价
        /// </summary>
        public Decimal Candy { get; set; }

        /// <summary>
        /// 现金价
        /// </summary>
        public Decimal Cash { get; set; }
    }
}
