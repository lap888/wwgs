using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Enums
{
    /// <summary>
    /// 会员认证状态
    /// </summary>
    public enum UserAuthState
    {
        /// <summary>
        /// 全部
        /// </summary>
        All = -1,

        /// <summary>
        /// 未认证
        /// </summary>
        NotAuth = 0,

        /// <summary>
        /// 待审核
        /// </summary>
        WaitReview = 1,

        /// <summary>
        /// 已审核
        /// </summary>
        Audited = 2,
    }
}
