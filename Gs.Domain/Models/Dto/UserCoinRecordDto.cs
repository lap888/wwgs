using Gs.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Dto
{
    public class UserCoinRecordDto
    {
        public long RecordId { get; set; }
        public long AccountId { get; set; }
        public decimal PreChange { get; set; }
        public decimal Incurred { get; set; }
        public decimal PostChange { get; set; }
        public string ModifyDesc { get; set; }

        public CottonModifyType ModifyType { get; set; }
        public DateTime ModifyTime { get; set; }
        public string CoinType { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
    }
}
