using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Dto
{
    public class TransferOrder
    {
        public Int64 Id { get; set; }
        public String OrderNo { get; set; }
        public Int32 Type { get; set; }
        public Int64 YoUid { get; set; }
        public String YoUuid { get; set; }
        public Decimal Amount { get; set; }
        public Decimal Fee { get; set; }
        public String XyUuid { get; set; }
        public String XyOrderNo { get; set; }
        public Int32 State { get; set; }
        public DateTime CreateTime { get; set; }
        public String Remark { get; set; }
    }
}
