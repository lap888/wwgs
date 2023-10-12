using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Enums
{
    public enum ActionType
    {
        [Description("修改支付宝")]
        CHANGE_ALIPAY = 0,
        [Description("支付宝二次认证")]
        AUTH_ALIPAY = 1,
        [Description("付款至用户")]
        TRANSFER_TO_USER = 2,
        [Description("钱包现金充值")]
        CASH_RECHARGE = 3,
        [Description("钱包提现")]
        CASH_WITH_DRAW = 4,
        [Description("商城购物")]
        SHOPPING = 5
    }
}
