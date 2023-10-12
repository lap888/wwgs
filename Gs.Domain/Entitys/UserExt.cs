using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class UserExt
    {
        public ulong Id { get; set; }
        public long UserId { get; set; }
        public int TeamStart { get; set; }
        public int TeamCount { get; set; }
        public int AuthCount { get; set; }
        public int TeamCandyH { get; set; }
        public int BigCandyH { get; set; }
        public int LittleCandyH { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
