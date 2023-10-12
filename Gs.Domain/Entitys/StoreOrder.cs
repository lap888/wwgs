using Gs.Domain.Models.Store;
using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class StoreOrder
    {
        public long Id { get; set; }
        public long ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemPic { get; set; }
        public decimal PayIntegral { get; set; }
        public string PayNo { get; set; }
        public decimal ServicePrice { get; set; }
        public long UserId { get; set; }
        public long StoreId { get; set; }
        public string ExpressNum { get; set; }
        public string Contacts { get; set; }
        public string ContactTel { get; set; }
        public string ShippingAddress { get; set; }
        public OrderStatus State { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
