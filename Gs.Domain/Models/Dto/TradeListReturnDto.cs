using System;

namespace Gs.Domain.Models.Dto
{
    public class TradeListReturnDto
    {
        public int Id { get; set; }
        public string SellerTrueName { get; set; }
        public string BuyerTrueName { get; set; }
        public string TradeNumber { get; set; }
        public int BuyerUid { get; set; }
        public string BuyerMobile { get; set; }
        public string BuyerAvatarUrl { get; set; }
        public string BuyerAlipay { get; set; }
        public int SellerUid { get; set; }
        public string SellerMobile { get; set; }
        public string SellerAvatarUrl { get; set; }
        public string SellerAlipay { get; set; }
        public string PictureUrl { get; set; }
        public int Amount { get; set; }
        public double Price { get; set; }
        public double TotalPrice { get; set; }
        public DateTime DealTime { get; set; }
        public DateTime DealEndTime { get; set; }
        public DateTime PaidTime { get; set; }
        public DateTime PaidEndTime { get; set; }
        public int Status { get; set; }

    }
}