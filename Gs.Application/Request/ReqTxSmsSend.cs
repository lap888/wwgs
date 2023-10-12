using Gs.Application.Enums;
using Gs.Application.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Gs.Application.Request
{
    /// <summary>
    /// 发送短信
    /// </summary>
    public class ReqTxSmsSend : IQCloudRequest<Response.RspTxSmsSend>
    {
        /// <summary>
        /// 下发手机号码
        /// </summary>
        public List<String> PhoneNumberSet { get; set; }

        /// <summary>
        /// 模板 ID
        /// </summary>
        public String TemplateID { get; set; }

        /// <summary>
        /// SdkAppid
        /// </summary>
        public String SmsSdkAppid { get; set; }

        /// <summary>
        /// 请求内容
        /// </summary>
        /// <returns></returns>
        public HttpContent GetContent()
        {
            StringContent Content = new StringContent(JsonConvert.SerializeObject(this));
            return Content;
        }

        public UtilDictionary GetHeaderParam()
        {
            throw new NotImplementedException();
        }

        public UtilDictionary GetHttpParam()
        {
            throw new NotImplementedException();
        }

        public QCloudMethod GetMethod()
        {
            return QCloudMethod.post;
        }

        public string GetPath()
        {
            return String.Empty;
        }
    }
}
