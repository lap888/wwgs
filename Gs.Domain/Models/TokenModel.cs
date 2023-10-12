using Gs.Domain.Enums;

namespace Gs.Domain.Models
{
    public class TokenModel
    {
        public int Id { get; set; } = -1;
        public string Mobile { get; set; }
        /// <summary>
        /// 请求来源
        /// </summary>
        /// <value></value>
        public SourceType Source { get; set; }
        public string Code { get; set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        /// <value></value>
        public AccountType Type { get; set; }
    }
}