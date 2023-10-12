using Gs.Domain.Entity;
using Gs.Domain.Models;
using Gs.Domain.Models.BuyBack;
using Gs.Domain.Models.Community;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gs.Domain.Repository
{
    /// <summary>
    /// 社区运营中心
    /// </summary>
    public interface ICommunityService
    {
        /// <summary>
        /// 评估
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<Object>> Assess(BuyBackModel model);

        /// <summary>
        /// 邮寄
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<Object>> SendPost(BuyBackModel model);

        /// <summary>
        /// 中心列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<CommunityInfo>>> AppList(QueryModel query);

        /// <summary>
        /// 我的评估单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<AssessOrder>>> AssessOrder(QueryModel query);

        /// <summary>
        /// 确定收货
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<MyResult<Object>> Received(CommunityBackOrder order);

        #region 社区管理

        /// <summary>
        /// 社区评估单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<AssessOrder>>> StoreOrder(QueryModel query);

        /// <summary>
        /// 社区发放积分
        /// </summary>
        /// <param name="OrderId"></param>
        /// <param name="Integral"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        Task<MyResult<Object>> StoreDistribute(Int64 OrderId, Int32 Integral, Int64 UserId);

        #endregion


        #region 后台管理
        /// <summary>
        /// 添加社区运营中心
        /// </summary>
        /// <param name="community"></param>
        /// <returns></returns>
        Task<MyResult<Object>> Add(CommunityInfo community);

        /// <summary>
        /// 修改社区运营中心
        /// </summary>
        /// <param name="community"></param>
        /// <returns></returns>
        Task<MyResult<Object>> Modify(CommunityInfo community);

        /// <summary>
        /// 社区运营中心列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<CommunityInfo>>> List(QueryModel query);

        /// <summary>
        /// 回购订单列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<AssessOrder>>> BackOrder(QueryModel query);

        /// <summary>
        /// 发放积分
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<MyResult<Object>> Distribute(CommunityBackOrder order);
        #endregion


    }
}
