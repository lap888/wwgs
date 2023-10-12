using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class UserDigMinerRecord
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public decimal Schedule { get; set; }
        public int Source { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Remark { get; set; }
    }
}
