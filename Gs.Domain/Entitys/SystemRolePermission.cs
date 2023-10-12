using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Entity
{
    public partial class SystemRolePermission
    {
        public int RoleId { get; set; }
        public string ActionId { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
