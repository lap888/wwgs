using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models
{
    /// <summary>
    /// 模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListModel<T>
    {
        /// <summary>
        /// 页码
        /// </summary>
        public Int32 PageIndex { get; set; }

        /// <summary>
        /// 单页量
        /// </summary>
        public Int32 PageSize { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        public Int32 Total { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public List<T> List { get; set; }
    }
}
