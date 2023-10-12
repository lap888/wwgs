
using Gs.Domain.Configs;
using Gs.Domain.Entity;
using Gs.Domain.Models;
using Gs.Domain.Models.Admin;
using Gs.Domain.Models.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gs.Domain.Repository
{
    public interface ISystemService
    {
        /// <summary>
        /// 广告 轮播
        /// </summary>
        /// <param name="source"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        MyResult<object> Banners(int source, long uid = 0);

        MyResult<object> Notices(NoticesDto model, int userId);
        MyResult<object> OneNotice();
        MyResult<object> AppDownloadUrl();
        Task<MyResult<object>> CopyWriting(string type);
        Task<MyResult<UserReturnDto>> UserInfo(int userId);

        /// <summary>
        /// APP消息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<NoticeInfos>>> AppNotice(QueryModel query);

        /// <summary>
        /// app版本信息
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns></returns>
        Task<MyResult<object>> ClientDownloadUrl(string systemName);

        /// <summary>
        /// APP轮播
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<SysBanner>>> AppBanner(QueryModel query);

        /// <summary>
        /// 交易所消息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<NoticeInfos>>> ExchangeNotice(QueryModel query);

        /// <summary>
        /// 交易所轮播
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<SysBanner>>> ExchangeBanner(QueryModel query);

        /// <summary>
        /// 会员反馈
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<Object>> Feedback(UserFeedback feedback);

        #region 后台管理

        /// <summary>
        /// 反馈列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<AdminFeedback>>> UserFeedback(QueryModel query);

        /// <summary>
        /// 处理反馈
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<Object>> HandleFeedback(AdminFeedback feedback);

        /// <summary>
        /// 消息列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<NoticeInfos>>> NoticeList(QueryModel query);

        /// <summary>
        /// 添加消息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<Object>> AddNotice(NoticeInfos infos);

        /// <summary>
        /// 修改消息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<Object>> ModifyNotice(NoticeInfos infos);

        /// <summary>
        /// 轮播列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<SysBanner>>> BannerList(QueryModel query);

        /// <summary>
        /// 添加轮播
        /// </summary>
        /// <param name="banner"></param>
        /// <returns></returns>
        Task<MyResult<Object>> AddBanner(SysBanner banner);

         /// <summary>
         /// 修改轮播
         /// </summary>
         /// <param name="banner"></param>
         /// <returns></returns>
        Task<MyResult<Object>> ModifyBanner(SysBanner banner);
        #endregion
    }
}