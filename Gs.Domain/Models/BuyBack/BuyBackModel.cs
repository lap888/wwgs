using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.BuyBack
{
    /// <summary>
    /// 评估模型
    /// </summary>
    public class BuyBackModel
    {
        /// <summary>
        /// 会员编号
        /// </summary>
        public Int64 UserId { get; set; }

        /// <summary>
        /// 运营编号
        /// </summary>
        public Int64 StoreId { get; set; }

        /// <summary>
        /// 物品类型
        /// </summary>
        public RepoType Type { get; set; }

        /// <summary>
        /// 物品等级
        /// </summary>
        public ItemGrade Grade { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public Int32 Count { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public String Brand { get; set; }

        /// <summary>
        /// 成色
        /// </summary>
        public Condition Condition { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public Decimal UnitPrice { get; set; }

        /// <summary>
        /// 评估价
        /// </summary>
        public Decimal AssessPrice { get; set; }

        /// <summary>
        /// 送达方式
        /// </summary>
        public ShippingMethod Shipping { get; set; }
    }
}
