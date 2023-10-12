using Gs.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Admin
{
    public class AdminUser
    {
        public string Id { get; set; }
        public string LoginName { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public int Gender { get; set; }
        public string VerCode { get; set; }
        public int ErrCount { get; set; }
        public string OldPassword { get; set; }
        public string OpenId { get; set; }
        public AccountType AccountType { get; set; }
        public string IdCard { get; set; }

    }
}
