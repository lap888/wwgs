using Gs.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models
{
    /// <summary>
    /// 量化宝信息
    /// </summary>
    public class MiningInfo : MiningBase
    {
        public Int64 Id { get; set; }
        public Int64 UserId { get; set; }
        public String Nick { get; set; }
        public String Mobile { get; set; }
        public DateTime BeginDate { get; set; }
        public Int32 Duration { get; set; }
        public DateTime ExpiryDate { get; set; }
        public MiningStatus State { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpTime { get; set; }
        public MiningSource Source { get; set; }
    }
}
