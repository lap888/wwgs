using Gs.Domain.Entity;
using Gs.Domain.Models;
using Gs.Domain.Models.Admin;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gs.WebAdmin.Controllers
{
    public class SystemController : Controller
    {
        private readonly ISystemService SystemSub;
        public SystemController(ISystemService systemService)
        {
            SystemSub = systemService;
        }

        /// <summary>
        /// 消息列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<NoticeInfos>>> NoticeList([FromBody] QueryModel query)
        {
            return await SystemSub.NoticeList(query);
        }

        /// <summary>
        /// 添加消息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> AddNotice([FromBody] NoticeInfos infos)
        {
            if (infos.Id > 0)
            {
                return await SystemSub.ModifyNotice(infos);
            }
            return await SystemSub.AddNotice(infos);
        }

        /// <summary>
        /// 轮播列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<SysBanner>>> BannerList([FromBody] QueryModel query)
        {
            return await SystemSub.BannerList(query);
        }

        /// <summary>
        /// 添加轮播
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> AddBanner([FromBody] SysBanner banner)
        {
            if (banner.Id > 0)
            {
                return await SystemSub.ModifyBanner(banner);
            }
            return await SystemSub.AddBanner(banner);
        }

        /// <summary>
        /// 反馈列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<AdminFeedback>>> UserFeedback([FromBody] QueryModel query)
        {
            return await SystemSub.UserFeedback(query);
        }

        /// <summary>
        /// 处理反馈
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> HandleFeedback([FromBody] AdminFeedback feedback)
        {
            return await SystemSub.HandleFeedback(feedback);
        }

    }
}
