using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class UserWithdrawHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int WithdrawType { get; set; }
        public string WithdrawTo { get; set; }
        public string OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string Content { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string FailReason { get; set; }
    }
}
