using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models
{
    /// <summary>
    /// 分红记录
    /// </summary>
    public class DividendRecord
    {
        public String Title { get; set; }

        public String Desc { get; set; }

        public Decimal Amount { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
