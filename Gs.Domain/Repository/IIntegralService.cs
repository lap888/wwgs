using System;
using Gs.Domain.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Gs.Domain.Enums;

namespace Gs.Domain.Repository
{
    /// <summary>
    /// 积分
    /// </summary>
    public interface IIntegralService
    {
        Task Init(Int64 Uid);
        /// <summary>
        /// 积分信息
        /// </summary>
        /// <param name="Uid"></param>
        /// <returns></returns>
        Task<AccountInfo> Info(Int64 Uid);

        /// <summary>
        /// 积分冻结
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        Task<Boolean> Frozen(Int64 Uid, Decimal Amount);

        /// <summary>
        /// 积分记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<AccountRecord>>> Records(QueryModel query);

        /// <summary>
        /// 变更
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="Amount"></param>
        /// <param name="UseFrozen">使用冻结金额，账户金额增加时，此参数无效</param>
        /// <param name="ModifyType">账户变更类型</param>
        /// <param name="Desc">描述</param>
        /// <returns></returns>
        Task<Boolean> ChangeAmount(Int64 Uid, Decimal Amount, IntegralModifyType ModifyType, Boolean UseFrozen, params string[] Desc);
    }
}
