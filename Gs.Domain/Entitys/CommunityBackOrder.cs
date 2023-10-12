using System;
using Gs.Domain.Models.BuyBack;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class CommunityBackOrder
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long StoreId { get; set; }
        public RepoType RepoType { get; set; }
        public ItemGrade ItemGrade { get; set; }
        public int ItemCunt { get; set; }
        public string ItemBrand { get; set; }
        public decimal UnitPrice { get; set; }
        public Condition Condition { get; set; }
        public decimal AssessIntegral { get; set; }
        public ShippingMethod ShipMethod { get; set; }
        public BackStauts State { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
