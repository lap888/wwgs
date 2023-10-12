using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gs.Domain.Entity
{
    public partial class SystemRoles
    {
        public SystemRoles()
        {
            AdminUser = new HashSet<SystemUser>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<SystemUser> AdminUser { get; set; }

        [NotMapped]
        public virtual List<string> Menus { get; set; }

    }
}
