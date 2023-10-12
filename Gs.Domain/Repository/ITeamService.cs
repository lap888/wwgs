using Gs.Domain.Models;
using Gs.Domain.Models.Dto;
using System.Threading.Tasks;

namespace Gs.Domain.Repository
{
    /// <summary>
    /// 团队操作
    /// </summary>
    public interface ITeamService
    {
        /// <summary>
        /// 团队信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<object>> TeamInfos(TeamInfosReqDto model);
        
        /// <summary>
        /// 更新LEVEL
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> UpdateLevel(int userId);

        /// <summary>
        /// 团队信息 For Ex
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<object>> InfoForExchange(TeamInfosReqDto model);

        /// <summary>
        /// 更新团队人数
        /// </summary>
        /// <param name="Uid">用户ID</param>
        /// <param name="Number">变更人员数量(默认+1)</param>
        /// <returns></returns>
        Task<bool> UpdateTeamPersonnel(long Uid, int Number = 1);

        /// <summary>
        /// 更新团队活跃
        /// </summary>
        /// <param name="Uid">用户ID</param>
        /// <param name="Quantity">更新数量(默认+1)</param>
        /// <returns></returns>
        Task<bool> UpdateTeamKernel(long Uid, decimal Quantity = 1);

        /// <summary>
        /// 更新某个团队的直推人数
        /// </summary>
        /// <param name="Uid">用户ID(不传或NULL则更新所有用户)</param>
        /// <param name="Status">计入直推人数的用户状态(默认:已认证)</param>
        /// <returns></returns>
        Task<bool> UpdateTeamDirectPersonnel(long? Uid, int? Status = 2);

        /// <summary>
        /// 获取用户推荐关系
        /// </summary>
        /// <param name="Uid">用户ID</param>
        /// <returns></returns>
        Task<MemberRelation> GetRelation(long Uid);

        /// <summary>
        /// 设置用户推荐关系
        /// </summary>
        /// <param name="Uid">用户ID</param>
        /// <param name="PUid">父级推荐人ID</param>
        /// <returns></returns>
        Task<MemberRelation> SetRelation(long Uid, long PUid);

    }
}
