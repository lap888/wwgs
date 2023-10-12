using System;
using Gs.Domain.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Gs.Domain.Enums;

namespace Gs.Domain.Repository
{
    /// <summary>
    /// 活跃度
    /// </summary>
    public interface IActiveService
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="Uid"></param>
        /// <returns></returns>
        Task Init(Int64 Uid);

        /// <summary>
        /// 活跃度信息
        /// </summary>
        /// <param name="Uid"></param>
        /// <returns></returns>
        Task<AccountInfo> Info(Int64 Uid);

        /// <summary>
        /// 活跃度 操作
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="Amount"></param>
        /// <param name="ModifyType"></param>
        /// <param name="UseFrozen"></param>
        /// <param name="Desc"></param>
        /// <returns></returns>
        Task<Boolean> ChangeAmount(Int64 Uid, Decimal Amount, ActiveModifyType ModifyType, Boolean UseFrozen, params string[] Desc);

        /// <summary>
        /// 活跃度记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<AccountRecord>>> Records(QueryModel query);

    }
}
