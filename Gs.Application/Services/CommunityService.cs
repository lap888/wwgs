using Gs.Core;
using Gs.Domain.Context;
using Gs.Domain.Entity;
using Gs.Domain.Enums;
using Gs.Domain.Models;
using Gs.Domain.Models.BuyBack;
using Gs.Domain.Models.Community;
using Gs.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Gs.Application.Services
{
    /// <summary>
    /// 社区运营中心
    /// </summary>
    public class CommunityService : ICommunityService
    {
        private readonly WwgsContext context;
        private readonly IIntegralService IntegralSub;
        private readonly Models.AppSetting AppSettings;
        public CommunityService(WwgsContext mySql, IIntegralService integralService, IOptionsMonitor<Models.AppSetting> monitor)
        {
            context = mySql;
            IntegralSub = integralService;
            AppSettings = monitor.CurrentValue;
        }

        #region 后台管理
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="community"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> Add(CommunityInfo community)
        {
            MyResult<Object> result = new MyResult<Object>();
            CommunityCenter model = new CommunityCenter()
            {
                Id = community.Id,
                UserId = community.UserId,
                CityCode = community.CityCode,
                AreaCode = community.AreaCode,
                Address = community.Address,
                Company = community.Company,
                Lat = community.Lat,
                Lng = community.Lng,
                Contacts = community.Contacts,
                ContactTel = community.ContactTel,
                Describe = community.Describe,
                Doorhead = community.Doorhead,
                StartDate = community.StartDate,
                EndDate = community.EndDate,
                Website = community.Website,
                WeChat = community.WeChat,
                IsDel = community.IsDel,
                Remark = community.Remark,
                Qq = community.Qq,
                CreateTime = community.CreateTime,
            };
            try
            {
                if (string.IsNullOrWhiteSpace(community.Mobile)) { return result.SetStatus(ErrorCode.InvalidData, "会员手机号必须添写"); }
                model.UserId = await context.UserEntity.Where(item => item.Mobile == community.Mobile).Select(item => item.Id).FirstOrDefaultAsync();
                if (model.UserId < 1) { return result.SetStatus(ErrorCode.InvalidData, "会员手机号不有误"); }
                model.CreateTime = DateTime.Now;
                context.CommunityCenter.Add(model);
                if (context.SaveChanges() > 0) { return result; }
            }
            catch (Exception ex)
            {
                SystemLog.Debug("添加社区" + community.ToJson(), ex);
            }
            return result.SetStatus(ErrorCode.InvalidData, "操作失败");

        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="community"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> Modify(CommunityInfo community)
        {
            MyResult<Object> result = new MyResult<Object>();
            var model = await context.CommunityCenter.FirstOrDefaultAsync(item => item.Id == community.Id && item.IsDel == 0);
            if (model == null) { return result.SetStatus(ErrorCode.InvalidData, "信息不存在"); }
            model.UserId = community.UserId;
            model.CityCode = community.CityCode;
            model.AreaCode = community.AreaCode;
            model.Address = community.Address;
            model.Company = community.Company;
            model.Lat = community.Lat;
            model.Lng = community.Lng;
            model.Contacts = community.Contacts;
            model.ContactTel = community.ContactTel;
            model.Describe = community.Describe;
            model.Doorhead = community.Doorhead;
            model.StartDate = community.StartDate;
            model.EndDate = community.EndDate;
            model.Website = community.Website;
            model.WeChat = community.WeChat;
            model.IsDel = community.IsDel;
            model.Remark = community.Remark;
            model.Qq = community.Qq;
            if (string.IsNullOrWhiteSpace(community.Mobile)) { return result.SetStatus(ErrorCode.InvalidData, "会员手机号必须添写"); }
            model.UserId = await context.UserEntity.Where(item => item.Mobile == community.Mobile).Select(item => item.Id).FirstOrDefaultAsync();
            if (model.UserId < 1) { return result.SetStatus(ErrorCode.InvalidData, "会员手机号不有误"); }
            context.Update(model);
            if (context.SaveChanges() < 1) { return result.SetStatus(ErrorCode.InvalidData, "操作失败"); }
            return result;
        }

        /// <summary>
        /// 中心列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<CommunityInfo>>> List(QueryModel query)
        {
            MyResult<List<CommunityInfo>> result = new MyResult<List<CommunityInfo>>();
            Expression<Func<CommunityCenter, bool>> where = null;
            where = where.AndAlso(item => item.IsDel == 0);
            if (query.Id > 0) { where = where.AndAlso(item => item.Id == query.Id); }
            if (!String.IsNullOrWhiteSpace(query.Mobile))
            {
                query.UserId = context.UserEntity.Where(item => item.Mobile == query.Mobile).Select(item => item.Id).FirstOrDefault();
            }
            if (query.UserId > 0) { where = where.AndAlso(item => item.UserId == query.UserId); }
            if (!string.IsNullOrWhiteSpace(query.Keyword)) { where = where.AndAlso(item => item.Company.Contains(query.Keyword)); }
            result.RecordCount = await context.CommunityCenter.Where(where).CountAsync();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            result.Data = await context.CommunityCenter.Where(where).OrderByDescending(item => item.Id)
                .Skip(query.PageSize * (query.PageIndex - 1)).Take(query.PageSize)
                .Join(context.UserEntity, c => c.UserId, u => u.Id, (c, u) => new CommunityInfo()
                {
                    Id = c.Id,
                    Nick = u.Name,
                    Mobile = u.Mobile,
                    UserId = c.UserId,
                    CityCode = c.CityCode,
                    AreaCode = c.AreaCode,
                    Address = c.Address,
                    Company = c.Company,
                    Lat = c.Lat,
                    Lng = c.Lng,
                    Contacts = c.Contacts,
                    ContactTel = c.ContactTel,
                    Describe = c.Describe,
                    Doorhead = c.Doorhead,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Website = c.Website,
                    WeChat = c.WeChat,
                    IsDel = c.IsDel,
                    Remark = c.Remark,
                    Qq = c.Qq,
                    CreateTime = c.CreateTime,

                }).ToListAsync();
            return result;
        }

        /// <summary>
        /// 订单列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<AssessOrder>>> BackOrder(QueryModel query)
        {
            MyResult<List<AssessOrder>> result = new MyResult<List<AssessOrder>>();
            Expression<Func<CommunityBackOrder, bool>> where = null;
            if (query.Id >= 0)
            {
                where = where.AndAlso(item => item.State == (BackStauts)query.Id);
            }
            else { where = where.AndAlso(item => item.Id > 0); }
            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                List<Int64> StoreIds = context.CommunityCenter.Where(item => item.Company.Contains(query.Keyword))
                    .Select(item => item.Id).ToList();
                where = where.AndAlso(item => StoreIds.Contains(item.StoreId));
            }

            result.RecordCount = await context.CommunityBackOrder.Where(where).CountAsync();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            result.Data = await context.CommunityBackOrder.Where(where).OrderByDescending(item => item.Id)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize)
                .Join(context.CommunityCenter, order => order.StoreId, store => store.Id, (order, store) => new
                {
                    Order = order,
                    Store = store
                }).Join(context.UserEntity, obj => obj.Order.UserId, user => user.Id, (obj, user) => new AssessOrder()
                {
                    Id = obj.Order.Id,
                    UserId = obj.Order.UserId,
                    Nick = user.Name,
                    StoreId = obj.Order.StoreId,
                    StoreName = obj.Store.Company,
                    AssessIntegral = obj.Order.AssessIntegral,
                    UnitPrice = obj.Order.UnitPrice,
                    RepoType = obj.Order.RepoType,
                    ShipMethod = obj.Order.ShipMethod,
                    Condition = obj.Order.Condition,
                    ItemBrand = obj.Order.ItemBrand,
                    ItemCunt = obj.Order.ItemCunt,
                    ItemGrade = obj.Order.ItemGrade,
                    State = obj.Order.State,
                    CreateTime = obj.Order.CreateTime,
                    Remark = obj.Order.Remark
                }).OrderByDescending(item => item.Id).ToListAsync();

            return result;
        }

        /// <summary>
        /// 发放积分
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> Distribute(CommunityBackOrder order)
        {
            MyResult<object> result = new MyResult<object>();
            var model = await context.CommunityBackOrder.FirstOrDefaultAsync(item => item.Id == order.Id);
            if (model == null) { return result.SetStatus(ErrorCode.InvalidData, "信息不存在"); }
            if (model.State != BackStauts.Sending) { return result.SetStatus(ErrorCode.InvalidData, "订单已处理"); }
            model.AssessIntegral = order.AssessIntegral;
            model.State = BackStauts.Completed;
            context.CommunityBackOrder.Update(model);
            if (context.SaveChanges() < 1) { return result.SetStatus(ErrorCode.InvalidData, "操作失败"); }
            if (!await IntegralSub.ChangeAmount(model.UserId, model.AssessIntegral, IntegralModifyType.OLD_ASSESS, false, order.RepoType.GetDescription()))
            {
                SystemLog.Warn("收货赠送积分失败:" + model.ToJson());
            }
            return result;
        }


        #endregion

        #region 社区管理

        /// <summary>
        /// 社区评估单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<AssessOrder>>> StoreOrder(QueryModel query)
        {
            MyResult<List<AssessOrder>> result = new MyResult<List<AssessOrder>>();
            Expression<Func<CommunityBackOrder, bool>> where = null;
            if (query.UserId < 1) { return result.SetStatus(ErrorCode.ReLogin); }
            CommunityCenter StoreInfo = await context.CommunityCenter.Where(item => item.UserId == query.UserId).FirstOrDefaultAsync();
            if (StoreInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "您不社区管理员"); }
            where = where.AndAlso(item => item.StoreId == StoreInfo.Id);

            result.RecordCount = context.CommunityBackOrder.Where(where).Count();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            result.Data = await context.CommunityBackOrder.Where(where).OrderByDescending(item => item.Id)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize)
                .Join(context.UserEntity, order => order.UserId, user => user.Id, (order, user) => new AssessOrder()
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    Nick = user.Name,
                    StoreId = order.StoreId,
                    StorePic = AppSettings.QCloudUrl + user.AvatarUrl,
                    StoreName = StoreInfo.Company,
                    StoreAddress = StoreInfo.Address,
                    AssessIntegral = order.AssessIntegral,
                    UnitPrice = order.UnitPrice,
                    RepoType = order.RepoType,
                    ShipMethod = order.ShipMethod,
                    Condition = order.Condition,
                    ItemBrand = order.ItemBrand,
                    ItemCunt = order.ItemCunt,
                    ItemGrade = order.ItemGrade,
                    State = order.State,
                    CreateTime = order.CreateTime,
                    Remark = order.Remark
                }).OrderByDescending(item => item.Id).ToListAsync();

            return result;

        }

        /// <summary>
        /// 社区发放积分
        /// </summary>
        /// <param name="OrderId"></param>
        /// <param name="Integral"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> StoreDistribute(Int64 OrderId, Int32 Integral, Int64 UserId)
        {
            MyResult<object> result = new MyResult<object>();
            if (Integral < 1) { return result.SetStatus(ErrorCode.InvalidData, "积分数量不对"); }
            var model = await context.CommunityBackOrder.FirstOrDefaultAsync(item => item.Id == OrderId);
            if (model == null) { return result.SetStatus(ErrorCode.InvalidData, "信息不存在"); }
            var StoreId = context.CommunityCenter.Where(item => item.UserId == UserId && item.IsDel == 0).Select(item => item.Id).FirstOrDefault();
            if (model.State != BackStauts.Sending) { return result.SetStatus(ErrorCode.InvalidData, "订单已处理"); }
            if (model.StoreId != StoreId) { return result.SetStatus(ErrorCode.InvalidData, "此订单您无权处理"); }
            model.AssessIntegral = Integral;
            model.State = BackStauts.Completed;
            context.CommunityBackOrder.Update(model);
            if (context.SaveChanges() < 1) { return result.SetStatus(ErrorCode.InvalidData, "操作失败"); }
            if (!await IntegralSub.ChangeAmount(model.UserId, model.AssessIntegral, IntegralModifyType.OLD_ASSESS, false, model.RepoType.GetDescription()))
            {
                SystemLog.Warn("收货赠送积分失败:" + model.ToJson());
            }
            return result;
        }



        #endregion

        /// <summary>
        /// 中心列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<CommunityInfo>>> AppList(QueryModel query)
        {
            MyResult<List<CommunityInfo>> result = new MyResult<List<CommunityInfo>>();
            Expression<Func<CommunityCenter, bool>> where = null;
            where = where.AndAlso(item => item.IsDel == 0);
            if (query.Id > 0) { where = where.AndAlso(item => item.Id == query.Id); }
            if (!string.IsNullOrWhiteSpace(query.Keyword)) { where = where.AndAlso(item => item.Company.Contains(query.Keyword)); }
            result.RecordCount = await context.CommunityCenter.Where(where).CountAsync();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            result.Data = await context.CommunityCenter.Where(where).OrderByDescending(item => item.Id)
                .Skip(query.PageSize * (query.PageIndex - 1)).Take(query.PageSize)
                .Join(context.UserEntity, c => c.UserId, u => u.Id, (c, u) => new CommunityInfo()
                {
                    Id = c.Id,
                    Nick = u.Name,
                    Mobile = u.Mobile,
                    UserId = c.UserId,
                    CityCode = c.CityCode,
                    AreaCode = c.AreaCode,
                    Address = c.Address,
                    Company = c.Company,
                    Lat = c.Lat,
                    Lng = c.Lng,
                    Contacts = c.Contacts,
                    ContactTel = c.ContactTel,
                    Describe = c.Describe,
                    Doorhead = AppSettings.QCloudUrl + c.Doorhead,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Website = c.Website,
                    WeChat = c.WeChat,
                    IsDel = c.IsDel,
                    Remark = c.Remark,
                    Qq = c.Qq,
                    CreateTime = c.CreateTime,
                }).ToListAsync();
            foreach (var item in result.Data)
            {
                foreach (var img in item.Describe.Split(","))
                {
                    item.ListImgs = new List<string>();
                    item.ListImgs.Add(AppSettings.QCloudUrl + img);
                }
            }
            return result;
        }


        /// <summary>
        /// 确认收货
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> Received(CommunityBackOrder order)
        {
            MyResult<object> result = new MyResult<object>();
            var model = await context.CommunityBackOrder.FirstOrDefaultAsync(item => item.Id == order.Id);
            if (model == null) { return result.SetStatus(ErrorCode.InvalidData, "信息不存在"); }
            model.State = BackStauts.Completed;
            context.CommunityBackOrder.Update(model);
            if (context.SaveChanges() < 1) { return result.SetStatus(ErrorCode.InvalidData, "操作失败"); }
            if (!await IntegralSub.ChangeAmount(model.UserId, model.AssessIntegral, IntegralModifyType.OLD_ASSESS, false, order.RepoType.GetDescription()))
            {
                SystemLog.Warn("收货赠送积分失败:" + model.ToJson());
            }
            return result;
        }

        /// <summary>
        /// 评估
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> Assess(BuyBackModel model)
        {
            MyResult<Object> result = new MyResult<object>();
            result.Data = await AssessItem(model);
            return result;
        }

        /// <summary>
        /// 邮寄
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> SendPost(BuyBackModel model)
        {
            MyResult<object> result = new MyResult<object>();
            model = await AssessItem(model);
            try
            {
                CommunityBackOrder BackOrder = new CommunityBackOrder()
                {
                    StoreId = model.StoreId,
                    UserId = model.UserId,
                    RepoType = model.Type,
                    ItemCunt = model.Count,
                    ItemBrand = model.Brand,
                    ItemGrade = model.Grade,
                    ShipMethod = model.Shipping,
                    UnitPrice = model.UnitPrice,
                    AssessIntegral = model.AssessPrice,
                    State = BackStauts.Sending,
                    Condition = model.Condition,
                    CreateTime = DateTime.Now,
                    Remark = string.Empty,
                };
                context.CommunityBackOrder.Add(BackOrder);
                if (context.SaveChanges() > 0) { return result; }
                return result.SetStatus(ErrorCode.InvalidData, "邮寄失败");

            }
            catch (Exception ex)
            {
                SystemLog.Warn($"旧物回购-邮寄{ex}" + model.ToJson());
                return result.SetStatus(ErrorCode.InvalidData, "邮寄失败");
            }

        }

        /// <summary>
        /// 会员评估订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<AssessOrder>>> AssessOrder(QueryModel query)
        {
            MyResult<List<AssessOrder>> result = new MyResult<List<AssessOrder>>();
            Expression<Func<CommunityBackOrder, bool>> where = null;
            if (query.UserId < 1) { return result.SetStatus(ErrorCode.ReLogin); }
            where = where.AndAlso(item => item.UserId == query.UserId);

            result.RecordCount = context.CommunityBackOrder.Where(where).Count();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            result.Data = await context.CommunityBackOrder.Where(where).OrderByDescending(item => item.Id)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize)
                .Join(context.CommunityCenter, order => order.StoreId, store => store.Id, (order, store) => new
                {
                    Order = order,
                    Store = store
                }).Join(context.UserEntity, obj => obj.Order.UserId, user => user.Id, (obj, user) => new AssessOrder()
                {
                    Id = obj.Order.Id,
                    UserId = obj.Order.UserId,
                    Nick = user.Name,
                    StoreId = obj.Order.StoreId,
                    StorePic = AppSettings.QCloudUrl + obj.Store.Doorhead,
                    StoreName = obj.Store.Company,
                    StoreAddress = obj.Store.Address,
                    AssessIntegral = obj.Order.AssessIntegral,
                    UnitPrice = obj.Order.UnitPrice,
                    RepoType = obj.Order.RepoType,
                    ShipMethod = obj.Order.ShipMethod,
                    Condition = obj.Order.Condition,
                    ItemBrand = obj.Order.ItemBrand,
                    ItemCunt = obj.Order.ItemCunt,
                    ItemGrade = obj.Order.ItemGrade,
                    State = obj.Order.State,
                    CreateTime = obj.Order.CreateTime,
                    Remark = obj.Order.Remark
                }).OrderByDescending(item => item.Id).ToListAsync();

            return result;
        }

        /// <summary>
        /// 评估商品
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<BuyBackModel> AssessItem(BuyBackModel model)
        {
            return await Task.Run(() =>
            {
                switch (model.Condition)
                {
                    case Condition.BrandNew:
                        model.AssessPrice = model.UnitPrice * model.Count * 0.30M;
                        break;
                    case Condition.AlmostNew:
                        model.AssessPrice = model.UnitPrice * model.Count * 0.28M;
                        break;
                    case Condition.PracticallyNew:
                        model.AssessPrice = model.UnitPrice * model.Count * 0.23M;
                        break;
                    case Condition.Other:
                        model.AssessPrice = model.UnitPrice * model.Count * 0.23M;
                        break;
                    default:
                        model.AssessPrice = model.UnitPrice * model.Count * 0.11M;
                        break;
                }
                return model;
            });
        }

    }
}
