namespace Gs.Domain.Models.Dto
{
    public class PaidDto
    {
        /// <summary>
        /// 支付截图
        /// </summary>
        /// <value></value>
        public string PicUrl { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        /// <value></value>
        public string OrderNum { get; set; }
        /// <summary>
        /// 交易密码
        /// </summary>
        /// <value></value>
        public string TradePwd { get; set; }
    }
}