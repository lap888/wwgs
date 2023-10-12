using Gs.Application.Response;
using Gs.Application.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Request
{
    /// <summary>
    /// 充值查询
    /// </summary>
    public class ReqRechargeQuery : IFuluRequest<FuluResponse<RspRechargeQuery>>
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public String CustomerOrderNo { get; set; }
        /// <summary>
        /// 接口方法名称
        /// </summary>
        /// <returns></returns>
        public String GetApiName()
        {
            return "fulu.order.info.get";
        }
        /// <summary>
        /// 请求参数集合
        /// </summary>
        /// <returns></returns>
        public String GetBizContent()
        {
            Dictionary<String, String> BizContent = new Dictionary<string, string>();
            BizContent.Add("customer_order_no", this.CustomerOrderNo);
            return JsonConvert.SerializeObject(BizContent);
        }
    }
}
