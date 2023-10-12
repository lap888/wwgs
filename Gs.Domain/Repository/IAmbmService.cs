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
    public interface IAmbmService
    {
        //登录
        MyResult<object> LoginMbm(string name, string pwd, string device);
        //注册实名
        Task<MyResult<object>> DoMbm(string projectId, string inviderCode, string mobile, string secretKey, string publicKey, string device);

        Task<MyResult<object>> List(QueryTradeOrder query);

        Task<MyResult<object>> AddUser(string name,string pubkey);

        Task<MyResult<object>> AddAuth(int userId,decimal amount);

        Task<MyResult<object>> AddMsg(int userId,decimal amount);


    }
}
