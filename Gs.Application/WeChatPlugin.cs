using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Gs.Application
{
    /// <summary>
    /// 微信公众号
    /// </summary>
    public class WeChatPlugin : IWechatPlugin
    {
        private const String CodeUrl = "https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_base&state={2}#wechat_redirect";
        private readonly Models.WeChatConfig config;
        private HttpClient client;
        private readonly CSRedis.CSRedisClient RedisCache;
        public WeChatPlugin(IHttpClientFactory factory, IOptionsMonitor<Models.WeChatConfig> monitor,CSRedis.CSRedisClient cSRedis)
        {
            config = monitor.CurrentValue;
            client = factory.CreateClient(config.ClientName);
            RedisCache = cSRedis;
        }

        /// <summary>
        /// 获取Code Url
        /// </summary>
        /// <param name="BackUrl"></param>
        /// <param name="State"></param>
        /// <returns></returns>
        public String GetCodeUrl(String BackUrl, String State = "")
        {
            return String.Format(CodeUrl, config.AppId, BackUrl, State);
        }
    }
}
