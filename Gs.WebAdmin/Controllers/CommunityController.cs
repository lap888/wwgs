using Gs.Domain.Entity;
using Gs.Domain.Models;
using Gs.Domain.Models.Community;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gs.WebAdmin.Controllers
{
    /// <summary>
    /// 社区运营中心
    /// </summary>
    public class CommunityController : Controller
    {
        private readonly ICommunityService Community;
        public CommunityController(ICommunityService communityService)
        {
            Community = communityService;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 添加社区运营中心
        /// </summary>
        /// <param name="community"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> Add([FromBody] CommunityInfo community)
        {
            if (community.Id < 1)
            {
                return await Community.Add(community);
            }
            return await Community.Modify(community);
        }

        /// <summary>
        /// 社区运营中心列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<CommunityInfo>>> List([FromBody] QueryModel query)
        {
            return await Community.List(query);
        }

        /// <summary>
        /// 回购订单列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<AssessOrder>>> BackOrder([FromBody] QueryModel query)
        {
            return await Community.BackOrder(query);
        }

        /// <summary>
        /// 发放积分
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> Distribute([FromBody] CommunityBackOrder order)
        {
            return await Community.Distribute(order);
        }

    }
}
