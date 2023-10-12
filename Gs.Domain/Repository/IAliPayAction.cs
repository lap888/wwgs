using Gs.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace Gs.Domain.Repository
{
    public interface IAliPayAction
    {
        /// <summary>
        /// 生成支付串
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Amount"></param>
        /// <param name="action"></param>
        /// <param name="Custom"></param>
        /// <returns></returns>
        Task<MyResult<object>> CreatePayUrl(int UserId, Decimal Amount, ActionType action, String Custom = "");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        Task<String> ChangeAliPay(String TradeNo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        Task<String> AuthAliPay(String TradeNo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        Task<String> CashRecharge(String TradeNo);

        /// <summary>
        /// 购物支付
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        Task<String> Shopping(String TradeNo);

    }
}
