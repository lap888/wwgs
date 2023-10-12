using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Entitys
{
    public partial class CoinTrade
    {
        public int Id { get; set; }
        public string TradeNumber { get; set; }
        public int? BuyerUid { get; set; }
        public string BuyerAlipay { get; set; }
        public int? SellerUid { get; set; }
        public string SellerAlipay { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Price { get; set; }
        public decimal? TotalPrice { get; set; }
        public string TrendSide { get; set; }
        public string PictureUrl { get; set; }
        public int? Status { get; set; }
        public DateTime? EntryOrderTime { get; set; }
        public DateTime? PaidTime { get; set; }
        public DateTime? PayCoinTime { get; set; }
        public DateTime? DealTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? Ctime { get; set; }
        public string CoinType { get; set; }
        public DateTime? Utime { get; set; }
        public decimal? Fee { get; set; }
        public DateTime? AppealTime { get; set; }
        public DateTime? PaidEndTime { get; set; }
        public DateTime? DealEndTime { get; set; }
        public sbyte? BuyerBan { get; set; }
        public sbyte? SellerBan { get; set; }
        public int? MonthlyTradeCount { get; set; }
    }
}
