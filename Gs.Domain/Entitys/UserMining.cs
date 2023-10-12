using Gs.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Entity
{
    /// <summary>
    /// 量化宝
    /// </summary>
    public partial class UserMining
    {
        public Int64 Id { get; set; }
        public Int64 UserId { get; set; }
        public Int32 BaseId { get; set; }
        public DateTime BeginDate { get; set; }
        public Int32 Duration { get; set; }
        public DateTime ExpiryDate { get; set; }
        public MiningStatus State { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpTime { get; set; }
        public MiningSource Source { get; set; }
        public String Remark { get; set; }
    }
}
