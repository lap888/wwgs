using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class RechargeOrder
    {
        public long Id { get; set; }
        public string OrderNo { get; set; }
        public string ChannelNo { get; set; }
        public int OrderType { get; set; }
        public long UserId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string FaceValue { get; set; }
        public string Account { get; set; }
        public decimal Price { get; set; }
        public int BuyNum { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal PayCandy { get; set; }
        public decimal PayPeel { get; set; }
        public int State { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string Remark { get; set; }
    }
}
