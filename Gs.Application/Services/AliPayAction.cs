using CSRedis;
using Dapper;
using Gs.Application.Utils;
using Gs.Core;
using Gs.Domain.Context;
using Gs.Domain.Entity;
using Gs.Domain.Enums;
using Gs.Domain.Models.Dto;
using Gs.Domain.Repository;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Gs.Application.Services
{
    public class AliPayAction : IAliPayAction
    {
        private readonly CSRedisClient RedisCache;
        private readonly WwgsContext context;
        private readonly Models.AppSetting AppSetting;
        private readonly IAlipay AlipaySub;
        private readonly IWalletService UserWallet;
        private readonly IIntegralService IntegralSub;

        public AliPayAction(WwgsContext mysql, CSRedisClient redisClient, IAlipay alipay, IOptionsMonitor<Models.AppSetting> monitor,
            IIntegralService integralService, IWalletService userWallet)
        {
            this.context = mysql;
            this.AlipaySub = alipay;
            this.UserWallet = userWallet;
            this.RedisCache = redisClient;
            this.IntegralSub = integralService;
            this.AppSetting = monitor.CurrentValue;
        }

        /// <summary>
        /// 二次认证的异步通知
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        public async Task<String> AuthAliPay(String TradeNo)
        {
            if (String.IsNullOrWhiteSpace(TradeNo)) { return "fail"; }
            AlipayResult<Response.RspAlipayTradeQuery> PayRult = await AlipaySub.Execute(new Request.ReqAlipayTradeQuery { OutTradeNo = TradeNo });
            if (PayRult.IsError || !PayRult.Result.TradeStatus.Equals("TRADE_SUCCESS")) { return "fail"; }
            var OrderInfo = await context.Dapper.QueryFirstOrDefaultAsync<PayInfo>($"SELECT * FROM `pay_record` WHERE `PayId`={TradeNo} LIMIT 1");
            if (OrderInfo == null) { return "fail"; }
            if (OrderInfo.PayStatus != PayStatus.UN_PAID) { return "fail"; }
            try
            {
                if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
                using (IDbTransaction transaction = context.Dapper.BeginTransaction())
                {
                    try
                    {
                        var ChannelUid = String.IsNullOrWhiteSpace(PayRult.Result.BuyerUserId) ? String.Empty : PayRult.Result.BuyerUserId;
                        var PayRow = context.Dapper.Execute($"UPDATE `pay_record` SET `PayStatus`={(int)PayStatus.PAID},`ChannelUID`='{ChannelUid}',`ModifyTime`=NOW() WHERE `PayId`={TradeNo}", null, transaction);
                        var UserRow = context.Dapper.Execute($"UPDATE `user` SET `alipayUid`='{ChannelUid}' WHERE `id`={OrderInfo.UserId}", null, transaction);
                        transaction.Commit();
                        return "success";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        SystemLog.Error($"修改支付宝时发生错误,订单号{TradeNo}", ex);
                        return "fail";
                    }
                    finally { if (context.Dapper.State == ConnectionState.Open) { context.Dapper.Close(); } }
                }
            }
            catch { return "fail"; }
        }

        /// <summary>
        /// 修改支付宝的异步通知
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        public async Task<String> ChangeAliPay(String TradeNo)
        {
            if (String.IsNullOrWhiteSpace(TradeNo)) { return "fail"; }
            AlipayResult<Response.RspAlipayTradeQuery> PayRult = await AlipaySub.Execute(new Request.ReqAlipayTradeQuery { OutTradeNo = TradeNo });
            if (PayRult.IsError || !PayRult.Result.TradeStatus.Equals("TRADE_SUCCESS")) { return "fail"; }
            var OrderInfo = await context.Dapper.QueryFirstOrDefaultAsync<PayInfo>($"SELECT * FROM `pay_record` WHERE `PayId`={TradeNo} LIMIT 1");
            if (OrderInfo == null) { return "fail"; }
            if (OrderInfo.PayStatus != PayStatus.UN_PAID) { return "fail"; }
            try
            {
                if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
                using (IDbTransaction transaction = context.Dapper.BeginTransaction())
                {
                    try
                    {
                        var ChannelUid = String.IsNullOrWhiteSpace(PayRult.Result.BuyerUserId) ? String.Empty : PayRult.Result.BuyerUserId;
                        var PayRow = context.Dapper.Execute($"UPDATE `pay_record` SET `PayStatus`={(int)PayStatus.PAID},`ChannelUID`='{ChannelUid}',`ModifyTime`=NOW() WHERE `PayId`={TradeNo}", null, transaction);
                        var UserRow = context.Dapper.Execute($"UPDATE `user` SET `alipay`='{OrderInfo.Custom}' WHERE `id`={OrderInfo.UserId}", null, transaction);
                        transaction.Commit();
                        return "success";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        SystemLog.Error($"修改支付宝时发生错误,订单号{TradeNo}", ex);
                        return "fail";
                    }
                    finally { if (context.Dapper.State == ConnectionState.Open) { context.Dapper.Close(); } }
                }
            }
            catch { return "fail"; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        public async Task<String> CashRecharge(String TradeNo)
        {
            if (String.IsNullOrWhiteSpace(TradeNo)) { return "fail"; }
            AlipayResult<Response.RspAlipayTradeQuery> PayRult = await AlipaySub.Execute(new Request.ReqAlipayTradeQuery { OutTradeNo = TradeNo });
            if (PayRult.IsError || !PayRult.Result.TradeStatus.Equals("TRADE_SUCCESS")) { return "fail"; }
            var OrderInfo = await context.Dapper.QueryFirstOrDefaultAsync<PayInfo>($"SELECT * FROM `pay_record` WHERE `PayId`={TradeNo} LIMIT 1");
            if (OrderInfo == null) { return "fail"; }
            if (OrderInfo.PayStatus != PayStatus.UN_PAID) { return "fail"; }
            try
            {
                if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
                using (IDbTransaction transaction = context.Dapper.BeginTransaction())
                {
                    try
                    {
                        var ChannelUid = String.IsNullOrWhiteSpace(PayRult.Result.BuyerUserId) ? String.Empty : PayRult.Result.BuyerUserId;
                        var PayRow = context.Dapper.Execute($"UPDATE `pay_record` SET `PayStatus`={(int)PayStatus.PAID},`ChannelUID`='{ChannelUid}',`ModifyTime`=NOW() WHERE `PayId`={TradeNo}", null, transaction);
                        transaction.Commit();
                        await UserWallet.ChangeWalletAmount(OrderInfo.UserId, OrderInfo.Amount, AccountModifyType.CASH_RECHARGE, false, OrderInfo.PayId.ToString());
                        return "success";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        SystemLog.Error($"修改支付宝时发生错误,订单号{TradeNo}", ex);
                        return "fail";
                    }
                    finally { if (context.Dapper.State == ConnectionState.Open) { context.Dapper.Close(); } }
                }
            }
            catch { return "fail"; }
        }

        /// <summary>
        /// 购物支付
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        public async Task<String> Shopping(String TradeNo)
        {
            if (String.IsNullOrWhiteSpace(TradeNo)) { return "fail"; }
            AlipayResult<Response.RspAlipayTradeQuery> PayRult = await AlipaySub.Execute(new Request.ReqAlipayTradeQuery { OutTradeNo = TradeNo });
            if (PayRult.IsError || !PayRult.Result.TradeStatus.Equals("TRADE_SUCCESS")) { return "fail"; }
            var OrderInfo = await context.Dapper.QueryFirstOrDefaultAsync<PayInfo>($"SELECT * FROM `pay_record` WHERE `PayId`={TradeNo} LIMIT 1");
            if (OrderInfo == null) { return "fail"; }
            if (OrderInfo.PayStatus != PayStatus.UN_PAID) { return "fail"; }
            try
            {
                if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
                using (IDbTransaction transaction = context.Dapper.BeginTransaction())
                {
                    try
                    {
                        var ChannelUid = String.IsNullOrWhiteSpace(PayRult.Result.BuyerUserId) ? String.Empty : PayRult.Result.BuyerUserId;
                        var PayRow = context.Dapper.Execute($"UPDATE `pay_record` SET `PayStatus`={(int)PayStatus.PAID},`ChannelUID`='{ChannelUid}',`ModifyTime`=NOW() WHERE `PayId`={TradeNo}", null, transaction);
                        transaction.Commit();

                        StoreOrder GoodsOrder = context.StoreOrder.Where(item => item.PayNo == TradeNo).FirstOrDefault();
                        if (GoodsOrder != null && GoodsOrder.State == Domain.Models.Store.OrderStatus.UN_PAID)
                        {
                            if (await IntegralSub.ChangeAmount(OrderInfo.UserId, GoodsOrder.PayIntegral, IntegralModifyType.SHOPPING, true, TradeNo))
                            {
                                GoodsOrder.State = Domain.Models.Store.OrderStatus.PAID;
                                context.StoreOrder.Update(GoodsOrder);
                                context.SaveChanges();
                            }
                        }
                        return "success";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        SystemLog.Error($"修改支付宝时发生错误,订单号{TradeNo}", ex);
                        return "fail";
                    }
                    finally { if (context.Dapper.State == ConnectionState.Open) { context.Dapper.Close(); } }
                }
            }
            catch { return "fail"; }

        }


        /// <summary>
        /// 生成支付链接的方法
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Amount"></param>
        /// <param name="action"></param>
        /// <param name="Custom"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> CreatePayUrl(int UserId, Decimal Amount, ActionType action, String Custom = "")
        {
            MyResult result = new MyResult();
            if (UserId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"CreatePayUrl:{UserId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//
                decimal Fee = Math.Ceiling(Amount * 0.006M * 100.00M) * 0.01M;
                PayInfo info = new PayInfo
                {
                    UserId = UserId,
                    Channel = PayChannel.AliPay,
                    Currency = Currency.Rmb,
                    Custom = String.IsNullOrWhiteSpace(Custom) ? String.Empty : Custom,
                    Amount = Amount,
                    Fee = Fee,
                    ActionType = action,
                    ChannelUID = String.Empty,
                    CreateTime = DateTime.Now,
                    ModifyTime = null,
                    PayStatus = PayStatus.UN_PAID,
                };

                String PayId = context.Dapper.ExecuteScalar<String>("INSERT INTO `pay_record` (`UserId`, `Channel`, `Currency`, `Amount`, `Fee`, `ActionType`, `Custom`, `PayStatus`, `ChannelUID`, `CreateTime`) VALUES (@UserId, @Channel, @Currency,@Amount, @Fee,@ActionType, @Custom, @PayStatus, @ChannelUID, NOW());SELECT @@IDENTITY", info);
                if (String.IsNullOrWhiteSpace(PayId)) { return result.SetStatus(ErrorCode.InvalidData, "生成支付订单失败"); }
                String AppUrl = await AlipaySub.GetSignStr(new Request.ReqAlipayAppSubmit() { OutTradeNo = PayId, TotalAmount = Amount.ToString("0.00"), Subject = action.GetDescription(), NotifyUrl = AppSetting.AlipayNotify, TimeOutExpress = "15m", PassbackParams = action.ToString() });
                result.Data = AppUrl;
            }
            catch (Exception)
            {
                return result.SetStatus(ErrorCode.InvalidData, "生成支付订单失败");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
            return result;
        }
    }
}
