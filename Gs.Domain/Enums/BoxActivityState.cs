using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 宝箱活动状态
    /// </summary>
    public enum BoxActivityState
    {
        /// <summary>
        /// 未启动
        /// </summary>
        [Description("未启动")]
        NotInitiated = 0,

        /// <summary>
        /// 正常
        /// </summary>
        [Description("正常")]
        Normal = 1,

        /// <summary>
        /// 完成
        /// </summary>
        [Description("完成")]
        Completed = 2
    }
}
