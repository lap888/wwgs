using System;
using Gs.Domain.Models;
using Gs.Domain.Models.Dto;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gs.Domain.Repository
{
    public interface ITradeService
    {
        /// <summary>
        /// 交易面板统计
        /// </summary>
        /// <returns></returns>
        MyResult<CoinTradeExt> GetTradeTotal(int userId);
        /// <summary>
        /// 买单单列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<List<CoinTradeDto>>> TradeList(TradeReqDto model);
        /// <summary>
        /// 发布买单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> StartBuy(TradeDto model, int userId);
        
        /// <summary>
        /// 确认出售
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> DealBuy(TradeDto model, int userId);


        /// <summary>
        /// 取消买单
        /// </summary>
        /// <param name="orderNum"></param>
        /// <param name="tradePwd"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> CancleTrade(string orderNum, string tradePwd, int userId);

        /// <summary>
        /// 我的交易订单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<List<TradeListReturnDto>>> MyTradeList(MyTradeListDto model, int userId);

        Task<MyResult<object>> Paid(PaidDto model, int userId);
        //确认收款 发送MBM
        Task<MyResult<object>> PaidCoin(PaidDto model, int userId);
        //强制发送MBM
        MyResult<object> ForcePaidCoin();
        //申诉
        MyResult<object> CreateAppeal(CreateAppealDto model, int userId);

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<ListModel<TradeOrder>>> QueryTradeOrder(QueryTradeOrder query);

        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<MyResult<object>> CloseTradeOrder(TradeOrder order);

        /// <summary>
        /// 恢复订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<MyResult<object>> ResumeTradeOrder(TradeOrder order);

        /// <summary>
        /// 恢复订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<MyResult<object>> BanBuyer(TradeOrder order);

        /// <summary>
        /// 解除超时封禁
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<MyResult<object>> Unblock(TradeOrder order);

        /// <summary>
        /// 查看申诉
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<MyResult<List<TradeAppeals>>> ViewAppeal(TradeOrder order);
    }
}