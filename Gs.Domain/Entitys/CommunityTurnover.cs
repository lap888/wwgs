using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class CommunityTurnover
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public long StoreId { get; set; }
        public decimal Turnover { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
