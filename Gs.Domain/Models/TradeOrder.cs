using Gs.Domain.Enums;
using System;

namespace Gs.Domain.Models
{
    /// <summary>
    /// 交易订单
    /// </summary>
    public class TradeOrder
    {
        /// <summary>
        /// 编号
        /// </summary>
        public Int64 Id { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public String OrderId { get; set; }

        /// <summary>
        /// 买方编号
        /// </summary>
        public Int64 BuyerUid { get; set; }

        /// <summary>
        /// 买方支付宝号
        /// </summary>
        public String BuyerAlipay { get; set; }

        /// <summary>
        /// 卖方编号
        /// </summary>
        public Int64 SellerUid { get; set; }

        /// <summary>
        /// 卖方真实姓名
        /// </summary>
        public String TrueName { get; set; }

        /// <summary>
        /// 卖方支付宝
        /// </summary>
        public String SellerAlipay { get; set; }

        /// <summary>
        /// 交易数量
        /// </summary>
        public Decimal SellCount { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public Decimal UnitPrice { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public Decimal TotalPrice { get; set; }

        /// <summary>
        /// 交易手续费
        /// </summary>
        public Decimal TradeFee { get; set; }

        /// <summary>
        /// 成单时间
        /// </summary>
        public DateTime? ConfirmTime { get; set; }

        /// <summary>
        /// 支付图
        /// </summary>
        public String PayPic { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PayTime { get; set; }

        /// <summary>
        /// 交易状态
        /// </summary>
        public TradeState TradeState { get; set; }

        /// <summary>
        /// 超时用户
        /// </summary>
        public String TimeOutUser { get; set; }

        /// <summary>
        /// 申诉图
        /// </summary>
        public String AppealPic { get; set; }

        /// <summary>
        /// 申诉原因
        /// </summary>
        public String AppealReason { get; set; }

        /// <summary>
        /// 申诉时间
        /// </summary>
        public DateTime? AppealTime { get; set; }
    }
}
