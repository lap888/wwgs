using Gs.Domain.Models;
using Gs.Domain.Models.Dto;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Gs.WebApi.Controllers
{
    /// <summary>
    /// 团队
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TeamController : BaseController
    {
        private readonly ITeamService Team;
        
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="teamService"></param>
        public TeamController(ITeamService teamService)
        {
            Team = teamService;
        }

        /// <summary>
        /// 团队信息 For wwgs
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Info([FromBody] TeamInfosReqDto query)
        {
            query.UserId = this.TokenModel.Id;
            query.Mobile = this.TokenModel.Mobile;
            return await Team.TeamInfos(query);
        }

        /// <summary>
        /// 团队信息 For 交易所
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> InfoForExchange([FromBody] TeamInfosReqDto query)
        {
            query.UserId = this.TokenModel.Id;
            query.Mobile = this.TokenModel.Mobile;
            return await Team.TeamInfos(query);
        }


    }
}
