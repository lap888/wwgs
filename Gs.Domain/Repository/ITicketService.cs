using System;
using Gs.Domain.Entity;
using Gs.Domain.Models;
using System.Threading.Tasks;
using Gs.Domain.Models.Ticket;
using System.Collections.Generic;
using Gs.Domain.Enums;

namespace Gs.Domain.Repository
{
    /// <summary>
    /// 新人券
    /// </summary>
    public interface ITicketService
    {
        /// <summary>
        /// 初始化新人券账户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        Task<MyResult<object>> InitAccount(long userId);

        /// <summary>
        /// 新人券页面
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<TicketModel>> TicketPage(long userId);

        /// <summary>
        /// 新人券信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<UserAccountTicket>> TicketInfo(long userId);

        /// <summary>
        /// 新人券兑换
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        Task<MyResult<Object>> ExchangeTicket(TicketExchange exchange);

        /// <summary>
        /// 新人券开关
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        Task<MyResult<Object>> TicketSwitch(Int64 UserId);

        /// <summary>
        /// 使用新人券
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        Task<MyResult<Object>> UseTicket(long UserId);

        /// <summary>
        /// 新人券量化宝
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        Task<MyResult<Object>> TicketTask(long UserId);

        /// <summary>
        /// 新人券记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<UserAccountTicketRecord>>> TicketRecords(QueryModel query);

        Task<MyResult<object>> ChangeAmount(long userId, decimal Amount, TicketModifyType modifyType, bool useFrozen, params string[] Desc);
    }
}
