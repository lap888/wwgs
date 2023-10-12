using Gs.Core;
using Gs.Domain.Context;
using Gs.Domain.Entity;
using Gs.Domain.Enums;
using Gs.Domain.Models;
using Gs.Domain.Models.City;
using Gs.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Gs.Application.Services
{
    /// <summary>
    /// 城市合伙人
    /// </summary>
    public class CityService : ICityService
    {
        private readonly WwgsContext context;

        public CityService(WwgsContext mySql)
        {
            context = mySql;
        }

        /// <summary>
        /// 添加城市合伙人
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> Add(PartnerInfo city)
        {
            MyResult<Object> result = new MyResult<Object>();
            CityMaster model = new CityMaster()
            {
                CityId = city.CityId,
                CityCode = city.CityCode,
                CityName = city.CityName,
                AreaCode = city.AreaCode,
                AreaName = city.AreaName,
                StartDate = city.StartDate,
                EndDate = city.EndDate,
                Mobile = city.Mobile,
                WeChat = city.WeChat,
                Remark = city.Remark
            };
            try
            {
                if (string.IsNullOrWhiteSpace(city.Mobile)) { return result.SetStatus(ErrorCode.InvalidData, "会员手机号必须添写"); }
                model.UserId = await context.UserEntity.Where(item => item.Mobile == city.Mobile).Select(item => item.Id).FirstOrDefaultAsync();
                if (model.UserId < 1) { return result.SetStatus(ErrorCode.InvalidData, "会员手机号不有误"); }
                model.CreateTime = DateTime.Now;
                context.CityMaster.Add(model);
                if (context.SaveChanges() > 0) { return result; }
            }
            catch (Exception ex)
            {
                SystemLog.Debug("添加社区" + city.ToJson(), ex);
            }
            return result.SetStatus(ErrorCode.InvalidData, "操作失败");
        }

        /// <summary>
        /// 修改城市合伙人
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> Edit(PartnerInfo city)
        {
            MyResult<Object> result = new MyResult<Object>();
            var model = await context.CityMaster.FirstOrDefaultAsync(item => item.CityId == city.CityId);
            if (model == null) { return result.SetStatus(ErrorCode.InvalidData, "信息不存在"); }
            model.UserId = city.UserId;
            model.CityCode = city.CityCode;
            model.CityName = city.CityName;
            model.AreaCode = city.AreaCode;
            model.AreaName = city.AreaName;
            model.StartDate = city.StartDate;
            model.EndDate = city.EndDate;
            model.WeChat = city.WeChat;
            model.Remark = city.Remark;
            if (string.IsNullOrWhiteSpace(city.Mobile)) { return result.SetStatus(ErrorCode.InvalidData, "会员手机号必须添写"); }
            model.UserId = await context.UserEntity.Where(item => item.Mobile == city.Mobile).Select(item => item.Id).FirstOrDefaultAsync();
            if (model.UserId < 1) { return result.SetStatus(ErrorCode.InvalidData, "会员手机号不有误"); }
            context.Update(model);
            if (context.SaveChanges() < 1) { return result.SetStatus(ErrorCode.InvalidData, "操作失败"); }
            return result;
        }

        /// <summary>
        /// 修改城市合伙人
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        public async Task<MyResult<List<PartnerInfo>>> List(QueryModel query)
        {
            MyResult<List<PartnerInfo>> result = new MyResult<List<PartnerInfo>>();
            Expression<Func<CityMaster, bool>> where = null;
            if (query.Id == 1)
            {
                where = where.AndAlso(item => item.AreaCode == "0000");
            }
            else { where = where.AndAlso(item => item.CityCode == "0000"); }
            if (!String.IsNullOrWhiteSpace(query.Mobile))
            {
                query.UserId = context.UserEntity.Where(item => item.Mobile == query.Mobile).Select(item => item.Id).FirstOrDefault();
            }
            if (query.UserId > 0) { where = where.AndAlso(item => item.UserId == query.UserId); }
            result.RecordCount = await context.CityMaster.Where(where).CountAsync();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            result.Data = await context.CityMaster.Where(where).OrderByDescending(item => item.CityId)
                .Skip(query.PageSize * (query.PageIndex - 1)).Take(query.PageSize)
                .Join(context.UserEntity, c => c.UserId, u => u.Id, (c, u) => new PartnerInfo()
                {
                    CityId = c.CityId,
                    Nick = u.Name,
                    Mobile = u.Mobile,
                    UserId = c.UserId,
                    CityCode = c.CityCode,
                    AreaCode = c.AreaCode,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    WeChat = c.WeChat,
                    Remark = c.Remark,
                    AreaName = c.AreaName,
                    CityName = c.CityName,
                    CreateTime = c.CreateTime,

                }).ToListAsync();
            return result;
        }
    }
}
