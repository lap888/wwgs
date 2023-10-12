namespace Gs.Domain.Models.Dto
{
    public class NoticesDto : BaseModel
    {
        /// <summary>
        /// 类型 type=0 系统消息 type=1 我的消息 type=2 指南
        /// </summary>
        /// <value></value>
        public int Type { get; set; }
    }
}