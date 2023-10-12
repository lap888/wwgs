using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class StoreItem
    {
        public long Id { get; set; }
        public int CateId { get; set; }
        public string Name { get; set; }
        public string Keywords { get; set; }
        public string MetaTitle { get; set; }
        public string Sku { get; set; }
        public string Description { get; set; }
        public string Images { get; set; }
        public decimal OldPrice { get; set; }
        public decimal PointsPrice { get; set; }
        public decimal ServicePrice { get; set; }
        public int Stock { get; set; }
        public bool Published { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Remark { get; set; }
    }
}
