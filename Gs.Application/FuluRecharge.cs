using Gs.Application.Models;
using Gs.Application.Utils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gs.Application
{
    public class FuluRecharge : IFuluRecharge
    {
        private readonly FuluConfig config;
        private readonly HttpClient client;
        public FuluRecharge(IOptionsMonitor<FuluConfig> monitor, IHttpClientFactory factory)
        {
            config = monitor.CurrentValue;
            client = factory.CreateClient(config.ClientName);
        }

        /// <summary>
        /// 福禄充值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<FuluResponse<T>> Execute<T>(IFuluRequest<FuluResponse<T>> request)
        {
            String ResultStr = String.Empty;
            FuluResponse<T> Rult = new FuluResponse<T>();
            Dictionary<String, String> RequestParams = new Dictionary<String, String>();
            RequestParams.Add("app_key", config.AppKey);
            RequestParams.Add("method", request.GetApiName());
            RequestParams.Add("timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            RequestParams.Add("version", "2.0");
            RequestParams.Add("format", "json");
            RequestParams.Add("charset", "utf-8");
            RequestParams.Add("sign_type", "md5");
            RequestParams.Add("app_auth_token", "");
            RequestParams.Add("biz_content", request.GetBizContent());
            #region 计算签名 并 添加签名
            String ReqStr = JsonConvert.SerializeObject(RequestParams);
            Char[] ReqChars = ReqStr.ToCharArray();
            Array.Sort(ReqChars);
            String ReqSignStr = new String(ReqChars) + config.AppSecret;
            RequestParams.Add("sign", Security.MD5(ReqSignStr).ToLower());
            #endregion
            try
            {
                #region 请求 接口
                String Content = JsonConvert.SerializeObject(RequestParams);
                StringContent HttpContent = new StringContent(Content, Encoding.UTF8, "application/json");
                HttpResponseMessage HttpResponse = await this.client.PostAsync(this.config.ApiUrl, HttpContent);
                ResultStr = await HttpResponse.Content.ReadAsStringAsync();
                Rult = JsonConvert.DeserializeObject<FuluResponse<T>>(ResultStr);
                HttpContent.Dispose();
                HttpResponse.Dispose();
                #endregion
                if (Rult.IsError) { return Rult; }
                #region 验证签名
                Char[] RspChars = Rult.Result.ToCharArray();
                Array.Sort(RspChars);
                String RspSignStr = new String(RspChars) + this.config.AppSecret;
                String CheckSign = Security.MD5(RspSignStr).ToLower();
                if (Rult.Sign.Equals(CheckSign))
                {
                    return Rult;
                }
                #endregion
                throw new Exception("签名错误");
            }
            catch (Exception ex)
            {
                Rult.Result = ResultStr;
                Rult.Code = -1;
                Rult.Message = ex.Message;
                return Rult;
            }
        }
    }
}
