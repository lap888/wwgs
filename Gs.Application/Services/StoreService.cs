using Gs.Core;
using System;
using System.Linq;
using Gs.Domain.Enums;
using Gs.Domain.Models;
using Gs.Domain.Entity;
using Gs.Domain.Context;
using Gs.Domain.Repository;
using Gs.Domain.Models.Store;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Gs.Core.Utils;
using CSRedis;
using System.Text;
using Dapper;
using Gs.Application.Models;
using Microsoft.Extensions.Options;

namespace Gs.Application.Services
{
    /// <summary>
    /// 商城
    /// </summary>
    public class StoreService : IStoreService
    {
        private readonly WwgsContext context;
        private readonly CSRedisClient RedisCache;
        private readonly AppSetting AppSettings;
        private readonly IAlipay AlipaySub;
        private readonly IWePayPlugin WePaySub;
        private readonly IIntegralService IntegralSub;
        private readonly ICottonService CottonSub;
        private const String HtmlStr = @"<!DOCTYPE html><html><head><meta charset=""utf-8""><meta http-equiv=""x-dns-prefetch-control"" content=""on"" /><meta name=""apple-mobile-web-app-capable"" content=""yes"" /><meta content = ""telephone=no"" name=""format-detection"" /><meta name = ""viewport""content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, minimum-scale=1.0, user-scalable=0""><title></title></head><body>{0}</body></html>";
        public StoreService(WwgsContext mySql, CSRedisClient cSRedisClient, IOptionsMonitor<AppSetting> monitor,
            IIntegralService integralService, ICottonService cottonService, IAlipay alipay, IWePayPlugin wepay)
        {
            context = mySql;
            AlipaySub = alipay;
            WePaySub = wepay;
            AppSettings = monitor.CurrentValue;
            RedisCache = cSRedisClient;
            IntegralSub = integralService;
            CottonSub = cottonService;
        }

