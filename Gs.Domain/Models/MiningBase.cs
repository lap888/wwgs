using System;

namespace Gs.Domain.Models
{
    /// <summary>
    /// 量化宝模型
    /// </summary>
    public class MiningBase
    {
        /// <summary>
        /// 量化宝编号
        /// </summary>
        public Int32 BaseId { get; set; }
        /// <summary>
        /// 量化宝名称
        /// </summary>
        public String BaseName { get; set; }
        /// <summary>
        /// 是否允许兑换
        /// </summary>
        public Boolean IsExchange { get; set; }
        /// <summary>
        /// 是否允许续期
        /// </summary>
        public Boolean IsRenew { get; set; }
        /// <summary>
        /// 是否显示
        /// </summary>
        public Boolean StoreShow { get; set; }
        /// <summary>
        /// 量化宝单价
        /// </summary>
        public Int32 UnitPrice { get; set; }
        /// <summary>
        /// 日产出
        /// </summary>
        public Decimal DayOut { get; set; }
        /// <summary>
        /// 总产出
        /// </summary>
        public Decimal TotalOut { get; set; }
        /// <summary>
        /// 量化宝时长
        /// </summary>
        public Int32 TaskDuration { get; set; }
        /// <summary>
        /// 有效期
        /// </summary>
        public Int32 TaskExpires { get; set; }
        /// <summary>
        /// 活跃度
        /// </summary>
        public Decimal Active { get; set; }
        /// <summary>
        /// 活跃度时长
        /// </summary>
        public Decimal ActiveDuration { get; set; }
        /// <summary>
        /// 荣耀值
        /// </summary>
        public Decimal HonorValue { get; set; }
        /// <summary>
        /// 量化宝上限
        /// </summary>
        public Int32 MaxHave { get; set; }
        /// <summary>
        /// 量化宝颜色
        /// </summary>
        public string Colors { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; } = "";

    }
}
