using Gs.Application.Response;
using Gs.Application.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Request
{
    /// <summary>
    /// 手机号归属地接口
    /// </summary>
    public class ReqMobileInfo : IFuluRequest<FuluResponse<RspMobileInfo>>
    {
        /// <summary>
        /// 手机号
        /// </summary>
        public String Phone { get; set; }

        /// <summary>
        /// 接口方法名称
        /// </summary>
        /// <returns></returns>
        public String GetApiName()
        {
            return "fulu.mobile.info.get";
        }

        /// <summary>
        /// 请求参数集合
        /// </summary>
        /// <returns></returns>
        public String GetBizContent()
        {
            Dictionary<String, String> BizContent = new Dictionary<String, String>();
            BizContent.Add("phone", this.Phone);
            return JsonConvert.SerializeObject(BizContent);
        }
    }
}
