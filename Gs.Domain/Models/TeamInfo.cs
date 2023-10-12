using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models
{
    /// <summary>
    /// 团队信息
    /// </summary>
    public class TeamInfo
    {
        public long Id { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        /// <value></value>
        public string Mobile { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        /// <value></value>
        public string AvatarUrl { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        /// <value></value>
        public string Name { get; set; }
        /// <summary>
        /// 贡献果核数
        /// </summary>
        public decimal Contributions { get; set; }
        /// <summary>
        /// 注册时间
        /// </summary>
        /// <value></value>
        public DateTime ctime { get; set; }
        /// <summary>
        /// 我的邀请人数
        /// </summary>
        /// <value></value>
        public int AuthCount { get; set; } = 0;
        /// <summary>
        /// 升级需要的邀请人数
        /// </summary>
        public int NeedAuthCount { get; set; } = 0;

        /// <summary>
        /// 英雄活跃度
        /// </summary>
        /// <value></value>
        public int BigCandyH { get; set; } = 0;
        /// <summary>
        /// 联盟活跃
        /// </summary>
        /// <value></value>
        public int LittleCandyH { get; set; } = 0;
        /// <summary>
        /// 升级所需的联盟活跃
        /// </summary>
        public int NeedLittleCandyH { get; set; } = 0;
        /// <summary>
        /// 团队活跃
        /// </summary>
        /// <value></value>
        public int TeamCandyH { get; set; } = 0;

        /// <summary>
        /// 升级所需团队活跃
        /// </summary>
        public int NeedTeamCandyH { get; set; } = 0;

        /// <summary>
        /// 团队人数
        /// </summary>
        /// <value></value>
        public int TeamCount { get; set; } = 0;
        /// <summary>
        /// 团队等级
        /// </summary>
        /// <value></value>
        public int TeamStart { get; set; } = 0;
        /// <summary>
        /// 活跃度
        /// </summary>
        public int Active { get; set; }
        /// <summary>
        /// 认证状态
        /// </summary>
        public int AuditState { get; set; } = 0;
        /// <summary>
        /// 最后活跃时间
        /// </summary>
        public DateTime ActiveTime { get; set; }

    }
}
