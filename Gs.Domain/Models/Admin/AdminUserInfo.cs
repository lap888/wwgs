using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Admin
{
    public class AdminUserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Nickname { get; set; }
        public int RoleId { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
