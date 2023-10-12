namespace Gs.Domain.Models.Dto
{
    public class FaceDto
    {
        public string CertName { get; set; }
        public string CertNo { get; set; }
        /// <summary>
        /// 支付宝号
        /// </summary>
        public string Alipay { get; set; }

        public string Metainfo { get; set; }
    }
    public class FaceModel
    {
        public string CertifyUrl { get; set; }
        public string CertifyId { get; set; }
    }
    public class FaceQueryModel
    {
        /// <summary>
        /// 是否通过，通过为T，不通过为F。	
        /// </summary>
        /// <value></value>
        public string Passed { get; set; }
        /// <summary>
        /// 认证的主体信息，一般的认证场景返回为空。
        /// </summary>
        /// <value></value>
        public string IdentityInfo { get; set; }
        /// <summary>
        /// 认证主体附件信息，主要为图片类材料，一般的认证场景都是返回空。
        /// </summary>
        /// <value></value>
        public string MaterialInfo { get; set; }
    }
}