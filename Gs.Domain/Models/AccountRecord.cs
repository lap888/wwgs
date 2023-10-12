using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models
{
    /// <summary>
    /// 账户记录
    /// </summary>
    public class AccountRecord
    {
        public Int64 RecordId { get; set; }
        public Int64 AccountId { get; set; }
        public Decimal PreChange { get; set; }
        public Decimal Incurred { get; set; }
        public Decimal PostChange { get; set; }
        public Int32 ModifyType { get; set; }
        public String ModifyDesc { get; set; }
        public DateTime ModifyTime { get; set; }

    }
}
