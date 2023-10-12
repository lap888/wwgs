using System;

namespace Gs.Domain.Models.Dto
{
    public class TradeReqDto : BaseModel
    {
        /// <summary>
        /// ÂòÂô
        /// </summary>
        public String Sale { get; set; }
        /// <summary>
        /// type 0 amount type price
        /// </summary>
        /// <value></value>
        public string Type { get; set; } = "amount";
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Order { get; set; } = "desc";
        public string SearchText { get; set; } = "";
        /// <summary>
        /// 0 È«²¿ 1 1-10 2 11-50 3 51-100
        /// </summary>
        public int Range { get; set; }
    }
}