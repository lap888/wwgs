using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Dto
{
    public class UserAccountWalletModel
    {
        public int Id { get; set; }

        public long AccountId { get; set; }
        public long UserId { get; set; }
        public int Type { get; set; }
        public string CoinType { get; set; }
        /// <summary>
        /// 总收入
        /// </summary>
        /// <value></value>
        public decimal Revenue { get; set; }
        /// <summary>
        /// 总支出
        /// </summary>
        /// <value></value>
        public decimal Expenses { get; set; }
        /// <summary>
        /// 余额
        /// </summary>
        /// <value></value>
        public decimal Balance { get; set; }
        /// <summary>
        /// 冻结
        /// </summary>
        /// <value></value>
        public decimal Frozen { get; set; }

        public DateTime? ModifyTime { get; set; }
    }

    public class CoinUserAccountWallet
    {
        /// <summary>
        /// MBM余额
        /// </summary>
        /// <value></value>
        public decimal BalanceCottonCoin { get; set; } = 0;
        /// <summary>
        /// MBM冻结
        /// </summary>
        /// <value></value>
        public decimal FrozenCottonCoin { get; set; } = 0;

        public List<UserAccountWalletModel> Lists = new List<UserAccountWalletModel>();
    }
}
