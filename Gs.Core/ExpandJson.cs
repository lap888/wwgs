using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Core
{
    /// <summary>
    /// JSON拓展方法
    /// </summary>
    public static class ExpandJson
    {
        #region Json转换方法
        /// <summary>
        /// 转换对象为Json
        /// </summary>
        /// <param name="Obj">需要转换的对象</param>
        /// <param name="WithFormat">是否包含字符格式</param>
        /// <param name="WithNull">是否包含Null值</param>
        /// <param name="UseCamelCase">驼峰序列化[默认:false]</param>
        /// <returns></returns>
        public static string ToJson(this object Obj, bool WithFormat = true, bool WithNull = true, bool UseCamelCase = false)
        {
            try
            {
                if (null == Obj) { return String.Empty; }
                Newtonsoft.Json.Formatting JFormat = Newtonsoft.Json.Formatting.None;
                Newtonsoft.Json.NullValueHandling JNull = Newtonsoft.Json.NullValueHandling.Ignore;
                Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver CamelCase = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                if (WithFormat) { JFormat = Newtonsoft.Json.Formatting.Indented; }
                if (WithNull) { JNull = Newtonsoft.Json.NullValueHandling.Include; }
                Newtonsoft.Json.JsonSerializerSettings JSet = new Newtonsoft.Json.JsonSerializerSettings { NullValueHandling = JNull, DateFormatString = "yyyy-MM-dd HH:mm:ss" };
                if (UseCamelCase) { JSet.ContractResolver = CamelCase; }
                return Newtonsoft.Json.JsonConvert.SerializeObject(Obj, JFormat, JSet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        /// <summary>
        /// Json字符串转换为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="JsonStr">Json字符串</param>
        /// <returns></returns>
        public static T JsonTo<T>(this string JsonStr) where T : class, new()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(JsonStr)) { throw new ArgumentException("The parameter is empty"); }
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(JsonStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
