using System;

namespace Gs.Domain.Models.Dto
{
    public class AuthenticationInfosDto
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
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int AuditState { get; set; }
    }
}