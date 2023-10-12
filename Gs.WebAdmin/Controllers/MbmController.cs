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
    public class MbmController : WebBaseController
    {
        private readonly IAmbmService MbmSub;
        public MbmController(IAmbmService mbmService)
        {
            MbmSub = mbmService;
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 账户管理
        /// </summary>
        /// <returns></returns>
        [Action("账户管理", ActionType.NW, 1)]
        public ViewResult AccountRecord() { return View(); }

        /// <summary>
        /// 订单列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> List([FromBody] QueryTradeOrder query)
        {
            return await MbmSub.List(query);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pubkey"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> AddUser(string name, string pubkey)
        {
            return await MbmSub.AddUser(name, pubkey);
        }

        /// <summary>
        /// 添加实名or信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="amount"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> AddAuthOrMsg(int userId, decimal amount, int type)
        {

            if (type == 1)
            {
                return await MbmSub.AddAuth(userId, amount);
            }
            else if (type == 2)
            {
                return await MbmSub.AddMsg(userId, amount);
            }
            else
            {
                return new MyResult<object>(500, "类型有误");
            }
        }




    }
}
