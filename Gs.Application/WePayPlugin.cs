using Gs.Application.Response;
using Gs.Application.Utils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Gs.Application
{
    public class WePayPlugin : IWePayPlugin
    {
        private readonly HttpClient client;
        private readonly HttpClient certClient;
        private readonly Models.WepayConfig config;

        public WePayPlugin(IHttpClientFactory factory, IOptionsMonitor<Models.WepayConfig> monitor)
        {
            config = monitor.CurrentValue;
            client = factory.CreateClient(config.ClientName);
            certClient = factory.CreateClient(config.CertClient);
        }

        /// <summary>
        /// 请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<T> Execute<T>(IWePayRequest<T> request) where T : WeResponse, new()
        {
            request.AppId = config.AppId;
            request.MchId = config.MchId;
            WeXmlDoc xml = request.GetXmlDoc();
            xml.Add("nonce_str", Guid.NewGuid().ToString("N"));
            xml.Add("sign", xml.GetSign(config.ApiV3Key));
            String Body = xml.ToXmlStr();
            String ResultStr = String.Empty;
            T Result = new T();
            try
            {
                HttpResponseMessage Context;
                StringContent Content = new StringContent(Body, Encoding.UTF8);
                if (request.UseCert())
                {
                    Context = await this.certClient.PostAsync(request.GetUrl(), Content);
                }
                else { Context = await this.client.PostAsync(request.GetUrl(), Content); }
                ResultStr = await Context.Content.ReadAsStringAsync();
                Content.Dispose();
                Context.Dispose();
                using (var reader = new StringReader(ResultStr))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    Result = (T)serializer.Deserialize(reader);
                    Result.Content = ResultStr;
                    return Result;
                }
            }
            catch (Exception ex)
            {
                Result.Content = ResultStr;
                Result.ErrCode = ex.Source;
                Result.ErrCodeDesc = ex.Message;
                return Result;
            }
        }

        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="PrepayId"></param>
        /// <returns></returns>
        public SortedDictionary<String, String> MakeSign(String PrepayId)
        {
            SortedDictionary<String, String> pairs = new SortedDictionary<String, String>();
            pairs.Add("appid", config.AppId);
            pairs.Add("partnerid", config.MchId);
            pairs.Add("prepayid", PrepayId);
            pairs.Add("package", "Sign=WXPay");
            pairs.Add("noncestr", Guid.NewGuid().ToString("N"));
            pairs.Add("timestamp", UnixTime());

            StringBuilder signStr = new StringBuilder();
            pairs.Aggregate(signStr, (s, i) => s.Append($"{i.Key}={i.Value.ToString()}&"));
            signStr.Append("key=");
            signStr.Append(config.ApiV3Key);
            pairs.Add("sign", Security.MD5(signStr.ToString()));
            return pairs;
        }

        /// <summary>
        /// 解析异常通知
        /// </summary>
        /// <param name="notify"></param>
        /// <returns></returns>
        public RspWepayNotify Notify(String notify)
        {
            RspWepayNotify result = new RspWepayNotify();
            using (var reader = new StringReader(notify))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RspWepayNotify));
                result = (RspWepayNotify)serializer.Deserialize(reader);
            }
            return result;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        private String UnixTime()
        {
            DateTime Dt = DateTime.Now;
            DateTime dateStart = new DateTime(1970, 1, 1, 8, 0, 0);
            Int32 timeStamp = Convert.ToInt32((Dt - dateStart).TotalSeconds);
            return timeStamp.ToString();
        }
    }
}
