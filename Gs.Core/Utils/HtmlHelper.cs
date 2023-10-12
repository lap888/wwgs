using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Gs.Core.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public class HtmlHelper
    {
        /// <summary>
        /// HTML中提取图片地址
        /// </summary>
        public static List<string> PickupImgUrl(string html)
        {
            Regex regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);
            MatchCollection matches = regImg.Matches(html);
            List<string> lstImg = new List<string>();
            foreach (Match match in matches)
            {
                lstImg.Add(match.Groups["imgUrl"].Value);
            }
            return lstImg;
        }

        /// <summary>
        /// HTML中提取图片地址
        /// </summary>
        public static string PickupImgUrlFirst(string html)
        {
            List<string> lstImg = PickupImgUrl(html);
            return lstImg.Count == 0 ? string.Empty : lstImg[0];
        }
    }
}
