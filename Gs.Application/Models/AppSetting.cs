using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Models
{
    /// <summary>
    /// 通用配置
    /// </summary>
    public class AppSetting
    {
        /// <summary>
        /// 支付宝异步通知地址
        /// </summary>
        public String AlipayNotify { get; set; }

        /// <summary>
        /// 微信异步通知地址
        /// </summary>
        public String WePayNotify { get; set; }

        /// <summary>
        /// 微信异步通知地址
        /// </summary>
        public String QCloudUrl { get; set; }

        /// <summary>
        /// 提现费率
        /// </summary>
        public Decimal WithdrawRate { get; set; }

        /// <summary>
        /// 用户等级配置
        /// </summary>
        public List<UserLevel> Levels { get; set; }

        /// <summary>
        /// 团队等级
        /// </summary>
        public List<TeamLevel> TeamLevels { get; set; }

        /// <summary>
        /// 交易开始时间
        /// </summary>
        public DateTime TradeStartTime { get; set; }

        /// <summary>
        /// 交易结束时间
        /// </summary>
        public DateTime TradeEndTime { get; set; }

        /// <summary>
        /// 卖单最大单价
        /// </summary>
        public Decimal SellMaxPrice { get; set; }

        /// <summary>
        /// 卖单最大单价
        /// </summary>
        public Decimal SellMinPrice { get; set; }
    }
}
