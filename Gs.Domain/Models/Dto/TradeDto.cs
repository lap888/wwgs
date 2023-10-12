namespace Gs.Domain.Models.Dto
{
    public class TradeDto
    {
        /// <summary>
        /// 买单数量
        /// </summary>
        /// <value></value>
        public int Amount { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        /// <value></value>
        public double Price { get; set; }
        /// <summary>
        /// 交易密码
        /// </summary>
        /// <value></value>
        public string TradePwd { get; set; }
        /// <summary>
        /// 支付宝账号
        /// </summary>
        /// <value></value>
        public string Alipay { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNum { get; set; }
        /// <summary>
        /// 坐标X
        /// </summary>
        public decimal LocationX { get; set; }
        /// <summary>
        /// 坐标Y
        /// </summary>
        public decimal LocationY { get; set; }
        /// <summary>
        /// 用户所在省份
        /// </summary>
        public string UserProvince { get; set; }
        /// <summary>
        /// 用户所在城市
        /// </summary>
        public string UserCity { get; set; }
        /// <summary>
        /// 城市代码
        /// </summary>
        public string CityCode { get; set; } = string.Empty;
        /// <summary>
        /// 用户所在地区
        /// </summary>
        public string UserArea { get; set; }
        /// <summary>
        /// 区县代码
        /// </summary>
        public string AreaCode { get; set; } = string.Empty;
    }
}