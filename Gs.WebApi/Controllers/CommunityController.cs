using Gs.Domain.Enums;
using Gs.Domain.Models;
using Gs.Domain.Models.BuyBack;
using Gs.Domain.Models.Community;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gs.WebApi.Controllers
{
    /// <summary>
    /// 社区运营中心
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CommunityController : BaseController
    {
        private readonly ICommunityService Community;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="communityService"></param>
        public CommunityController(ICommunityService communityService)
        {
            Community = communityService;
        }

        /// <summary>
        /// 评估
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> Assess([FromBody] BuyBackModel model)
        {
            return await Community.Assess(model);
        }

        /// <summary>
        /// 邮寄
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> SendPost([FromBody] BuyBackModel model)
        {
            MyResult<Object> result = new MyResult<object>();
            if (TokenModel.Id < 1) { return result.SetStatus(ErrorCode.ReLogin); }
            model.UserId = TokenModel.Id;
            return await Community.SendPost(model);
        }

        /// <summary>
        /// 我的回购订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<AssessOrder>>> BackOrder([FromBody] QueryModel query)
        {
            query.UserId = base.TokenModel.Id;
            MyResult<List<AssessOrder>> result = new MyResult<List<AssessOrder>>();
            if (query.UserId < 1) { return result.SetStatus(ErrorCode.ReLogin); }
            return await Community.AssessOrder(query);
        }

        /// <summary>
        /// 运营中心列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<CommunityInfo>>> List([FromBody] QueryModel query)
        {
            query.UserId = base.TokenModel.Id;
            return await Community.AppList(query);
        }

        #region 运营中心
        /// <summary>
        /// 运营中心订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<AssessOrder>>> Order([FromBody] QueryModel query)
        {
            query.UserId = base.TokenModel.Id;
            return await Community.StoreOrder(query);
        }

        /// <summary>
        /// 运营中心确认订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> Distribute([FromBody] AssessOrder order)
        {
            order.UserId = base.TokenModel.Id;
            return await Community.StoreDistribute(order.Id, (int)order.AssessIntegral, order.UserId);
        }

        #endregion



    }
}
