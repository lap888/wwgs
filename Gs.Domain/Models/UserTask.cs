using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models
{
    /// <summary>
    /// 量化宝列表
    /// </summary>
    public class UserTask
    {
        /// <summary>
        /// 编号
        /// </summary>
        public Int64 Id { get; set; }

        /// <summary>
        /// 会员编号
        /// </summary>
        public Int64 UserId { get; set; }

        /// <summary>
        /// 会员昵称
        /// </summary>
        public String UserNick { get; set; }

        /// <summary>
        /// 会员手机号
        /// </summary>
        public String UserMobile { get; set; }

        /// <summary>
        /// 量化宝编号
        /// </summary>
        public Int64 TaskId { get; set; }

        /// <summary>
        /// 量化宝标题
        /// </summary>
        public String TaskTitle { get; set; }

        /// <summary>
        /// 日产量
        /// </summary>
        public Decimal DailyOutput { get; set; }

        /// <summary>
        /// 总产量
        /// </summary>
        public Decimal TotalOutput { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime BeginTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public Int32 Status { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public Int32 Source { get; set; }
    }
}
