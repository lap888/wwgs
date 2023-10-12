using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 账户变更类型
    /// </summary>
    public enum AccountModifyType
    {
        /// <summary>
        /// 全部
        /// </summary>
        [Description("")]
        ALL = 0,
        /// <summary>
        /// 现金充值
        /// </summary>
        [Description("钱包充值 单号:{0}")]
        CASH_RECHARGE = 1,
        /// <summary>
        /// 钱包提现
        /// </summary>
        [Description("钱包提现 单号:{0}")]
        CASH_WITH_DRAW = 2,
        /// <summary>
        /// 扣除赏金
        /// </summary>
        [Description("扣除赏金 量化宝编号:{0}")]
        DEDUCTION_BOUNTY = 3,
        /// <summary>
        /// 量化宝赏金
        /// </summary>
        [Description("量化宝赏金 量化宝编号:{0}")]
        MIAAION_BOUNTY = 4,
        /// <summary>
        /// 现金分红
        /// </summary>
        [Description("现金分红 NW:{0}颗,每MBM{1}元")]
        CASH_DEVIDEND = 5,
        /// <summary>
        /// 系统收单
        /// </summary>
        [Description("MBM交易 单号:{0}")]
        SYSTEM_ACQUIRE = 6,
        /// <summary>
        /// 现金分红体验券
        /// </summary>
        [Description("现金分红体验券 NW:{0}颗,每MBM{1}元")]
        CASH_DEVIDEND_COUPON = 7,
        /// <summary>
        /// 发布人上级分红
        /// </summary>
        [Description("下级发布的悬赏[{0}]完成奖励")]
        YOBANG_DEVIDEND_PUBLISHER = 8,
        /// <summary>
        /// 量化宝人上级分红
        /// </summary>
        [Description("下级完成悬赏量化宝[{0}]奖励")]
        YOBANG_DEVIDEND_TASKER = 9,
        /// <summary>
        /// 下级会员认证奖励
        /// </summary>
        [Description("下级会员[{0}]认证奖励")]
        SUBUSER_CERT_AWARD = 10,
        /// <summary>
        /// 城内会员认证奖励
        /// </summary>
        [Description("城内会员[{0}]认证奖励")]
        CITY_CERT_AWARD = 11,
        /// <summary>
        /// 城主哟帮分红
        /// </summary>
        [Description("[{0}]城主哟帮分红奖励")]
        CITY_YOBANG_DEVIDEND = 15,
        /// <summary>
        /// 城主广告分红
        /// </summary>
        [Description("[{0}]城主广告分红奖励")]
        CITY_VIDEO_AWARD = 16,
        /// <summary>
        /// 城主游戏分红
        /// </summary>
        [Description("[{0}]城主游戏分红奖励")]
        CITY_SHANDW_AWARD = 17,
        /// <summary>
        /// 城主商城分红
        /// </summary>
        [Description("[{0}]城主商城分红奖励")]
        CITY_MALL_DEVIDEND = 18,
        /// <summary>
        /// 提现失败
        /// </summary>
        [Description("提现失败：{0}")]
        Failed_Refund = 22,
        /// <summary>
        /// 股权季度分红
        /// </summary>
        [Description("股权[{0}]季度分红,每股{0}")]
        EQUITY_DEVIDEND = 23,
        /// <summary>
        /// 买单排行榜分红
        /// </summary>
        [Description("买单排行榜{0}奖励：{0}")]
        BUY_RANK_AWARD = 24,
    }
}
