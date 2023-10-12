using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Store
{
    /// <summary>
    /// 商品详情
    /// </summary>
    public class ItemDetail
    {
        public long Id { get; set; }
        public int CateId { get; set; }
        public string Name { get; set; }
        public string Keywords { get; set; }
        public string MetaTitle { get; set; }
        public string Sku { get; set; }
        public string Description { get; set; }
        public List<String> Images { get; set; }
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
