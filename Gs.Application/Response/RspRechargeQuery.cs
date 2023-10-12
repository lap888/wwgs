using Gs.Application.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Response
{
    /// <summary>
    /// 充值订单查询
    /// </summary>
    public class RspRechargeQuery
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
        public String ProductId { get; set; }

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
        public Int32 BuyNum { get; set; }

        /// <summary>
        /// 交易单价
        /// </summary>
        [JsonProperty("order_price")]
        public Decimal OrderPrice { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        [JsonProperty("order_type")]
        public String OrderType { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        [JsonProperty("order_state")]
        public RechargeState OrderState { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty("create_time")]
        public String CreateTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        [JsonProperty("finish_time")]
        public String FinishTime { get; set; }

        /// <summary>
        /// 充值区（中文）,仅网游直充订单返回
        /// </summary>
        [JsonProperty("area")]
        public String Area { get; set; }

        /// <summary>
        /// 充值区（中文）,仅网游直充订单返回
        /// </summary>
        [JsonProperty("server")]
        public String Server { get; set; }

        /// <summary>
        /// 充值区（中文）,仅网游直充订单返回
        /// </summary>
        [JsonProperty("type")]
        public String Type { get; set; }

        /// <summary>
        /// 卡密信息，仅卡密订单返回（注意：卡密是密文，需要进行解密使用）
        /// </summary>
        [JsonProperty("cards")]
        public String Cards { get; set; }

        /// <summary>
        /// 运营商流水号
        /// </summary>
        [JsonProperty("operator_serial_number")]
        public String OperatorSerialNumber { get; set; }

    }
}
