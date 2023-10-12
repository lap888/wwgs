using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class NoticeInfos
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public DateTime? CeratedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDel { get; set; }
        public string Remark { get; set; }
    }
}
