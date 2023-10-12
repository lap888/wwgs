using System;

namespace Gs.Domain.Models.Dto
{
    public class CoinTradeExt
    {
        /// <summary>
        /// 涨幅
        /// </summary>
        /// <value></value>
        public decimal UpRate { get; set; } = 0.01M;

        /// <summary>
        /// 历史成交总金额
        /// </summary>
        public Decimal HistoryFinishCount { get; set; }
        /// <summary>
        /// 历史成交总笔数
        /// </summary>
        public int HistoryFinishOrderCount { get; set; }

        /// <summary>
        /// 今日需求总额
        /// </summary>
        public Decimal TodayBuyCount { get; set; }

        /// <summary>
        /// 今日需求总量笔数
        /// </summary>
        public int TodayBuyOrderCount { get; set; }

        /// <summary>
        /// 可用MBM
        /// </summary>
        public Decimal CanUserCottonCoin { get; set; }
        /// <summary>
        /// 可用MBM
        /// </summary>
        public Decimal CanUserCotton { get; set; }

        /// <summary>
        /// 参考价最高
        /// </summary>
        public decimal SysMaxPrice { get; set; } = 2;
        /// <summary>
        /// 参考价最低
        /// </summary>
        public decimal SysMinPrice { get; set; } = 1;



    }
}