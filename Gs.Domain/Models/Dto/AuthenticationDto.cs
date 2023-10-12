namespace Gs.Domain.Models.Dto
{
    public class AuthenticationDto
    {
        /// <summary>
        /// 扫脸认证编号
        /// </summary>
        public string CertifyId { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        /// <value></value>
        public string TrueName { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        /// <value></value>
        public string IdNum { get; set; }
        /// <summary>
        /// 0 扫脸 1人工
        /// </summary>
        /// <value></value>
        public int AuthType { get; set; }
        /// <summary>
        /// 支付宝
        /// </summary>
        /// <value></value>
        public string Alipay { get; set; }
        /// <summary>
        /// 身份证正面
        /// </summary>
        /// <value></value>
        public string PositiveUrl { get; set; }
        /// <summary>
        /// 身份证反面
        /// </summary>
        /// <value></value>
        public string NegativeUrl { get; set; }
        /// <summary>
        /// 手持
        /// </summary>
        /// <value></value>
        public string CharacterUrl { get; set; }
    }
}