using System;

namespace Gs.Domain.Models.Dto
{
    public class UserDto : BaseModel
    {
        public long Id { get; set; }
        public int Status { get; set; }
        public int AuditState { get; set; }
        public string Name { get; set; }
        public string TrueName { get; set; }
        public string Rcode { get; set; }
        public string ContryCode { get; set; }
        public string Mobile { get; set; }
        public string InviterMobile { get; set; }
        public string Password { get; set; }
        public int IsPay { get; set; }
        public string PasswordSalt { get; set; }
        public DateTime Ctime { get; set; }
        public DateTime Utime { get; set; }
        public int? CCount { get; set; }
        public string AvatarUrl { get; set; }
        public decimal? Golds { get; set; }
        public string Uuid { get; set; }
        public string Level { get; set; }
        public string TradePwd { get; set; }
        public string Alipay { get; set; }
        public String AlipayUid { get; set; }
        public string Remark { get; set; }
        public decimal CoinBalance { get; set; }

        public decimal HonorBalance { get; set; }
    }
}