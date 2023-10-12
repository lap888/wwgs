using Gs.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Admin
{
    /// <summary>
    /// 查询量化宝
    /// </summary>
    public class QueryMining : QueryModel
    {
        /// <summary>
        /// 量化宝编号
        /// </summary>
        public Int32 BaseId { get; set; }

        /// <summary>
        /// 量化宝状态
        /// </summary>
        public MiningStatus? State { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public MiningSource? Source { get; set; }
    }
}
