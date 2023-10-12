using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 量化宝状态
    /// </summary>
    public enum MiningStatus
    {
        /// <summary>
        /// 失效
        /// </summary>
        NO_EFFECTIVE = 0,
        /// <summary>
        /// 有效
        /// </summary>
        EFFECTIVE = 1,
        /// <summary>
        /// 过期
        /// </summary>
        EXPIRED = 2,
    }
}
