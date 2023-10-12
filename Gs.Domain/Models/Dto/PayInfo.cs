using Gs.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Models.Dto
{
    public class PayInfo
    {
        public long PayId { get; set; }

        public long UserId { get; set; }

        public PayChannel Channel { get; set; }

        public Currency Currency { get; set; }

        public decimal Amount { get; set; }

        public decimal Fee { get; set; }

        public ActionType ActionType { get; set; }

        public string Custom { get; set; }

        public PayStatus PayStatus { get; set; }

        public string ChannelUID { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? ModifyTime { get; set; }
    }
}
