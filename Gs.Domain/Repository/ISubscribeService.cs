using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gs.Domain.Repository
{
    /// <summary>
    /// 消息订阅
    /// </summary>
    public interface ISubscribeService
    {
        /// <summary>
        /// 消息订阅 => 用户注册事件
        /// </summary>
        /// <param name="Msg">消息主体</param>
        /// <returns></returns>
        Task SubscribeMemberRegist(String Msg);

        /// <summary>
        /// 消息订阅 => 用户认证事件
        /// </summary>
        /// <param name="Msg">消息主体</param>
        /// <returns></returns>
        Task SubscribeMemberCertified(String Msg);

        /// <summary>
        /// 消息订阅 => 量化宝开启事件
        /// </summary>
        /// <param name="Msg"></param>
        /// <returns></returns>
        Task SubscribeTaskAction(String Msg);
    }
}
