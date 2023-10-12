using Gs.Domain.Enums;
using System;

namespace Gs.Domain.Models
{
    /// <summary>
    /// 查询模型
    /// </summary>
    public class QueryTradeOrder
    {
        /// <summary>
        /// 通用编号
        /// </summary>
        public Int64 CurrentId { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public String Mobile { get; set; }

        /// <summary>
        /// 支付宝
        /// </summary>
        public String Alipay { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public String Type { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public TradeState Status { get; set; } = TradeState.All;

        /// <summary>
        /// 页码
        /// </summary>
        public Int32 PageIndex { get; set; } = 1;

        /// <summary>
        /// 页量
        /// </summary>
        public Int32 PageSize { get; set; } = 10;

        public string CoinType { get; set; }
    }
}
