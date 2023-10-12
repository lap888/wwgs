using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Models
{
    /// <summary>
    /// 股权配置
    /// </summary>
    public class EquityConfig
    {
        /// <summary>
        /// 期次
        /// </summary>
        public Int32 Period { get; set; }

        /// <summary>
        /// 认购总数
        /// </summary>
        public Int32 SubscribeTotal { get; set; }

        /// <summary>
        /// 认购单价
        /// </summary>
        public Decimal UnitPrice { get; set; }

        /// <summary>
        /// 转让手续费比率
        /// </summary>
        public Decimal TransferFee { get; set; }

        /// <summary>
        /// 最低认购份数
        /// </summary>
        public Int32 LimitShares { get; set; }

        /// <summary>
        /// 糖果限制
        /// </summary>
        public Int32 CandyLimit { get; set; }

        /// <summary>
        /// 规则
        /// </summary>
        public String Rules { get; set; }
    }
}
