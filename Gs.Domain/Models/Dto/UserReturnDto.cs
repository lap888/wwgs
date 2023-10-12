namespace Gs.Domain.Models.Dto
{
    public class UserReturnDto
    {
        public string Name { get; set; }
        /// <summary>
        /// 钱包余额
        /// </summary>
        /// <value></value>
        public decimal UserBalanceNormal { get; set; }
        /// <summary>
        /// 冻结余额
        /// </summary>
        public decimal UserBalanceLock { get; set; }
        /// <summary>
        /// 日产量
        /// </summary>
        /// <value></value>
        public string DayNum { get; set; } = "0";
        /// <summary>
        /// 是否做今日量化宝
        /// </summary>
        /// <value></value>
        public int IsDoTask { get; set; } = 0;
        /// <summary>
        /// 手机号
        /// </summary>
        /// <value></value>
        public string Mobile { get; set; }

        /// <summary>
        /// 观看广告次数
        /// </summary>
        public int TotalWatch { get; set; }

        /// <summary>
        /// 认证广告次数
        /// </summary>
        public int AuthAdCount { get; set; }

        public string Level { get; set; }
        public int Golds { get; set; }
        public string Rcode { get; set; }
        public string inviterMobile { get; set; }

        public int Status { get; set; }
        public int AuditState { get; set; }

        public int IsPay { get; set; }

        public string AlipayUid { get; set; }

        /// <summary>
        /// 量化宝进度
        /// </summary>
        public decimal TaskSchedule { get; set; }

        /// <summary>
        /// NW
        /// </summary>
        public decimal Cotton { get; set; }

        /// <summary>
        /// 积分
        /// </summary>
        public decimal InteralSub { get; set; }

        /// <summary>
        /// 是否有社区
        /// </summary>
        public bool IsStore { get; set; }
        //可用
        public decimal CanUserCoin { get; set; }
        //余额
        public decimal BalanceUserCoin { get; set; }
        //冻结
        public decimal FrozenUserCoin { get; set; }
    }
}