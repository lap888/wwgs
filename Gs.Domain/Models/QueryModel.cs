using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models
{
    /// <summary>
    /// 通用查询模型
    /// </summary>
    public class QueryModel
    {
        /// <summary>
        /// 通用编号
        /// </summary>
        public Int64 Id { get; set; }

        /// <summary>
        /// 会员编号
        /// </summary>
        public Int64 UserId { get; set; }

        /// <summary>
        /// 关键字
        /// </summary>
        public String Keyword { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public String Mobile { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        private int pageIndex;
        public int PageIndex
        {
            get { return pageIndex < 1 ? 1 : pageIndex; }
            set { pageIndex = value; }
        }

        /// <summary>
        /// 页量
        /// </summary>
        private int pageSize;
        public int PageSize
        {
            get { return pageSize < 1 ? 10 : pageSize; }
            set { pageSize = value; }
        }
    }
}
