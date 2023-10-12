using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class LoginHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Mobile { get; set; }
        public string UniqueId { get; set; }
        public string SystemName { get; set; }
        public string SystemVersion { get; set; }
        public string DeviceName { get; set; }
        public string AppVersion { get; set; }
        public int UnLockCount { get; set; }
        public DateTime Ctime { get; set; }
        public DateTime Utime { get; set; }
    }
}
