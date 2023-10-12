using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class AuthenticationInfos
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public string TrueName { get; set; }
        public string Pic { get; set; }
        public string Pic1 { get; set; }
        public string Pic2 { get; set; }
        public string IdNum { get; set; }
        public int AuthType { get; set; }
        public string FailReason { get; set; }
        public string CertifyId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
