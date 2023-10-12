using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class UserRelation
    {
        public long UserId { get; set; }
        public long ParentId { get; set; }
        public int RelationLevel { get; set; }
        public string Topology { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
