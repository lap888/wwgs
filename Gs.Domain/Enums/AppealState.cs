using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 申诉状态
    /// </summary>
    public enum AppealState
    {
        /// <summary>
        /// 未处理
        /// </summary>
        Unprocessed = 0,

        /// <summary>
        /// 已经过
        /// </summary>
        Passed = 1,

        /// <summary>
        /// 已拒绝
        /// </summary>
        Rejected = 2
    }
}