        /// <summary>
        /// 积分兑换
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> Exchange(ExchangeModel exchange)
        {
            MyResult<Object> result = new MyResult<Object>();
            CSRedisClientLock CacheLock = null;
            Decimal Rate = 0.01M;

            try
            {
                CacheLock = RedisCache.Lock($"EXCHANGE_INTEGRAL_{exchange.UserId}", 30);
                if (exchange.Num < 1) { return result.SetStatus(ErrorCode.InvalidData, "输入金额有误"); }
                Decimal CottonNum = exchange.Num * Rate;
                if (await CottonSub.ChangeAmount(exchange.UserId, -CottonNum, CottonModifyType.EXCHANGE_INTEGRAL, false, CottonNum.ToString("0.####")))
                {
                    if (!await IntegralSub.ChangeAmount(exchange.UserId, exchange.Num, IntegralModifyType.EXCHANGE_INTEGRAL, false, exchange.Num.ToString()))
                    {
                        SystemLog.Warn($"积分兑换添加积分失败:{exchange.UserId}-积分{exchange.Num}");

                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                SystemLog.Debug("积分兑换", ex);
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
            return result.SetStatus(ErrorCode.InvalidData, "兑换失败");
        }

        /// <summary>
        /// 商品列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<ItemDetail>>> GoodsList(QueryModel query)
        {
            MyResult<List<ItemDetail>> result = new MyResult<List<ItemDetail>>();
            Expression<Func<StoreItem, bool>> where = null;
            where = where.AndAlso(item => item.Deleted == false && item.Published == true);

            result.RecordCount = await context.StoreItem.Where(where).CountAsync();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            var ListItem = await context.StoreItem.Where(where).OrderByDescending(item => item.Id)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToListAsync();
            result.Data = new List<ItemDetail>();
            if (ListItem == null) { return result; }
            foreach (var item in ListItem)
            {
                var model = new ItemDetail()
                {
                    Id = item.Id,
                    CateId = item.CateId,
                    Images = new List<string>(item.Images.Split(",")),
                    Deleted = item.Deleted,
                    Description = item.Description,
                    Keywords = item.Keywords,
                    MetaTitle = item.MetaTitle,
                    Name = item.Name,
                    OldPrice = item.OldPrice,
                    PointsPrice = item.PointsPrice,
                    ServicePrice = item.ServicePrice,
                    Stock = item.Stock,
                    Remark = item.Remark,
                    Sku = item.Sku,
                };
                model.Images = new List<string>();
                model.Description = String.Format(HtmlStr,model.Description);
                foreach (var img in item.Images.Split(","))
                {
                    model.Images.Add(AppSettings.QCloudUrl + img);
                }
                result.Data.Add(model);
            }

            return result;
        }

        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="submit"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> SubOrder(SubmitOrder submit)
        {
            MyResult<Object> result = new MyResult<object>();
            CSRedisClientLock CacheLock = null;

            try
            {
                CacheLock = RedisCache.Lock($"MALL_SUBMIT_{submit.UserId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "您操作太快了"); }

                if (submit.UserId < 1) { return result.SetStatus(ErrorCode.ReLogin); }
                if (submit.StoreId < 1) { return result.SetStatus(ErrorCode.InvalidData, "请选择运营中心"); }
                if (submit.ItemId < 1) { return result.SetStatus(ErrorCode.InvalidData, "商品不存在"); }
                if (submit.AddressId < 1) { return result.SetStatus(ErrorCode.InvalidData, "请先择收货地址"); }

                var Address = await context.UserAddress.FirstOrDefaultAsync(item => item.Id == submit.AddressId && item.IsDel == 0);
                if (Address == null) { return result.SetStatus(ErrorCode.InvalidData, "收货地址有误"); }

                var UserInfo = await context.UserEntity.FirstOrDefaultAsync(item => item.Id == submit.UserId);
                if (UserInfo == null) { return result.SetStatus(ErrorCode.ReLogin); }
                if (UserInfo.Status > 0) { return result.SetStatus(ErrorCode.InvalidData, "账号已被冻结"); }

                var Community = await context.CommunityCenter.FirstOrDefaultAsync(item => item.Id == submit.StoreId);
                if (Community == null) { return result.SetStatus(ErrorCode.InvalidData, "请选择运营中心"); }

                var Goods = context.StoreItem.Where(item => item.Id == submit.ItemId && item.Stock > 0 && item.Deleted == false).FirstOrDefault();
                if (Goods == null) { return result.SetStatus(ErrorCode.InvalidData, "商品已售完"); }

                var Integral = await IntegralSub.Info(submit.UserId);
                if (Integral.Usable < Goods.PointsPrice) { return result.SetStatus(ErrorCode.InvalidData, "积分不足"); }

                StoreOrder OrderInfo = new StoreOrder()
                {
                    ItemId = submit.ItemId,
                    ItemName = Goods.Name,
                    ItemPic = Goods.Images.Split(',')[0],
                    PayIntegral = Goods.PointsPrice,
                    PayNo = String.Empty,
                    ServicePrice = Goods.ServicePrice,
                    UserId = submit.UserId,
                    StoreId = submit.StoreId,
                    ExpressNum = string.Empty,
                    Contacts = Address.Name,
                    ContactTel = Address.Phone,
                    ShippingAddress = $"{Address.Province} {Address.City} {Address.Area} {Address.Address}",
                    State = OrderStatus.UN_PAID,
                    CreateTime = DateTime.Now,
                    Remark = String.Empty
                };
                context.StoreOrder.Add(OrderInfo);
                if (context.SaveChanges() < 1) { return result.SetStatus(ErrorCode.InvalidData, "下单失败"); }
                result.Data = new { OrderNo = OrderInfo.Id };
                return result;
            }
            catch (Exception ex) { SystemLog.Debug("商城下单", ex); }
            finally { if (null != CacheLock) { CacheLock.Unlock(); } }
            return result.SetStatus(ErrorCode.InvalidData, "下单失败");
        }

        /// <summary>
        /// 支付
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> SubPay(PaymentModel model)
        {
            MyResult<Object> result = new MyResult<Object>();
            CSRedisClientLock CacheLock = null;

            try
            {
                CacheLock = RedisCache.Lock($"MALL_PAYMENT_{model.OrderNo}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "您操作太快了"); }

                StoreOrder OrderInfo = context.StoreOrder.Where(item => item.Id.ToString() == model.OrderNo).FirstOrDefault();
                if (OrderInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "订单不存在"); }
                if (OrderInfo.State != OrderStatus.UN_PAID) { return result.SetStatus(ErrorCode.InvalidData, "订单已失效"); }

                var Integral = await IntegralSub.Info(OrderInfo.UserId);
                if (Integral.Usable < OrderInfo.PayIntegral) { return result.SetStatus(ErrorCode.InvalidData, "积分不足"); }

                if (await IntegralSub.Frozen(OrderInfo.UserId, OrderInfo.PayIntegral))
                {
                    try
                    {
                        #region 支付订单
                        PayRecord PayInfo = new PayRecord()
                        {
                            UserId = model.UserId,
                            Channel = model.PayType,
                            Currency = 1,
                            Amount = OrderInfo.ServicePrice,
                            Fee = OrderInfo.ServicePrice * 0.006M,
                            ActionType = ActionType.SHOPPING,
                            Custom = String.Empty,
                            ChannelUid = String.Empty,
                            CreateTime = DateTime.Now,
                            ModifyTime = null,
                            PayStatus = PayStatus.UN_PAID,
                        };

                        StringBuilder PaySql = new StringBuilder();
                        PaySql.Append("INSERT INTO `pay_record` (`UserId`, `Channel`, `Currency`, `Amount`, `Fee`, `ActionType`, `Custom`, `PayStatus`, `ChannelUID`, `CreateTime`) VALUES ");
                        PaySql.Append("(@UserId, @Channel, @Currency,@Amount, @Fee,@ActionType, @Custom, @PayStatus, @ChannelUID, NOW());SELECT @@IDENTITY");

                        String PayId = context.Dapper.ExecuteScalar<String>(PaySql.ToString(), PayInfo);
                        if (String.IsNullOrWhiteSpace(PayId)) { return result.SetStatus(ErrorCode.InvalidData, "下单失败"); }

                        OrderInfo.PayNo = PayId;
                        context.StoreOrder.Update(OrderInfo);
                        context.SaveChanges();

                        if (model.PayType == PayChannel.AliPay)
                        {
                            String AppUrl = await AlipaySub.GetSignStr(new Request.ReqAlipayAppSubmit()
                            {
                                OutTradeNo = PayId,
                                TotalAmount = OrderInfo.ServicePrice.ToString("0.00"),
                                Subject = OrderInfo.ItemName,
                                NotifyUrl = AppSettings.AlipayNotify,
                                TimeOutExpress = "15m",
                                PassbackParams = PayInfo.ActionType.ToString()
                            });
                            result.Data = AppUrl;
                        }
                        else
                        {
                            var PayObj = await WePaySub.Execute(new Request.ReqWepaySubmit()
                            {
                                TradeNo = PayId,
                                Body = OrderInfo.ItemName,
                                TotalFee = Math.Ceiling(OrderInfo.ServicePrice * 100.00M).ToInt(),
                                Attach = PayInfo.ActionType.ToString(),
                                TradeType = "APP",
                                NotifyUrl = AppSettings.WePayNotify,
                            });
                            result.Data = new { TradeNo = PayId, PayStr = WePaySub.MakeSign(PayObj.PrepayId) };
                        }
                        return result;
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        await IntegralSub.Frozen(OrderInfo.UserId, -OrderInfo.PayIntegral);
                        SystemLog.Debug("下单失败", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLog.Debug("商城支付下单失败", ex);
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
            return result.SetStatus(ErrorCode.InvalidData, "下单失败");
        }

        /// <summary>
        /// 我的订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<StoreOrder>>> MyOrders(QueryModel query)
        {
            MyResult<List<StoreOrder>> result = new MyResult<List<StoreOrder>>();
            result.Data = new List<StoreOrder>();
            Expression<Func<StoreOrder, bool>> where = null;
            where = where.AndAlso(item => item.UserId == query.UserId);

            result.RecordCount = await context.StoreOrder.Where(where).CountAsync();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            var orders = await context.StoreOrder.Where(where).OrderByDescending(item => item.Id)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToListAsync();
            foreach (var item in orders)
            {
                item.ItemPic = AppSettings.QCloudUrl + item.ItemPic;
                result.Data.Add(item);
            }
            return result;
        }

        /// <summary>
        /// 收货
        /// </summary>
        /// <returns></returns>
        public async Task<MyResult<Object>> Receive(ReceiveModel model)
        {
            MyResult<Object> result = new MyResult<Object>();
            var order = await context.StoreOrder.Where(item => item.UserId == model.UserId && item.Id.ToString() == model.OrderNo)
                .FirstOrDefaultAsync();
            if (order == null) { return result.SetStatus(ErrorCode.InvalidData, "订单不存在"); }
            if (order.State != OrderStatus.DELIVERED) { return result.SetStatus(ErrorCode.InvalidData, "订单已完成"); }
            return result;
        }

        #region 后台管理
        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> AddItem(StoreItem item)
        {
            MyResult<Object> result = new MyResult<Object>();
            try
            {
                item.CreateTime = DateTime.Now;
                item.UpdateTime = DateTime.Now;
                context.StoreItem.Add(item);
                if (await context.SaveChangesAsync() > 0) { return result; }
                return result.SetStatus(ErrorCode.InvalidData, "添加失败");
            }
            catch (Exception ex)
            {
                SystemLog.Debug("添加商品失败", ex);
                return result.SetStatus(ErrorCode.InvalidData, "添写信息不完整~");
            }
        }

        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> DelItem(StoreItem item)
        {
            MyResult<Object> result = new MyResult<Object>();
            try
            {
                if (item.Id < 1) { return result.SetStatus(ErrorCode.InvalidData, "商品不存在"); }
                var model = await context.StoreItem.FirstOrDefaultAsync(e => e.Id == item.Id);
                if (model == null) { return result.SetStatus(ErrorCode.InvalidData, "商品不存在"); }
                model.Deleted = true;
                context.StoreItem.Update(model);
                if (context.SaveChanges() > 0) { return result; }
            }
            catch (Exception ex)
            {
                SystemLog.Debug("删除商品失败", ex);
            }
            return result.SetStatus(ErrorCode.InvalidData, "删除失败");
        }

        /// <summary>
        /// 修改商品
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> ModifyItem(StoreItem item)
        {
            MyResult<Object> result = new MyResult<Object>();
            try
            {
                if (item.Id < 1) { return result.SetStatus(ErrorCode.InvalidData, "商品不存在"); }
                var model = await context.StoreItem.FirstOrDefaultAsync(e => e.Id == item.Id);
                if (model == null) { return result.SetStatus(ErrorCode.InvalidData, "商品不存在"); }
                model.Images = item.Images;
                model.UpdateTime = DateTime.Now;
                model.CateId = item.CateId;
                model.Name = item.Name;
                model.Keywords = item.Keywords;
                model.MetaTitle = item.MetaTitle;
                model.OldPrice = item.OldPrice;
                model.PointsPrice = item.PointsPrice;
                model.Published = item.Published;
                model.ServicePrice = item.ServicePrice;
                model.Stock = item.Stock;
                model.Sku = item.Sku;
                model.Remark = item.Remark;
                model.Description = item.Description;
                model.Deleted = item.Deleted;
                context.StoreItem.Update(model);
                if (context.SaveChanges() > 0) { return result; }
            }
            catch (Exception ex)
            {
                SystemLog.Debug("修改商品失败", ex);
            }
            return result.SetStatus(ErrorCode.InvalidData, "修改失败");
        }

        /// <summary>
        /// 商品列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<StoreItem>>> ItemList(QueryModel query)
        {
            MyResult<List<StoreItem>> result = new MyResult<List<StoreItem>>();
            Expression<Func<StoreItem, bool>> where = null;
            where = where.AndAlso(item => item.Deleted == false);

            result.RecordCount = await context.StoreItem.Where(where).CountAsync();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            result.Data = await context.StoreItem.Where(where).OrderByDescending(item => item.Id)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

            return result;
        }

        /// <summary>
        /// 订单列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<StoreOrder>>> OrderList(QueryModel query)
        {
            MyResult<List<StoreOrder>> result = new MyResult<List<StoreOrder>>();
            Expression<Func<StoreOrder, bool>> where = null;
            if (query.Id >= 0)
            {
                where = where.AndAlso(item => item.State == (OrderStatus)query.Id);
            }
            else { where = where.AndAlso(item => item.Id > 0); }
            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                List<Int64> StoreIds = context.CommunityCenter.Where(item => item.Company.Contains(query.Keyword))
                    .Select(item => item.Id).ToList();
                where = where.AndAlso(item => StoreIds.Contains(item.StoreId));
            }

            result.RecordCount = await context.StoreOrder.Where(where).CountAsync();
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            result.Data = await context.StoreOrder.Where(where).OrderByDescending(item => item.Id)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

            return result;
        }

        #endregion

    }
}
