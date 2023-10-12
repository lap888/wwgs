using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Entity
{
    public class UserFeedback
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Title { get; set; }
        public string Images { get; set; }
        public string Content { get; set; }
        public int State { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }

    }
}
