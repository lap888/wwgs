using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models
{
    /// <summary>
    /// 消息订阅团队信息模型
    /// </summary>
    public class SubscribeTeam
    {
        /// <summary>
        /// 记录编号
        /// </summary>
        public string RecordId { get; set; }

        /// <summary>
        /// 会员ID
        /// </summary>
        public long MemberId { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string Nick { get; set; }

        /// <summary>
        /// 推荐人ID【使用注册信道，此项必填】
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 量化宝编号
        /// </summary>
        public int BaseId { get; set; }
        /// <summary>
        /// 给予活跃贡献值
        /// </summary>
        public decimal Active { get; set; }
        /// <summary>
        /// 是否续期量化宝
        /// </summary>
        public bool RenewTask { get; set; } = false;
    }
}
