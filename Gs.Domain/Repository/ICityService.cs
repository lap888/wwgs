using Gs.Domain.Entity;
using Gs.Domain.Models;
using Gs.Domain.Models.City;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gs.Domain.Repository
{
    /// <summary>
    /// 城市合伙人
    /// </summary>
    public interface ICityService
    {




        #region 后台管理
        /// <summary>
        /// 添加合伙人
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        Task<MyResult<Object>> Add(PartnerInfo city);

        /// <summary>
        /// 添加合伙人
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        Task<MyResult<Object>> Edit(PartnerInfo city);

        /// <summary>
        /// 合伙人列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<PartnerInfo>>> List(QueryModel query);
        #endregion
    }
}
