using Gs.Core;
using Gs.Domain.Enums;
using Gs.Domain.Models;
using Gs.Domain.Models.Dto;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gs.WebApi.Controllers
{
    /// <summary>
    /// 交易所
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CoinController : BaseController
    {
        private readonly ICoinService Coin;
        private readonly IConchService Conch;
        private readonly ICottonService CottonService;
        private readonly IHonorService HonorService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coin"></param>
        /// <param name="conchService"></param>
        /// <param name="cottonService"></param>
        /// <param name="honorService"></param>
        public CoinController(ICoinService coin, IConchService conchService, ICottonService cottonService, IHonorService honorService)
        {
            Coin = coin;
            CottonService = cottonService;
            Conch = conchService;
            HonorService = honorService;
        }

        /// <summary>
        /// NW兑换Gas
        /// </summary>
        /// <param name="cotton"></param>
        /// <param name="passward"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MyResult<object>> CottonExCoin(decimal cotton, string passward)
        {
            return await Coin.CottonExCoin(this.TokenModel.Id, cotton, passward);
        }

        /// <summary>
        /// 获取账户币资产
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<CoinUserAccountWallet>> FindCoinAmount()
        {
            var userId = base.TokenModel.Id;
            return await Coin.FindCoinAmount(userId);
        }
        /// <summary>
        /// MBM换Gas记录
        /// </summary>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <param name="ModifyType"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> ExchangeRecord(int PageIndex = 1, int PageSize = 20, ConchModifyType ModifyType = ConchModifyType.Sys_Ex)
        {
            var id = this.TokenModel.Id;
            return await Coin.ExchangeRecord(id, PageIndex, PageSize, ConchModifyType.EXCHANGE_CONCH);
        }
        /// <summary>
        /// 各币种明细记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<AccountRecord>>> CoinRecord([FromBody] QueryCoinRecordDto query)
        {
            MyResult<List<AccountRecord>> result = new MyResult<List<AccountRecord>>();
            query.UserId = this.TokenModel.Id;
            if (query.CoinType.Equals("NW"))
            {
                return await Conch.Records(query);
            }
            // else if (query.CoinType.Equals("贡献值"))
            // {
            //     return await CottonService.Records(query);
            // }
            else if (query.CoinType.Equals("Gas"))
            {
                return await HonorService.Records(query);
            }
            else
            {
                return result.SetStatus(ErrorCode.NoAuth, "类型有误...");
            }
        }

    }
}
