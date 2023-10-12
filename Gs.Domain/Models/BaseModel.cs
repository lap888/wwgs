namespace Gs.Domain.Models
{
    public class BaseModel
    {
        /// <summary>
        /// 页索引
        /// </summary>
        /// <value></value>
        public int PageIndex { get; set; } = 0;
        /// <summary>
        /// 页大小
        /// </summary>
        /// <value></value>
        public int PageSize { get; set; } = 10;
    }
}