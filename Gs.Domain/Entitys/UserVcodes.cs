using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class UserVcodes
    {
        public uint Id { get; set; }
        public string Mobile { get; set; }
        public string MsgId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
