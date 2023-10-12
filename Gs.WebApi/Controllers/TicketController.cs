using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSRedis;
using Gs.Domain.Entity;
using Gs.Domain.Models;
using Gs.Domain.Models.Ticket;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Gs.WebApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class TicketController : BaseController
    {
        private readonly CSRedisClient RedisCache;
        private readonly ITicketService TicketSub;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="redisClient"></param>
        /// <param name="ticketService"></param>
        public TicketController(CSRedisClient redisClient, ITicketService ticketService)
        {
            RedisCache = redisClient;
            TicketSub = ticketService;
        }

        /// <summary>
        /// 新人券页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<TicketModel>> Info()
        {
            return await TicketSub.TicketPage(base.TokenModel.Id);
        }

        /// <summary>
        /// 新人券开关
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> Switch()
        {
            return await TicketSub.TicketSwitch(base.TokenModel.Id);
        }

        /// <summary>
        /// 新人券量化宝
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> Task()
        {
            return await TicketSub.TicketTask(base.TokenModel.Id);
        }

        /// <summary>
        /// 新人券兑换
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Exchange([FromBody] TicketExchange exchange)
        {
            exchange.UserId = base.TokenModel.Id;
            return await TicketSub.ExchangeTicket(exchange);
        }

        /// <summary>
        /// 使用新人券
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<Object>> Use()
        {
            return await TicketSub.UseTicket(base.TokenModel.Id);
        }

        /// <summary>
        /// 新人券记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<UserAccountTicketRecord>>> Records(QueryModel query)
        {
            query.UserId = base.TokenModel.Id;
            return await TicketSub.TicketRecords(query);
        }
    }
}