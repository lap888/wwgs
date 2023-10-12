using System;

namespace Gs.Domain.Models.Dto
{
    /// <summary>
    /// 会员模型
    /// </summary>
    public class AdminUserModel : UserDto
    {
        /// <summary>
        /// 余额
        /// </summary>
        public Decimal Balance { get; set; }

        /// <summary>
        /// 冻结
        /// </summary>
        public Decimal Frozen { get; set; }

    }
}
