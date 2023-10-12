using CSRedis;
using Gs.Core;
using Gs.Domain.Entity;
using Gs.Domain.Models;
using Gs.Domain.Models.Dto;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gs.WebApi.Controllers
{
    /// <summary>
    /// system 系统配置
    /// </summary>
    [Route("api/[controller]/[action]")]
    public class SystemController : BaseController
    {
        /// <summary>
        /// 
        /// </summary>
        public ISystemService SystemService { get; set; }
        private readonly CSRedisClient RedisCache;
        private readonly bool UseRedis = true;
        private readonly int CacheTime = 1 * 60 * 60;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemService"></param>
        /// <param name="memory"></param>
        /// <param name="redisClient"></param>
        public SystemController(ISystemService systemService, IMemoryCache memory, CSRedisClient redisClient)
        {
            SystemService = systemService;
            RedisCache = redisClient;
        }

        /// <summary>
        /// 获取系统下载版本
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MyResult<object>> ClientDownloadUrl(string name)
        {
            var key = $"System:ClientUrl_{name}";
            if (UseRedis)
            {
                try
                {
                    if (this.RedisCache.Exists(key)) { return this.RedisCache.Get<MyResult<object>>(key); }
                    var cacheResult = await SystemService.ClientDownloadUrl(name);
                    var cacheString = cacheResult.ToJson(false, true, true);
                    this.RedisCache.Set(key, cacheString, CacheTime, RedisExistence.Nx);
                    return cacheResult;
                }
                catch (Exception ex)
                {
                    SystemLog.Debug("获取系统下载版本", ex);
                    return await SystemService.ClientDownloadUrl(name);
                }
            }
            var result = await SystemService.ClientDownloadUrl(name);
            return result;
        }

        /// <summary>
        /// 获取用户关键信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<UserReturnDto>> InitInfo()
        {
            return await SystemService.UserInfo(base.TokenModel.Id);
        }

        /// <summary>
        /// 获取轮播图
        /// </summary>
        /// <param name="source">0 首页轮播 1 游戏页面轮播 2 广告</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MyResult<List<SysBanner>>> Banners(int source)
        {
            return await SystemService.AppBanner(new QueryModel()
            {
                Id = source,
                UserId = base.TokenModel.Id
            });
        }

        /// <summary>
        /// APP消息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MyResult<List<NoticeInfos>>> Notices([FromBody] NoticesDto model)
        {
            return await SystemService.AppNotice(new QueryModel()
            {
                Id = model.Type,
                UserId = base.TokenModel.Id
            });
        }

        /// <summary>
        /// 获取公告
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public MyResult<object> OneNotice()
        {
            var key = "System:OneNotice";
            if (UseRedis)
            {
                try
                {
                    if (this.RedisCache.Exists(key)) { return this.RedisCache.Get<MyResult<object>>(key); }
                    var cacheResult = SystemService.OneNotice();
                    var cacheString = cacheResult.ToJson(false, true, true);
                    this.RedisCache.Set(key, cacheString, CacheTime, RedisExistence.Nx);
                    return cacheResult;
                }
                catch (Exception ex)
                {
                    SystemLog.Error("REDIS缓存错误", ex);
                    return SystemService.OneNotice();
                }
            }
            MyResult<object> result = SystemService.OneNotice();
            return result;
        }

        /// <summary>
        /// 获取APP文案
        /// </summary>
        /// <param name="type">文案类型</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MyResult<object>> CopyWriting(string type)
        {
            return await SystemService.CopyWriting(type);
        }

        /// <summary>
        /// 会员反馈
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> Feedback([FromBody] UserFeedback feedback)
        {
            feedback.UserId = base.TokenModel.Id;
            return await SystemService.Feedback(feedback);
        }


    }
}
