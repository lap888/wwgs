using Gs.Application.Response;
using Gs.Application.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Request
{
    /// <summary>
    /// 话费下单
    /// </summary>
    public class ReqMobileRecharge : IFuluRequest<FuluResponse<RspMobileRecharge>>
    {
        /// <summary>
        /// 充值手机号
        /// </summary>
        public String ChargePhone { get; set; }

        /// <summary>
        /// 充值数额
        /// </summary>
        public Decimal ChargeValue { get; set; }

        /// <summary>
        /// 外部订单号（应用订单号）
        /// </summary>
        public String CustomerOrderNo { get; set; }

        /// <summary>
        /// 接口方法名称
        /// </summary>
        /// <returns></returns>
        public String GetApiName()
        {
            return "fulu.order.mobile.add";
        }

        /// <summary>
        /// 请求参数集合
        /// </summary>
        /// <returns></returns>
        public String GetBizContent()
        {
            Dictionary<String, String> BizContent = new Dictionary<string, string>();
            BizContent.Add("charge_phone", ChargePhone);
            BizContent.Add("charge_value", ChargeValue.ToString("0.##"));
            BizContent.Add("customer_order_no", CustomerOrderNo);
            return JsonConvert.SerializeObject(BizContent);
        }
    }
}
