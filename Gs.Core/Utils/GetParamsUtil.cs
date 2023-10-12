using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json.Serialization;

namespace Gs.Core.Utils
{
    public static class GetParamsUtil
    {
        static SnakeCaseNamingStrategy snakeCaseNaming = new SnakeCaseNamingStrategy();
        static CamelCaseNamingStrategy camelCaseNaming = new CamelCaseNamingStrategy();
        /// <summary>
        /// 将 SnakeCaseNaming 转换为 snake_case_naming 格式
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string SnakeCaseNaming(this string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            return snakeCaseNaming.GetPropertyName(name, false);
        }
        /// <summary>
        /// 将CaseNaming 转换为 caseNaming 格式
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string CamelCaseNaming(this string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            return camelCaseNaming.GetPropertyName(name, false);
        }

        /// <summary>
        /// 将字典组织为 query string
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static string ToQueryString<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            if (dict == null || dict.Count == 0)
            {
                return string.Empty;
            }
            return string.Join("&", dict
                .Select(t => $"{t.Key}={(t.Value == null ? "" : System.Net.WebUtility.UrlEncode(t.Value.ToString()))}"));
        }
        /// <summary>
        /// 将dict转换成string 并且不加encode（编码）
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static string ToQueryStringWithNoEncode<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            if (dict == null || dict.Count == 0)
            {
                return string.Empty;
            }
            return string.Join("&", dict
                .Select(t => $"{t.Key}={(t.Value == null ? "" : t.Value.ToString())}"));
        }
        public static StringBuilder QueryString(this IDictionary<string, object> datas, bool checkIsNull = true)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kv in datas)
            {
                if (checkIsNull && kv.Value == null)
                {
                    throw new Exception($"ToQueryString 内部含有值为空的字段!ToQueryString:{kv.Key}");
                }
                if (kv.Key.Equals("sign", StringComparison.OrdinalIgnoreCase) || kv.Key.Equals("key", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                sb.Append($"&{kv.Key}={kv.Value}");
            }
            sb.Remove(0, 1);
            return sb;
        }
        public static bool IsEmpty(this IDictionary<string, object> datas, string key)
        {
            var has = datas.ContainsKey(key);
            if (has)
            {
                var value = datas[key];
                if (value == null)
                {
                    has = false;
                }
                else if (string.IsNullOrWhiteSpace(value.ToString()))
                {
                    has = false;
                }
            }
            return has;
        }
        /// <summary>
        /// get value from RouteData,QueryString,Form,Cookie
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Params(this HttpContext context, string key)
        {
            string value = string.Empty;
            var help = context.Items.SingleOrDefault(t => t.Value is UrlHelper).Value as UrlHelper;
            if (help != null)
            {
                var route = help.ActionContext.RouteData.Values[key];
                if (route != null)
                {
                    return route.ToString();
                }
            }

            if (context.Request.Query.ContainsKey(key))
            {
                value = context.Request.Query[key];
            }
            else if (context.Request.HasFormContentType && context.Request.Form.ContainsKey(key))
            {
                value = context.Request.Form[key];
            }
            else if (context.Request.Cookies.Count > 0 && context.Request.Cookies.ContainsKey(key))
            {
                value = context.Request.Cookies[key];
            }

            return value;
        }

        public static string Params(this RazorPage page, string key)
        {
            return page.Context.Params(key);
        }
        public static string Params(this ControllerBase controller, string key)
        {
            return controller.HttpContext.Params(key);
        }
    }
}