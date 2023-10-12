using System;
using Gs.Domain.Entity;
using Gs.Domain.Models;
using Gs.Domain.Models.Store;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gs.Domain.Repository
{
    /// <summary>
    /// 商城
    /// </summary>
    public interface IStoreService
    {
        /// <summary>
        /// 积分兑换
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        Task<MyResult<Object>> Exchange(ExchangeModel exchange);

        /// <summary>
        /// 商品列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<ItemDetail>>> GoodsList(QueryModel query);

        /// <summary>
        /// 下单
        /// </summary>
        /// <returns></returns>
        Task<MyResult<Object>> SubOrder(SubmitOrder submit);

        /// <summary>
        /// 支付
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<Object>> SubPay(PaymentModel model);

        /// <summary>
        /// 我的订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<StoreOrder>>> MyOrders(QueryModel query);

        /// <summary>
        /// 收货
        /// </summary>
        /// <returns></returns>
        Task<MyResult<Object>> Receive(ReceiveModel model);

        #region 后台管理
        /// <summary>
        /// 添加商品
        /// </summary>
        /// <returns></returns>
        Task<MyResult<Object>> AddItem(StoreItem item);

        /// <summary>
        /// 删除商品
        /// </summary>
        /// <returns></returns>
        Task<MyResult<Object>> DelItem(StoreItem item);

        /// <summary>
        /// 编辑商品
        /// </summary>
        /// <returns></returns>
        Task<MyResult<Object>> ModifyItem(StoreItem item);

        /// <summary>
        /// 商品列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<StoreItem>>> ItemList(QueryModel query);

        /// <summary>
        /// 订单列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<StoreOrder>>> OrderList(QueryModel query);

        #endregion


    }
}
