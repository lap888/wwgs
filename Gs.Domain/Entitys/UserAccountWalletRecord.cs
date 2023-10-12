using System;
using Gs.Domain.Enums;

namespace Gs.Domain.Entity
{
    public partial class UserAccountWalletRecord
    {
        public long RecordId { get; set; }
        public long AccountId { get; set; }
        public decimal PreChange { get; set; }
        public decimal Incurred { get; set; }
        public decimal PostChange { get; set; }
        public AccountModifyType ModifyType { get; set; }
        public string ModifyDesc { get; set; }
        public DateTime ModifyTime { get; set; }
    }
}
