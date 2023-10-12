using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gs.Application
{
    /// <summary>
    /// 微信公众号
    /// </summary>
    public interface IWechatPlugin
    {
        /// <summary>
        /// 获取Code Url
        /// </summary>
        /// <param name="BackUrl"></param>
        /// <param name="State"></param>
        /// <returns></returns>
        String GetCodeUrl(String BackUrl, String State = "");
    }
}
