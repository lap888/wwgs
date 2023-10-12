using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Dto
{
    /// <summary>
    /// 联系方式模型
    /// </summary>
    public class ContactModel
    {
        /// <summary>
        /// 会员编号
        /// </summary>
        public Int64 UserId { get; set; }

        /// <summary>
        /// 城市编码
        /// </summary>
        public String CityNo { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public String Mobile { get; set; } = String.Empty;

        /// <summary>
        /// 微信
        /// </summary>
        public String WeChat { get; set; } = String.Empty;
    }
}
