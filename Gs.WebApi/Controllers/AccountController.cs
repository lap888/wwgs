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
    public class AccountController : BaseController
    {
        private readonly IActiveService ActiveSub;
        private readonly ICottonService CottonSub;
        private readonly IWalletService WalletSub;
        private readonly IAdditionService AdditionSub;
        private readonly IIntegralService IntegralSub;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="cottonService"></param>
        /// <param name="activeService"></param>
        /// <param name="walletService"></param>
        /// <param name="additionService"></param>
        /// <param name="integralService"></param>
        public AccountController(ICottonService cottonService, IActiveService activeService, IWalletService walletService,
            IAdditionService additionService, IIntegralService integralService)
        {
            ActiveSub = activeService;
            CottonSub = cottonService;
            WalletSub = walletService;
            AdditionSub = additionService;
            IntegralSub = integralService;
        }

        /// <summary>
        /// 活跃度信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<AccountInfo>> Active()
        {
            MyResult<AccountInfo> result = new MyResult<AccountInfo>();
            if (base.TokenModel.Id < 1)
            {
                return result.SetStatus(ErrorCode.ReLogin);
            }
            result.Data = await ActiveSub.Info(base.TokenModel.Id);
            return result;
        }

        /// <summary>
        /// 活跃度记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<AccountRecord>>> ActiveRecord([FromBody] QueryModel query)
        {
            query.UserId = base.TokenModel.Id;
            return await ActiveSub.Records(query);
        }

        /// <summary>
        /// MBM信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<AccountInfo>> Cotton()
        {
            MyResult<AccountInfo> result = new MyResult<AccountInfo>();
            if (base.TokenModel.Id < 1)
            {
                return result.SetStatus(ErrorCode.ReLogin);
            }
            result.Data = await CottonSub.Info(base.TokenModel.Id);
            return result;
        }

        /// <summary>
        /// MBM记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<AccountRecord>>> CottonRecord([FromBody] QueryModel query)
        {
            query.UserId = base.TokenModel.Id;
            return await CottonSub.Records(query);
        }

        /// <summary>
        /// 钱包信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<UserAccountWallet>> Wallet()
        {
            return await WalletSub.WalletAccountInfo(base.TokenModel.Id); ;
        }

        /// <summary>
        /// 钱包记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<UserAccountWalletRecord>>> WalletRecord([FromBody] QueryModel query)
        {
            query.UserId = base.TokenModel.Id;
            return await WalletSub.WalletAccountRecord(query.UserId, query.PageIndex, query.PageSize);
        }

        /// <summary>
        /// 加成活跃信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<AccountInfo>> Addition()
        {
            MyResult<AccountInfo> result = new MyResult<AccountInfo>();
            if (base.TokenModel.Id < 1)
            {
                return result.SetStatus(ErrorCode.ReLogin);
            }
            result.Data = await AdditionSub.Info(base.TokenModel.Id);
            return result;
        }

        /// <summary>
        /// 加成活跃度记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<AccountRecord>>> AdditionRecord([FromBody] QueryModel query)
        {
            query.UserId = base.TokenModel.Id;
            return await AdditionSub.Records(query);
        }

        /// <summary>
        /// 积分信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<AccountInfo>> Integral()
        {
            MyResult<AccountInfo> result = new MyResult<AccountInfo>();
            if (base.TokenModel.Id < 1)
            {
                return result.SetStatus(ErrorCode.ReLogin);
            }
            result.Data = await IntegralSub.Info(base.TokenModel.Id);
            return result;
        }

        /// <summary>
        /// 积分记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<AccountRecord>>> IntegralRecord([FromBody] QueryModel query)
        {
            query.UserId = base.TokenModel.Id;
            return await IntegralSub.Records(query);
        }


    }
}
