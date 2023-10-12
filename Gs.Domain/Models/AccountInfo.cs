using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models
{
    public class AccountInfo
    {
        public long AccountId { get; set; }
        public long UserId { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal Balance { get; set; }
        /// <summary>
        /// 可用
        /// </summary>
        public decimal Usable { get; set; }
        public decimal Frozen { get; set; }
        public DateTime ModifyTime { get; set; }

    }
}
