using Gs.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class UserAccountTicketRecord
    {
        public long RecordId { get; set; }
        public long AccountId { get; set; }
        public decimal PreChange { get; set; }
        public decimal Incurred { get; set; }
        public decimal PostChange { get; set; }
        public TicketModifyType ModifyType { get; set; }
        public string ModifyDesc { get; set; }
        public DateTime ModifyTime { get; set; }
    }
}
