namespace Gs.Domain.Models.Dto
{
    public class AuthDto : BaseModel
    {
        /// <summary>
        /// 0 未提交 审核状态 1 提交人工 2 成功 3拒绝 默认待审核
        /// </summary>
        /// <value></value>
        public int AuthType { get; set; } = 1;

        public string Reson { get; set; }

        public int Status { get; set; }

        public int Id { get; set; }

        public int UserId { get; set; }

        public string IdNum { get; set; }

        public string Alipay { get; set; }

        public string TrueName { get; set; }
    }
}