using System;
using Gs.Domain.Enums;
using Gs.Domain.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gs.Domain.Repository
{
    /// <summary>
    /// 荣誉值
    /// </summary>
    public interface IHonorService
    {
        Task Init(Int64 Uid);
        /// <summary>
        /// 荣誉值信息
        /// </summary>
        /// <param name="Uid"></param>
        /// <returns></returns>
        Task<AccountInfo> Info(Int64 Uid);

        /// <summary>
        /// 荣誉值冻结
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        Task<Boolean> Frozen(Int64 Uid, Decimal Amount);

        /// <summary>
        /// 荣誉值记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<AccountRecord>>> Records(QueryModel query);

        /// <summary>
        /// 账户变更
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="Amount"></param>
        /// <param name="UseFrozen">使用冻结金额，账户金额增加时，此参数无效</param>
        /// <param name="ModifyType">账户变更类型</param>
        /// <param name="Desc">描述</param>
        /// <returns></returns>
        Task<Boolean> ChangeAmount(Int64 Uid, Decimal Amount, HonorModifyType ModifyType, Boolean UseFrozen, params string[] Desc);
    }
}
