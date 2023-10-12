using Gs.Domain.Entity;
using Gs.Domain.Models;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gs.WebAdmin.Controllers
{
    /// <summary>
    /// 商城
    /// </summary>
    public class MallController : WebBaseController
    {
        private readonly IStoreService MallSub;
        public MallController(IStoreService storeService)
        {
            MallSub = storeService;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> AddItem([FromBody] StoreItem item)
        {
            if (item.Id > 0)
            {
                return await MallSub.ModifyItem(item);
            }
            return await MallSub.AddItem(item);
        }

        /// <summary>
        /// 商品列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<StoreItem>>> ItemList([FromBody] QueryModel query)
        {
            return await MallSub.ItemList(query);
        }

        /// <summary>
        /// 订单列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<StoreOrder>>> OrderList([FromBody] QueryModel query)
        {
            return await MallSub.OrderList(query);
        }
    }
}
