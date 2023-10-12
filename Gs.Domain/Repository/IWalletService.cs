using Gs.Domain.Entity;
using Gs.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gs.Domain.Repository
{
    public interface IWalletService
    {
        /// <summary>
        /// 初始化钱包
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> InitWalletAccount(long userId);
        /// <summary>
        /// 获取钱包信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<UserAccountWallet>> WalletAccountInfo(long userId);
        /// <summary>
        /// 获取钱包流水
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <param name="ModifyType"></param>
        /// <returns></returns>
        Task<MyResult<List<UserAccountWalletRecord>>> WalletAccountRecord(long userId, int PageIndex = 1, int PageSize = 20, AccountModifyType ModifyType = AccountModifyType.ALL);
        /// <summary>
        /// 钱包账户余额冻结操作
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        Task<MyResult<object>> FrozenWalletAmount(long userId, decimal Amount);
        /// <summary>
        /// 钱包账户余额变动
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <param name="modifyType"></param>
        /// <param name="useFrozen">使用冻结金额</param>
        /// <param name="Desc">描述</param>
        /// <returns></returns>
        Task<MyResult<object>> ChangeWalletAmount(long userId, decimal Amount, AccountModifyType modifyType, bool useFrozen, params string[] Desc);
        /// <summary>
        /// 钱包现金充值
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="Amount"></param>
        /// <returns>反馈支付链接</returns>
        Task<MyResult<object>> Recharge(int userId, string type, decimal Amount);
        /// <summary>
        /// 现金钱包取现
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <param name="TradePwd"></param>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        Task<MyResult<object>> Withdraw(int userId, decimal Amount, string TradePwd, string TradeNo);
    }
}
