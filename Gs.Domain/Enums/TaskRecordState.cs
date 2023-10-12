using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 量化宝记录状态
    /// </summary>
    public enum TaskRecordState
    {
        /// <summary>
        /// 全部
        /// </summary>
        All = 0,

        /// <summary>
        /// 已报名
        /// </summary>
        Registered = 1,

        /// <summary>
        /// 已提交
        /// </summary>
        Submitted = 2,

        /// <summary>
        /// 已申诉
        /// </summary>
        Appealed = 3,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed = 4,

        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled = 5,

        /// <summary>
        /// 已拒绝
        /// </summary>
        Rejected = 6,

        /// <summary>
        /// 已超时
        /// </summary>
        TimeOut = 7,
    }
}
