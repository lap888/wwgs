using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Admin
{
    public class RoleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Menus { get; set; }
    }
}
