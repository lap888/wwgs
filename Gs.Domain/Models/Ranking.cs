using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models
{
    /// <summary>
    /// 排行榜
    /// </summary>
    public class Ranking
    {
        /// <summary>
        /// 排名
        /// </summary>
        public Int32 Rank { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public String Mobile { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public String Nick { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public String HeadImg { get; set; }

        /// <summary>
        /// 邀请总人数
        /// </summary>
        public Int32 InviteTotal { get; set; }
        /// <summary>
        /// 分享次数
        /// </summary>
        public Int32 ShareCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int32 InviteDay { get; set; }
        /// <summary>
        /// 复投数据
        /// </summary>
        public Decimal Duplicate { get; set; }
    }
}
