using Gs.Domain.Entity;
using Gs.Domain.Models;
using Gs.Domain.Models.Store;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gs.WebApi.Controllers
{
    /// <summary>
    /// 商城
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MallController : BaseController
    {
        private readonly IStoreService StoreSub;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeService"></param>
        public MallController(IStoreService storeService)
        {
            StoreSub = storeService;
        }

        /// <summary>
        /// 积分兑换
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> Exchange([FromBody] ExchangeModel exchange)
        {
            exchange.UserId = base.TokenModel.Id;
            return await StoreSub.Exchange(exchange);
        }

        /// <summary>
        /// 商品列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MyResult<List<ItemDetail>>> List([FromBody] QueryModel query)
        {
            return await StoreSub.GoodsList(query);
        }

        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="submit"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> SubOrder([FromBody] SubmitOrder submit)
        {
            submit.UserId = base.TokenModel.Id;
            return await StoreSub.SubOrder(submit);
        }

        /// <summary>
        /// 下单支付
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> SubPay([FromBody] PaymentModel payment)
        {
            payment.UserId = base.TokenModel.Id;
            return await StoreSub.SubPay(payment);
        }

        /// <summary>
        /// 我的订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<StoreOrder>>> MyOrders(QueryModel query)
        {
            query.UserId = base.TokenModel.Id;
            return await StoreSub.MyOrders(query);
        }

        /// <summary>
        /// 收货
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> Receive(ReceiveModel model)
        {
            model.UserId = base.TokenModel.Id;
            return await StoreSub.Receive(model);
        }

    }
}
