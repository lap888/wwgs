using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models
{
    public class TaskProgress
    {
        public Int64 Id { get; set; }
        public Int64 UserId { get; set; }
        public Decimal Schedule { get; set; }
        public Int32 Source { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime UpdateDate { get; set; }
        public String Remark { get; set; }
    }
}
