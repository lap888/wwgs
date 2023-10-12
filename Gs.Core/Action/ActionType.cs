using System.ComponentModel;

namespace Gs.Core.Action
{
    /// <summary>
    /// 父菜单类型-自定义系统模块
    /// </summary>
    public enum ActionType
    {
        [Description("系统管理")]
        SystemManager = 1,
        [Description("APP管理")]
        APPManager = 2,
        [Description("交易管理")]
        ExchangeManager = 3,
        [Description("会员管理")]
        UsersManager = 4,
        [Description("商城管理")]
        MallManager = 5,
        [Description("运营中心")]
        CommunityManager = 6,
        [Description("合伙人管理")]
        PartnerManager = 7,

        [Description("NW")]
        NW = 8,

    }
}