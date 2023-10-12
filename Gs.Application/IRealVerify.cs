using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Aliyun.Acs.Cloudauth.Model.V20190307;
using Gs.Application.Response;

namespace Gs.Application
{
    /// <summary>
    /// 实名认证
    /// </summary>
    public interface IRealVerify
    {
        /// <summary>
        /// 执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<T> Execute<T>(Utils.IVerifyRequest<T> request) where T : Utils.RealVerifyResponse, new();
        RspRealVerifyInitiate ExecuteNew(string OuterOrderNo, string CertName, string CertNo, string MetaInfo, string UserId);
        RspRealVerifyInitiate DescribeFaceVerify(long SceneId, string CertifyId);
    }
}
