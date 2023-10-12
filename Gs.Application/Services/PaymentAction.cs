using CSRedis;
using Dapper;
using Gs.Domain.Repository;
using Gs.Core;
using Gs.Domain.Context;
using Gs.Domain.Enums;
using Gs.Domain.Models.Dto;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Gs.Application.Services
{
    public class PaymentAction : IPaymentAction
    {
        private readonly IWePayPlugin WePaySub;
        private readonly WwgsContext context;
        private readonly CSRedisClient RedisCache;
        private readonly Models.AppSetting AppSetting;
        private readonly IWalletService UserWallet;
        public PaymentAction(WwgsContext mySql, CSRedisClient redisClient, IWePayPlugin payPlugin,  IOptionsMonitor<Models.AppSetting> monitor, IWalletService userWallet)
        {
            this.RedisCache = redisClient;
            this.AppSetting = monitor.CurrentValue;
            this.UserWallet = userWallet;
            this.WePaySub = payPlugin;
            this.context = mySql;
        }

        public async Task<string> CashRecharge(string TradeNo)
        {
            if (String.IsNullOrWhiteSpace(TradeNo)) { return "fail"; }
            var PayRult = await WePaySub.Execute(new Request.ReqWepayQuery() { TradeNo = TradeNo });
            if (PayRult.IsError || !PayRult.TradeState.Equals("SUCCESS")) { return "fail"; }
            var OrderInfo = await context.Dapper.QueryFirstOrDefaultAsync<PayInfo>($"SELECT * FROM `pay_record` WHERE `PayId`={TradeNo} LIMIT 1");
            if (OrderInfo == null) { return "fail"; }
            if (OrderInfo.PayStatus != PayStatus.UN_PAID) { return "fail"; }
            if (OrderInfo.PayStatus == PayStatus.PAID)
            {
                return "<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>";
            }
            try
            {
                if (context.Dapper.State == ConnectionState.Closed) { context.Dapper.Open(); }
                using (IDbTransaction transaction = context.Dapper.BeginTransaction())
                {
                    try
                    {
                        var ChannelUid = String.IsNullOrWhiteSpace(PayRult.OpenId) ? String.Empty : PayRult.OpenId;
                        var PayRow = context.Dapper.Execute($"UPDATE `pay_record` SET `PayStatus`={(int)PayStatus.PAID},`ChannelUID`='{ChannelUid}',`ModifyTime`=NOW() WHERE `PayId`={TradeNo}", null, transaction);
                        transaction.Commit();
                        if (PayRow > 0)
                        {
                            await UserWallet.ChangeWalletAmount(OrderInfo.UserId, OrderInfo.Amount, AccountModifyType.CASH_RECHARGE, false, OrderInfo.PayId.ToString());
                        }
                        return "<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>";
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

        public async Task<string> ChangeAliPay(string TradeNo)
        {
            if (String.IsNullOrWhiteSpace(TradeNo)) { return "fail"; }
            var PayRult = await WePaySub.Execute(new Request.ReqWepayQuery() { TradeNo = TradeNo });
            if (PayRult.IsError || !PayRult.TradeState.Equals("SUCCESS")) { return "fail"; }
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
                        var ChannelUid = String.IsNullOrWhiteSpace(PayRult.OpenId) ? String.Empty : PayRult.OpenId;
                        var PayRow = context.Dapper.Execute($"UPDATE `pay_record` SET `PayStatus`={(int)PayStatus.PAID},`ChannelUID`='{ChannelUid}',`ModifyTime`=NOW() WHERE `PayId`={TradeNo}", null, transaction);
                        var UserRow = context.Dapper.Execute($"UPDATE `user` SET `alipay`='{OrderInfo.Custom}' WHERE `id`={OrderInfo.UserId}", null, transaction);
                        transaction.Commit();
                        return "<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>";
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

        public async Task<string> WePayNotify(string TradeNo)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(TradeNo)) { return "fail"; }
                var PayRult = await WePaySub.Execute(new Request.ReqWepayQuery() { TradeNo = TradeNo });
                if (PayRult.IsError || !PayRult.TradeState.Equals("SUCCESS")) { return "fail"; }
                context.Dapper.Execute($"update `order_games` set `status`=1,updatedAt=NOW() where orderId='{TradeNo}' limit 1");
                return "<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>";
            }
            catch
            {
                return "fail";
            }
        }

    }
}
