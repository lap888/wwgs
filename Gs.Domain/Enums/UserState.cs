using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 会员状态
    /// </summary>
    public enum UserState
    {
        /// <summary>
        /// 全部
        /// </summary>
        All = -1,

        /// <summary>
        /// 正常
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 已激活
        /// </summary>
        Activation = 1,

        /// <summary>
        /// 冻结
        /// </summary>
        Freeze = 2
    }
}
