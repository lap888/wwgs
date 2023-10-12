using Gs.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class PayRecord
    {
        public long PayId { get; set; }
        public long UserId { get; set; }
        public PayChannel Channel { get; set; }
        public int Currency { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public ActionType ActionType { get; set; }
        public string Custom { get; set; }
        public PayStatus PayStatus { get; set; }
        public string ChannelUid { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? ModifyTime { get; set; }
    }
}
