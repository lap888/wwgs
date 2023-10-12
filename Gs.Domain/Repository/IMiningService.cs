using Gs.Domain.Enums;
using Gs.Domain.Models;
using Gs.Domain.Models.Admin;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gs.Domain.Repository
{
    /// <summary>
    /// 量化宝服务
    /// </summary>
    public interface IMiningService
    {
        /// <summary>
        /// 量化宝商店
        /// </summary>
        /// <returns></returns>
        Task<MyResult<List<MiningBase>>> Store();

        /// <summary>
        /// 我的量化宝
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        Task<MyResult<List<MiningInfo>>> MyBase(Int32 UserId, MiningStatus Status);

        /// <summary>
        /// 兑换量化宝
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="BaseId"></param>
        /// <param name="Source"></param>
        /// <returns></returns>
        Task<MyResult<Object>> Exchange(Int64 UserId, Int32 BaseId, MiningSource Source = MiningSource.EXCHANGE_MINER);

        /// <summary>
        /// 挖矿
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        Task<MyResult<Object>> DigMine(Int64 UserId);

        

        #region 后台管理

        /// <summary>
        /// 量化宝列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<MiningInfo>>> BaseList(QueryMining query);

        /// <summary>
        /// 量化宝延期
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        Task<MyResult<Object>> Extension(MiningInfo info);

        /// <summary>
        /// 关闭量化宝
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        Task<MyResult<Object>> Colse(MiningInfo info);

        /// <summary>
        /// 赠送量化宝
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        Task<MyResult<Object>> Grant(QueryMining info);
        #endregion
    }
}
