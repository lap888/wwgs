using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Utils
{
    /// <summary>
    /// 福禄统一返回模型
    /// </summary>
    public class FuluResponse<T>
    {
        /// <summary>
        /// 返回码
        /// </summary>
        public Int32 Code { get; set; } = -1;

        public Boolean IsError
        {
            get
            {
                if (this.Code == 0)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 返回码描述
        /// </summary>
        public String Message { get; set; }

        /// <summary>
        /// 响应结果
        /// </summary>
        public String Result { get; set; }

        /// <summary>
        /// 响应模型
        /// </summary>
        public T ResultData
        {
            get
            {
                if (this.Code == 0)
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<T>(this.Result);
                    }
                    catch (Exception ex) { throw ex; }
                }
                return default;
            }
        }

        /// <summary>
        /// 签名串
        /// </summary>
        public String Sign { get; set; }
    }
}
