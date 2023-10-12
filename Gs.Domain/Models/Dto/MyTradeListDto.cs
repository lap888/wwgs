namespace Gs.Domain.Models.Dto
{
    public class MyTradeListDto:BaseModel
    {
        /// <summary>
        /// 买卖
        /// </summary>
        public string Sale { get; set; }
        /// <summary>
        /// 1 我的买单
        /// </summary>
        /// <value></value>
        public int Status { get; set; }
        /// <summary>
        /// 券类型
        /// </summary>
        /// <value></value>
        public string CoinType { get; set; }
    }
}