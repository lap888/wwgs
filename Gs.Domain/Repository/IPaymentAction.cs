using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gs.Domain.Repository
{
    public interface IPaymentAction
    {
        /// <summary>
        /// 实名认证
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        Task<String> WePayNotify(String TradeNo);

        /// <summary>
        /// 修改支付宝
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        Task<String> ChangeAliPay(String TradeNo);

        /// <summary>
        /// 钱包充值
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        Task<String> CashRecharge(String TradeNo);

    }
}
