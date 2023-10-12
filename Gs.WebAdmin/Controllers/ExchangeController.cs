using Gs.Core.Action;
using Gs.Domain.Models;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gs.WebAdmin.Controllers
{
    public class ExchangeController : WebBaseController
    {
        private readonly ITradeService TradeSub;
        public ExchangeController(ITradeService tradeService)
        {
            TradeSub = tradeService;
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 订单列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<ListModel<TradeOrder>>> List([FromBody]QueryTradeOrder query)
        {
            return await TradeSub.QueryTradeOrder(query);
        }

        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Close([FromBody]TradeOrder trade)
        {
            return await TradeSub.CloseTradeOrder(trade);
        }

        /// <summary>
        /// 恢复订单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Resume([FromBody]TradeOrder trade)
        {
            return await TradeSub.ResumeTradeOrder(trade);
        }

        /// <summary>
        /// 封禁买家
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> BanBuyer([FromBody]TradeOrder trade)
        {
            return await TradeSub.BanBuyer(trade);
        }

        /// <summary>
        /// 解除超时封禁
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Unblock([FromBody]TradeOrder trade)
        {
            return await TradeSub.Unblock(trade);
        }

        /// <summary>
        /// 查看原因
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<TradeAppeals>>> ViewAppeal([FromBody]TradeOrder trade)
        {
            return await TradeSub.ViewAppeal(trade);
        }

        /// <summary>
        /// 订单管理
        /// </summary>
        /// <returns></returns>
        [Action("订单管理", ActionType.ExchangeManager, 1)]
        public ViewResult XzxOrderManager() { return View(); }


    }
}
