using Gs.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Store
{
    /// <summary>
    /// 下单
    /// </summary>
    public class SubmitOrder
    {
        /// <summary>
        /// 商品编号
        /// </summary>
        public Int64 ItemId { get; set; }

        /// <summary>
        /// 会员编号
        /// </summary>
        public Int64 UserId { get; set; }

        /// <summary>
        /// 社区编号
        /// </summary>
        public Int64 StoreId { get; set; }

        /// <summary>
        /// 地址编号
        /// </summary>
        public Int64 AddressId { get; set; }

    }
}
