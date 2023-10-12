using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Response
{
    /// <summary>
    /// 充值返回模型
    /// </summary>
    public class RspMobileRecharge
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        [JsonProperty("order_id")]
        public String OrderId { get; set; }

        /// <summary>
        /// 外部订单号
        /// </summary>
        [JsonProperty("customer_order_no")]
        public String CustomerOrderNo { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        [JsonProperty("product_id")]
        public Int64 ProductId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        [JsonProperty("product_name")]
        public String ProductName { get; set; }

        /// <summary>
        /// 充值账号
        /// </summary>
        [JsonProperty("charge_account")]
        public String ChargeAccount { get; set; }

        /// <summary>
        /// 购买数量
        /// </summary>
        [JsonProperty("buy_num")]
        public Decimal BuyNum { get; set; }

        /// <summary>
        /// 交易单价
        /// </summary>
        [JsonProperty("order_price")]
        public Decimal OrderPrice { get; set; }

        /// <summary>
        /// 订单类型：1-话费 2-流量 3-卡密 4-直充
        /// </summary>
        [JsonProperty("order_type")]
        public int OrderType { get; set; }

        /// <summary>
        /// 订单状态： （success：成功，processing：处理中，failed：失败，untreated：未处理）
        /// </summary>
        [JsonProperty("order_state")]
        public String OrderState { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty("create_time")]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty("finish_time")]
        public DateTime? FinishTime { get; set; }

        /// <summary>
        /// 运营商流水号
        /// </summary>
        [JsonProperty("operator_serial_number")]
        public String SerialNumber { get; set; }
    }
}
