using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Gs.Application.Response;
using Gs.Core;
using Gs.Domain.Enums;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gs.WebApi.Controllers
{
    /// <summary>
    /// 异步通知
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class NotifyController : ControllerBase
    {
        private readonly IUserSerivce UserSerivce;
        private readonly IAliPayAction AliAction;
        private readonly IPaymentAction PayAction;
        /// <summary>
        /// 异步通知
        /// </summary>
        /// <param name="userSerivce"></param>
        /// <param name="alipay"></param>
        /// <param name="paymentAction"></param>
        public NotifyController(IUserSerivce userSerivce, IAliPayAction alipay, IPaymentAction paymentAction)
        {
            AliAction = alipay;
            PayAction = paymentAction;
            UserSerivce = userSerivce;
        }

        /// <summary>
        /// 支付宝
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Ali()
        {
            Dictionary<string, string> keys = new Dictionary<string, string>();
            try
            {
                ICollection<string> requestItem = Request.Form.Keys;
                foreach (var item in requestItem) { keys.Add(item, Request.Form[item]); }
                if (!keys.TryGetValue("out_trade_no", out string out_trade_no)) { return Content("fail"); }
                if (String.IsNullOrWhiteSpace(out_trade_no)) { return Content("fail"); }
                if (!keys.TryGetValue("trade_status", out string trade_status)) { return Content("fail"); }
                if (String.IsNullOrWhiteSpace(trade_status)) { return Content("fail"); }
                if (!trade_status.Equals("TRADE_SUCCESS")) { return Content("fail"); }
                keys.TryGetValue("passback_params", out string passback_params);
                if (String.IsNullOrWhiteSpace(passback_params)) { passback_params = String.Empty; }
                if (passback_params.Equals(ActionType.CHANGE_ALIPAY.ToString()))
                {
                    return Content(await AliAction.ChangeAliPay(out_trade_no));
                }
                if (passback_params.Equals(ActionType.CASH_RECHARGE.ToString()))
                {
                    return Content(await AliAction.CashRecharge(out_trade_no));
                }
                if (passback_params.Equals(ActionType.SHOPPING.ToString()))
                {
                    return Content(await AliAction.Shopping(out_trade_no));
                }
                return Content(await UserSerivce.AliNotify(out_trade_no));
            }
            catch (Exception ex)
            {
                SystemLog.Error("异步通知异常:\r\n" + keys.ToJson(), ex);
                return Content("fail");
            }

        }

        /// <summary>
        /// 微信支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> WePay()
        {
            try
            {
                Stream stream = HttpContext.Request.Body;
                StreamReader readstream = new StreamReader(stream);
                String xmlMsg = readstream.ReadToEnd();
                RspWepayNotify model = new RspWepayNotify();
                using (var reader = new StringReader(xmlMsg))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(RspWepayNotify));
                    model = (RspWepayNotify)serializer.Deserialize(reader);
                }
                if (string.IsNullOrWhiteSpace(model.TradeNo))
                {
                    return Content("fail");
                }
                if (model.Attach.Equals(ActionType.CHANGE_ALIPAY.ToString()))
                {
                    return Content(await PayAction.ChangeAliPay(model.TradeNo));
                }
                if (model.Attach.Equals(ActionType.CASH_RECHARGE.ToString()))
                {
                    return Content(await PayAction.CashRecharge(model.TradeNo));
                }
                return Content(await PayAction.WePayNotify(model.TradeNo));
            }
            catch (Exception ex)
            {
                SystemLog.Debug(ex);
                return Content("fail");
            }
        }

        /// <summary>
        /// 查询微信充值订单
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<Object>> QueryOrder(String TradeNo)
        {
            MyResult<Object> result = new MyResult<object>();
            var rult = await PayAction.CashRecharge(TradeNo);
            if (rult.Equals("fail"))
            {
                result.SetStatus(ErrorCode.InvalidData, "支付失败");
            }
            return result;
        }
    }
}