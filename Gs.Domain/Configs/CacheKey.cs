using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Configs
{
    /// <summary>
    /// 缓存键
    /// </summary>
    public static class CacheKey
    {
        /// <summary>
        /// 会员头像上传锁
        /// </summary>
        public static String UpHeadPicLock(Int32 UserId)
        {
            return $"Lock:UpHeadPic_{UserId}";
        }

        /// <summary>
        /// 修改密码锁
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public static String ModifyLoginPwdLock(Int32 UserId)
        {
            return $"Lock:ModifyPwdLogin_{UserId}";
        }

        /// <summary>
        /// 修改昵称锁
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public static String ModifyNickLock(Int32 UserId)
        {
            return $"Lock:ModifyNick_{UserId}";
        }
    }
}
