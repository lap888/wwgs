using Gs.Domain.Enums;
using Gs.Domain.Models.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gs.Domain.Repository
{
    public interface ICoinService
    {
        /// <summary>
        /// MBM换MBM
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cotton"></param>
        /// <param name="passward"></param>
        /// <returns></returns>
        Task<MyResult<object>> CottonExCoin(int userId, decimal cotton, string passward);

        /// <summary>
        /// 资产列表
        /// </summary>
        /// <param name="type">0币币 1发币</param>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        Task<MyResult<CoinUserAccountWallet>> FindCoinAmount(long userId);

        /// <summary>
        /// MBM换MBM记录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <param name="ModifyType"></param>
        /// <returns></returns>
        Task<MyResult<object>> ExchangeRecord(long userId, int PageIndex = 1, int PageSize = 20, ConchModifyType ModifyType = ConchModifyType.Sys_Ex);

        /// <summary>
        /// 获取数字资产明细
        /// </summary>
        /// <param name="coinType"></param>
        /// <param name="accountId"></param>
        /// <param name="userId"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <param name="ModifyType"></param>
        /// <returns></returns>
        Task<MyResult<List<UserCoinRecordDto>>> CoinAccountRecord(string coinType,long accountId, long userId, int PageIndex = 1, int PageSize = 20, CottonModifyType ModifyType = CottonModifyType.ALL);

    }
}
