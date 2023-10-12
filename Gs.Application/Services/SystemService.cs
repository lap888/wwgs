using CSRedis;
using Dapper;
using Gs.Core;
using Gs.Domain.Configs;
using Gs.Domain.Context;
using Gs.Domain.Entity;
using Gs.Domain.Enums;
using Gs.Domain.Models;
using Gs.Domain.Models.Admin;
using Gs.Domain.Models.Dto;
using Gs.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gs.Application.Services
{
    public class SystemService : ISystemService
    {
        private readonly WwgsContext context;
        private readonly CSRedisClient RedisCache;
        private readonly Models.AppSetting AppSettings;
        private readonly IIntegralService IntegralSub;
        private readonly ICottonService CottonSub;
        private readonly IConchService ConchSub;
        private const String HtmlStr = @"<!DOCTYPE html><html><head><meta charset=""utf-8""><meta http-equiv=""x-dns-prefetch-control"" content=""on"" /><meta name=""apple-mobile-web-app-capable"" content=""yes"" /><meta content = ""telephone=no"" name=""format-detection"" /><meta name = ""viewport""content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, minimum-scale=1.0, user-scalable=0""><title></title></head><body>{0}</body></html>";

        public SystemService(WwgsContext mySql, IConchService conchSub, CSRedisClient redisClient, ICottonService cottonService, IIntegralService integralService, IOptionsMonitor<Models.AppSetting> monitor)
        {
            context = mySql;
            RedisCache = redisClient;
            AppSettings = monitor.CurrentValue;
            CottonSub = cottonService;
            IntegralSub = integralService;
            ConchSub = conchSub;
        }

        /// <summary>
        /// APP ���ص�ַ
        /// </summary>
        /// <returns></returns>
        public MyResult<object> AppDownloadUrl()
        {
            MyResult result = new MyResult();
            var androidClientVersion = context.Dapper.QueryFirstOrDefault<SysClientVersions>("SELECT * FROM `sys_client_versions` WHERE `deviceSystem` = 'android' ORDER BY id DESC LIMIT 1;");
            var iosClientVersion = context.Dapper.QueryFirstOrDefault<SysClientVersions>("SELECT * FROM `sys_client_versions` WHERE `deviceSystem` = 'ios' ORDER BY id DESC LIMIT 1;");
            result.Data = new { ios = iosClientVersion.DownloadUrl, android = androidClientVersion.DownloadUrl };
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> ClientDownloadUrl(string systemName)
        {
            MyResult result = new MyResult();
            if (string.IsNullOrEmpty(systemName))
            {
                return result.SetStatus(ErrorCode.InvalidData, "ϵͳ�豸�쳣");
            }
            var sysClientVersion = await context.Dapper.QueryFirstOrDefaultAsync<SysClientVersions>($"select * from `sys_client_versions` where `deviceSystem`=@systemName order by id desc limit 1", new { systemName });
            result.Data = sysClientVersion;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> CopyWriting(string type)
        {
            MyResult<object> result = new MyResult<object> { Data = new List<CopyWriting>() };
            if (String.IsNullOrWhiteSpace(type)) { return result; }
            string CacheKey = $"System:Copys_{type}";
            if (RedisCache.Exists(CacheKey))
            {
                if (type.Equals("real_name_rule", StringComparison.OrdinalIgnoreCase))
                {
                    result.Data = RedisCache.Get<List<CopyWriting>>(CacheKey).FirstOrDefault()?.Text;
                    return result;
                }
                result.Data = RedisCache.Get<List<CopyWriting>>(CacheKey);
                return result;
            }
            List<CopyWriting> dbdata = (await context.Dapper.QueryAsync<CopyWriting>("SELECT `key`,`title`,`text` FROM `system_copywriting` WHERE `type`=@Type ORDER BY `key`", new { Type = type })).ToList();
            if (dbdata.Count > 0) { RedisCache.Set(CacheKey, dbdata, 120 * 60); }
            if (type.Equals("real_name_rule", StringComparison.OrdinalIgnoreCase))
            {
                result.Data = dbdata.FirstOrDefault()?.Text;
                return result;
            }
            result.Data = dbdata;
            return result;
        }

        /// <summary>
        /// ϵͳ��Ϣ
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<object> Notices(NoticesDto model, int userId)
        {
            model.PageIndex = model.PageIndex < 1 ? 1 : model.PageIndex;
            MyResult result = new MyResult();
            StringBuilder QuerySql = new StringBuilder();
            StringBuilder CuntSql = new StringBuilder();
            CuntSql.Append("SELECT COUNT(id) FROM notice_infos WHERE `isDel` = 0 AND `type` = @Type ");
            DynamicParameters SqlParam = new DynamicParameters();
            QuerySql.Append("SELECT * FROM notice_infos WHERE `isDel` = 0 AND `type` = @Type ");
            SqlParam.Add("Type", model.Type, DbType.Int32);
            if (model.Type == 1)
            {
                CuntSql.Append("AND `userId` = @UserId ");
                QuerySql.Append("AND `userId` = @UserId ");
                SqlParam.Add("UserId", userId, DbType.Int64);
            }
            QuerySql.Append("ORDER BY id DESC LIMIT @PageIndex, @PageSize;");
            SqlParam.Add("PageIndex", (model.PageIndex - 1) * model.PageSize, DbType.Int32);
            SqlParam.Add("PageSize", model.PageSize, DbType.Int32);

            result.RecordCount = context.Dapper.QueryFirstOrDefault<Int32?>(CuntSql.ToString(), SqlParam) ?? 0;
            result.PageCount = (result.RecordCount + model.PageSize - 1) / model.PageSize;
            result.Data = context.Dapper.Query<NoticeInfos>(QuerySql.ToString(), SqlParam);
            return result;
        }

        /// <summary>
        /// �ֲ����
        /// </summary>
        /// <param name="source"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public MyResult<object> Banners(int source, long uid = 0)
        {
            MyResult result = new MyResult();
            StringBuilder QuerySql = new StringBuilder();
            DynamicParameters QueryParam = new DynamicParameters();

            QuerySql.Append("SELECT ");
            QuerySql.Append("`id` AS `Id`, ");
            QuerySql.Append("`queue` AS `Queue`, ");
            QuerySql.Append("`title` AS `Title`, ");
            QuerySql.Append("CONCAT(@CosUrl,`imageUrl`) AS `ImageUrl`, ");
            QuerySql.Append("`type` AS `Type`, ");
            QuerySql.Append("`source` AS `Source`, ");
            QuerySql.Append("`status` AS `Status`, ");
            QuerySql.Append("`params` AS `Params`, ");
            QuerySql.Append("`cityCode` AS `CityCode`, ");
            QuerySql.Append("`createdAt` AS `CreatedAt` ");
            QuerySql.Append("FROM ");
            QuerySql.Append("sys_banner ");
            QuerySql.Append("WHERE source = @Source AND `status` = 1 ORDER BY `queue` ASC;");

            QueryParam.Add("CosUrl", Constants.CosUrl, DbType.String);
            QueryParam.Add("Source", source, DbType.Int32);

            List<SysBanner> bannerList = context.Dapper.Query<SysBanner>(QuerySql.ToString(), QueryParam).ToList();
            if (uid > 0 && source == 2)
            {
                String CityNo = context.Dapper.QueryFirstOrDefault<String>("SELECT `cityCode` FROM `user_locations` WHERE `userId` = @UserId;", new { UserId = uid });
                List<SysBanner> rult = new List<SysBanner>();
                if (!String.IsNullOrWhiteSpace(CityNo))
                {
                    rult = bannerList.Where(item => item.CityCode == CityNo).ToList();
                }
                if (rult.Count() > 0)
                {
                    result.Data = rult;
                    return result;
                }
            }
            result.Data = bannerList.Where(item => String.IsNullOrWhiteSpace(item.CityCode)).ToList();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MyResult<object> OneNotice()
        {
            MyResult result = new MyResult();
            var noticeSql = $"SELECT * FROM notice_infos WHERE type=0 order by id desc limit 1";
            var notice = context.Dapper.QueryFirstOrDefault<NoticeInfos>(noticeSql);
            result.Data = notice;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<MyResult<UserReturnDto>> UserInfo(int userId)
        {
            MyResult<UserReturnDto> result = new MyResult<UserReturnDto>();
            if (userId <= 0)
            {
                result.Data = new UserReturnDto();
                return result;
            }
            var userInfoSql = $"select uubg.*,IFNULL(og.`status`,0) isPay, IFNULL( og.uuid, 0 ) totalWatch from (select uub.* from (select u.id, u.`uuid`, u.`alipay`,u.`level`,u.`rcode`,u.inviterMobile,u.`status`,u.`auditState`,u.`golds`,u.mobile,u.name,u.alipayUid from `user` u where u.id={userId}) uub) uubg left join `order_games` og on uubg.id=og.userId";
            var UserInfo = context.Dapper.QueryFirstOrDefault<UserReturnDto>(userInfoSql);
            if (UserInfo == null)
            {
                result.Data = new UserReturnDto();
            }
            else
            {
                #region 
                UserInfo.UserBalanceNormal = context.Dapper.QueryFirstOrDefault<Decimal>("SELECT Balance FROM user_account_wallet WHERE UserId = @UserId;", new { UserId = userId });

                UserInfo.IsPay = UserInfo.IsPay == 1 ? 1 : 0;
                UserInfo.Rcode = UserInfo.Rcode == null ? "0" : UserInfo.Rcode;

                var cotton = await ConchSub.Info(userId);
                var interalSub = await IntegralSub.Info(userId);

                UserInfo.Cotton = cotton.Balance;
                UserInfo.CanUserCoin = cotton.Usable;
                UserInfo.BalanceUserCoin = cotton.Balance;
                UserInfo.FrozenUserCoin = cotton.Frozen;
                UserInfo.InteralSub = interalSub.Balance;

                UserInfo.DayNum = (string)(await ConchSub.TodayOutNum(userId)).Data;
                #endregion
                UserInfo.IsStore = context.CommunityCenter.Where(item => item.UserId == userId).FirstOrDefault() != null;
                if (!String.IsNullOrWhiteSpace(UserInfo.Level)) { UserInfo.Level = UserInfo.Level.ToUpper(); }

                result.Data = UserInfo;
            }
            return result;
        }

        /// <summary>
        /// APP��Ϣ
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<NoticeInfos>>> AppNotice(QueryModel query)
        {
            MyResult<List<NoticeInfos>> result = new MyResult<List<NoticeInfos>>();
            Expression<Func<NoticeInfos, bool>> where = null;
            where = where.AndAlso(item => item.IsDel == false && item.Source == "app");
            if (query.Id > 0) { where = where.AndAlso(item => item.Type == query.Id.ToString()); }

            result.RecordCount = await context.NoticeInfos.Where(where).CountAsync();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            List<NoticeInfos> Notices = await context.NoticeInfos.Where(where).OrderByDescending(item => item.Id)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToListAsync();
            result.Data = new List<NoticeInfos>();

            foreach (var item in Notices)
            {
                item.Content = String.Format(HtmlStr, item.Content);
                result.Data.Add(item);
            }

            return result;
        }

        /// <summary>
        /// APP�ֲ�
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<SysBanner>>> AppBanner(QueryModel query)
        {
            MyResult<List<SysBanner>> result = new MyResult<List<SysBanner>>();
            Expression<Func<SysBanner, bool>> where = null;
            where = where.AndAlso(item => item.IsDel == false);
            if (query.Id > 0) { where = where.AndAlso(item => item.Type == query.Id); }
            where = where.AndAlso(item => item.Source == "app");
            result.RecordCount = await context.SysBanner.Where(where).CountAsync();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            var banners = await context.SysBanner.Where(where).OrderByDescending(item => item.Id)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToListAsync();
            result.Data = new List<SysBanner>();
            foreach (var item in banners)
            {
                var model = item;
                model.ImageUrl = AppSettings.QCloudUrl + item.ImageUrl;
                if (item.Type == 1)
                {
                    model.Params = String.Format(HtmlStr, model.Params);
                }
                result.Data.Add(model);
            }

            return result;
        }

        /// <summary>
        /// ��������Ϣ
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<NoticeInfos>>> ExchangeNotice(QueryModel query)
        {
            MyResult<List<NoticeInfos>> result = new MyResult<List<NoticeInfos>>();
            Expression<Func<NoticeInfos, bool>> where = null;
            where = where.AndAlso(item => item.IsDel == false && item.Source == "exchange");
            if (query.Id > 0) { where = where.AndAlso(item => item.Type == query.Id.ToString()); }

            result.RecordCount = await context.NoticeInfos.Where(where).CountAsync();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            result.Data = await context.NoticeInfos.Where(where).OrderByDescending(item => item.Id)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

            return result;
        }

        /// <summary>
        /// �������ֲ�
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<SysBanner>>> ExchangeBanner(QueryModel query)
        {
            MyResult<List<SysBanner>> result = new MyResult<List<SysBanner>>();
            Expression<Func<SysBanner, bool>> where = null;
            where = where.AndAlso(item => item.IsDel == false);
            if (query.Id > 0) { where = where.AndAlso(item => item.Type == query.Id); }
            where = where.AndAlso(item => item.Source == "exchange");
            result.RecordCount = await context.SysBanner.Where(where).CountAsync();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            result.Data = await context.SysBanner.Where(where).OrderByDescending(item => item.Id)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

            return result;
        }

        /// <summary>
        /// ��Ա����
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> Feedback(UserFeedback feedback)
        {
            MyResult<Object> result = new MyResult<Object>();
            try
            {
                feedback.CreateTime = DateTime.Now;
                context.Add(feedback);
                if (await context.SaveChangesAsync() > 0)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                SystemLog.Debug("��Ա����", ex);
            }
            return result.SetStatus(ErrorCode.InvalidData, "�ύʧ��");
        }

        #region ��̨����
        /// <summary>
        /// �����б�
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<AdminFeedback>>> UserFeedback(QueryModel query)
        {
            MyResult<List<AdminFeedback>> result = new MyResult<List<AdminFeedback>>();
            Expression<Func<UserFeedback, bool>> where = null;
            where = where.AndAlso(item => item.UserId > 0);
            if (query.Id > 0) { where = where.AndAlso(item => item.State == query.Id); }

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                where = where.AndAlso(item => item.Content.Contains(query.Keyword));
            }

            result.RecordCount = await context.UserFeedback.Where(where).CountAsync();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            result.Data = await context.UserFeedback.Where(where).OrderByDescending(item => item.Id)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize)
                .Join(context.UserEntity, feed => feed.UserId, user => user.Id, (feed, user) => new AdminFeedback()
                {
                    Id = feed.Id,
                    UserId = feed.UserId,
                    Nick = user.Name,
                    Mobile = user.Mobile,
                    Title = feed.Title,
                    Content = feed.Content,
                    Images = feed.Images,
                    State = feed.State,
                    Remark = feed.Remark,
                    CreateTime = feed.CreateTime,
                }).ToListAsync();

            return result;
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> HandleFeedback(AdminFeedback feedback)
        {
            MyResult<Object> result = new MyResult<Object>();
            var model = await context.UserFeedback.Where(item => item.Id == feedback.Id).FirstOrDefaultAsync();
            if (model == null) { return result.SetStatus(ErrorCode.InvalidData, "����ʧ��"); }
            model.Remark = feedback.Remark;
            model.State = 2;
            context.Update(model);
            if (context.SaveChanges() > 0) { return result; }
            return result.SetStatus(ErrorCode.InvalidData, "����ʧ��");
        }

        /// <summary>
        /// ��Ϣ�б�
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<NoticeInfos>>> NoticeList(QueryModel query)
        {
            MyResult<List<NoticeInfos>> result = new MyResult<List<NoticeInfos>>();
            Expression<Func<NoticeInfos, bool>> where = null;
            where = where.AndAlso(item => item.IsDel == false && item.Source == query.Mobile);
            if (query.Id > 0) { where = where.AndAlso(item => item.Type == query.Id.ToString()); }

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                where = where.AndAlso(item => item.Content.Contains(query.Keyword));
            }

            result.RecordCount = await context.NoticeInfos.Where(where).CountAsync();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            result.Data = await context.NoticeInfos.Where(where).OrderByDescending(item => item.Id)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

            return result;
        }

        /// <summary>
        /// ������Ϣ
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> AddNotice(NoticeInfos infos)
        {
            MyResult<Object> result = new MyResult<Object>();
            try
            {
                infos.CeratedAt = DateTime.Now;
                context.NoticeInfos.Add(infos);
                if (await context.SaveChangesAsync() > 0) { return result; }
            }
            catch (Exception ex)
            {
                SystemLog.Debug("������Ϣ" + infos.ToJson(), ex);
            }
            return result.SetStatus(ErrorCode.InvalidData, "����ʧ��");
        }

        /// <summary>
        /// �޸���Ϣ
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> ModifyNotice(NoticeInfos infos)
        {
            MyResult<Object> result = new MyResult<Object>();
            var model = await context.NoticeInfos.FirstOrDefaultAsync(item => item.Id == infos.Id);
            if (model == null) { return result.SetStatus(ErrorCode.InvalidData, "��Ϣ������"); }
            model.UserId = infos.UserId;
            model.Title = infos.Title;
            model.Content = infos.Content;
            model.Type = infos.Type;
            model.IsDel = infos.IsDel;
            model.UpdatedAt = DateTime.Now;
            context.Update(model);
            if (context.SaveChanges() < 1) { return result.SetStatus(ErrorCode.InvalidData, "����ʧ��"); }
            return result;
        }

        /// <summary>
        /// �ֲ��б�
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<SysBanner>>> BannerList(QueryModel query)
        {
            MyResult<List<SysBanner>> result = new MyResult<List<SysBanner>>();
            Expression<Func<SysBanner, bool>> where = null;
            where = where.AndAlso(item => item.IsDel == false);
            if (query.Id > 0) { where = where.AndAlso(item => item.Type == query.Id); }
            where = where.AndAlso(item => item.Source == query.Mobile);
            result.RecordCount = await context.SysBanner.Where(where).CountAsync();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            result.Data = await context.SysBanner.Where(where).OrderByDescending(item => item.Id)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

            return result;
        }

        /// <summary>
        /// �����ֲ�
        /// </summary>
        /// <param name="banner"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> AddBanner(SysBanner banner)
        {
            MyResult<Object> result = new MyResult<Object>();
            try
            {
                banner.CreatedAt = DateTime.Now;
                context.SysBanner.Add(banner);
                if (await context.SaveChangesAsync() > 0) { return result; }
            }
            catch (Exception ex)
            {
                SystemLog.Debug("�����ֲ�" + banner.ToJson(), ex);
            }
            return result.SetStatus(ErrorCode.InvalidData, "����ʧ��");

        }

        /// <summary>
        /// �޸��ֲ�
        /// </summary>
        /// <param name="banner"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> ModifyBanner(SysBanner banner)
        {
            MyResult<Object> result = new MyResult<Object>();
            var model = await context.SysBanner.FirstOrDefaultAsync(item => item.Id == banner.Id);
            if (model == null) { return result.SetStatus(ErrorCode.InvalidData, "��Ϣ������"); }
            model.ImageUrl = banner.ImageUrl;
            model.Title = banner.Title;
            model.Queue = banner.Queue;
            model.Source = banner.Source;
            model.Params = banner.Params;
            model.Type = banner.Type;
            model.IsDel = banner.IsDel;
            model.Remark = banner.Remark;
            context.Update(model);
            if (context.SaveChanges() < 1) { return result.SetStatus(ErrorCode.InvalidData, "����ʧ��"); }
            return result;
        }

        #endregion
    }
}