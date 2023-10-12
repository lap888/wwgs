using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Store
{
    /// <summary>
    /// 兑换模型
    /// </summary>
    public class ExchangeModel
    {
        /// <summary>
        /// 会员编号
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 兑换积分数
        /// </summary>
        public Decimal Num { get; set; }
    }
}
