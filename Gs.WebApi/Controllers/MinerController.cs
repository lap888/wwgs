using Gs.Domain.Enums;
using Gs.Domain.Models;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gs.WebApi.Controllers
{
    /// <summary>
    /// 量化宝
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MinerController : BaseController
    {
        private readonly IMiningService Mining;
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="miningService"></param>
        public MinerController(IMiningService miningService)
        {
            Mining = miningService;
        }

        /// <summary>
        /// 量化宝商店
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<List<MiningBase>>> Store()
        {
            return await Mining.Store();
        }

        /// <summary>
        /// 我的量化宝
        /// </summary>
        /// <param name="Status"></param>
        /// <returns></returns>
        [HttpGet("{Status}")]
        public async Task<MyResult<List<MiningInfo>>> MyBase(MiningStatus Status)
        {
            return await Mining.MyBase(base.TokenModel.Id, Status);
        }

        /// <summary>
        /// 兑换量化宝
        /// </summary>
        /// <param name="Bid"></param>
        /// <returns></returns>
        [HttpGet("Bid")]
        public async Task<MyResult<Object>> Exchange(Int32 Bid)
        {
            return await Mining.Exchange(base.TokenModel.Id, Bid);
        }

        /// <summary>
        /// 挖矿
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<Object>> DigMine()
        {
            return await Mining.DigMine(base.TokenModel.Id);
        }

    }
}
