using Gs.Domain.Models.BuyBack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Community
{
    public class AssessOrder
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Nick { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string StorePic { get; set; }
        public string StoreAddress { get; set; }
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
