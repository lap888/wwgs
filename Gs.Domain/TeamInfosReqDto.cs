using Gs.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Models
{
    public class TeamInfosReqDto: QueryModel
    {
        /// <summary>
        /// type 0 按团队果核排序 type 1 团队人数 type 2 时间
        /// </summary>
        /// <value></value>
        public int Type { get; set; }

        /// <summary>
        /// order asc 正序 order desc 倒叙
        /// </summary>
        /// <value></value>
        public string Order { get; set; }
    }
}
