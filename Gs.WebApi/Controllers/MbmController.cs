using Gs.Domain.Entity;
using Gs.Domain.Enums;
using Gs.Domain.Models;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gs.WebApi.Controllers
{
    /// <summary>
    /// 帐户记录
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MbmController : BaseController
    {
        private readonly IAmbmService AmbmService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ambmService"></param>
        public MbmController(IAmbmService ambmService)
        {
            AmbmService = ambmService;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="inviderCode"></param>
        /// <param name="mobile"></param>
        /// <param name="secretKey"></param>
        /// <param name="publicKey"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MyResult<object>> DoMbm(string projectId, string inviderCode, string mobile, string secretKey, string publicKey, string device)
        {
            return await AmbmService.DoMbm(projectId, inviderCode, mobile, secretKey, publicKey, device);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pwd"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public MyResult<object> LoginMbm(string name, string pwd, string device)
        {
            return AmbmService.LoginMbm(name, pwd, device);
        }


    }
}
