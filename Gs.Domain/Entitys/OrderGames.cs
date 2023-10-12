using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class OrderGames
    {
        public uint Id { get; set; }
        public string GameAppid { get; set; }
        public string OrderId { get; set; }
        public string Uuid { get; set; }
        public int? UserId { get; set; }
        public decimal? Candy { get; set; }
        public decimal? CandyAmount { get; set; }
        public decimal? OrderAmount { get; set; }
        public decimal? RealAmount { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
