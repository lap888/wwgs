using Gs.Application;
using Gs.Core;
using Gs.Core.Utils;
using Gs.Domain.Configs;
using Gs.Domain.Enums;
using Gs.Domain.Models.Dto;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gs.WebApi.Controllers
{
    /// <summary>
    /// 交易服务
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TradeController : BaseController
    {
        private readonly ITradeService TradeService;

        private readonly IQCloudPlugin QCloudSub;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coin"></param>
        /// <param name="qCloudSub"></param>
        public TradeController(ITradeService coin, IQCloudPlugin qCloudSub)
        {
            TradeService = coin;
            QCloudSub = qCloudSub;
        }
        /// <summary>
        /// 交易面板信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public MyResult<CoinTradeExt> GetTradeTotal()
        {
            return TradeService.GetTradeTotal(this.TokenModel.Id);
        }
        /// <summary>
        /// 交易列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MyResult<List<CoinTradeDto>>> TradeList([FromBody] TradeReqDto model)
        {
            model.PageSize = 20;
            return await TradeService.TradeList(model);
        }

        /// <summary>
        /// 发布买单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> StartBuy([FromBody] TradeDto model)
        {
            return await TradeService.StartBuy(model, base.TokenModel.Id);
        }

        /// <summary>
        /// 取消买单
        /// </summary>
        /// <param name="orderNum">订单号</param>
        /// <param name="tradePwd">交易密码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> CancleTrade( string orderNum, string tradePwd)
        {
            return await TradeService.CancleTrade( orderNum, tradePwd, base.TokenModel.Id);
        }

        /// <summary>
        /// 确认出售
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> DealBuy([FromBody] TradeDto model)
        {
            return await TradeService.DealBuy(model, base.TokenModel.Id);
        }

        /// <summary>
        /// 我的交易订单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<TradeListReturnDto>>> MyTradeList([FromBody] MyTradeListDto model)
        {
            return await TradeService.MyTradeList(model, base.TokenModel.Id);
        }

        /// <summary>
        /// 确认支付
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Paid([FromBody] PaidDto model, int userId)
        {
            if (!string.IsNullOrEmpty(model.PicUrl) && model.PicUrl.Length > 1000)
            {
                try
                {
                    String BasePic = model.PicUrl;
                    String FilePath = PathUtil.Combine(Constants.TRADE_PATH, DateTime.Now.ToString("yyyyMMdd"), Guid.NewGuid().ToString("N") + ".png");
                    Regex reg1 = new Regex("%2B", RegexOptions.IgnoreCase);
                    Regex reg2 = new Regex("%2F", RegexOptions.IgnoreCase);
                    Regex reg3 = new Regex("%3D", RegexOptions.IgnoreCase);
                    Regex reg4 = new Regex("(data:([^;]*);base64,)", RegexOptions.IgnoreCase);

                    var newBase64 = reg1.Replace(BasePic, "+");
                    newBase64 = reg2.Replace(newBase64, "/");
                    newBase64 = reg3.Replace(newBase64, "=");
                    BasePic = reg4.Replace(newBase64, "");

                    byte[] bt = Convert.FromBase64String(BasePic);
                    await QCloudSub.PutObject(FilePath, new System.IO.MemoryStream(bt));
                    model.PicUrl = FilePath;
                }
                catch (Exception ex)
                {
                    SystemLog.Debug(ex);
                    return new MyResult<object>() { Code = -1, Message = "上传交易凭证失败" };
                }
            }
            return await TradeService.Paid(model, base.TokenModel.Id);
        }

        /// <summary>
        /// 确认收款 发送MBM
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> PaidCoin([FromBody] PaidDto model)
        {
            return await TradeService.PaidCoin(model, base.TokenModel.Id);
        }

        /// <summary>
        /// 强制发送MBM
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public MyResult<object> ForcePaidCoin()
        {
            return TradeService.ForcePaidCoin();
        }

        /// <summary>
        /// 订单申述
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> CreateAppeal([FromBody] CreateAppealDto model)
        {
            MyResult result = new MyResult();
            if (!ProcessSqlStr(model.OrderId))
            {
                return result.SetStatus(ErrorCode.InvalidData, "非法操作");
            }
            if (!ProcessSqlStr(model.Description))
            {
                return result.SetStatus(ErrorCode.InvalidData, "非法操作");
            }
            if (!string.IsNullOrEmpty(model.PicUrl) && model.PicUrl.Length > 1000)
            {
                try
                {
                    String BasePic = model.PicUrl;
                    String FilePath = PathUtil.Combine(Constants.APPEALS_PATH, DateTime.Now.ToString("yyyyMMdd"), Guid.NewGuid().ToString("N") + ".png");
                    Regex reg1 = new Regex("%2B", RegexOptions.IgnoreCase);
                    Regex reg2 = new Regex("%2F", RegexOptions.IgnoreCase);
                    Regex reg3 = new Regex("%3D", RegexOptions.IgnoreCase);
                    Regex reg4 = new Regex("(data:([^;]*);base64,)", RegexOptions.IgnoreCase);

                    var newBase64 = reg1.Replace(BasePic, "+");
                    newBase64 = reg2.Replace(newBase64, "/");
                    newBase64 = reg3.Replace(newBase64, "=");
                    BasePic = reg4.Replace(newBase64, "");

                    byte[] bt = Convert.FromBase64String(BasePic);
                    await QCloudSub.PutObject(FilePath, new System.IO.MemoryStream(bt));
                    model.PicUrl = FilePath;
                }
                catch (Exception ex)
                {
                    SystemLog.Debug(ex);
                    return new MyResult<object>() { Code = -1, Message = "上传交易凭证失败" };
                }
            }
            return TradeService.CreateAppeal(model, base.TokenModel.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Str"></param>
        /// <returns></returns>
        private static bool ProcessSqlStr(string Str)
        {
            string SqlStr;
            SqlStr = " |,|=|'|and|exec|insert|select|delete|update|count|chr|mid|master|truncate|char|declare";
            bool ReturnValue = true;
            if (String.IsNullOrWhiteSpace(Str)) { return ReturnValue; }
            Str = Str.ToLower().Replace(",", "，").Replace("'", "‘").Replace(" ", "—").Replace("=", "〓");
            try
            {
                if (Str != "")
                {
                    string[] anySqlStr = SqlStr.Split('|');
                    foreach (string ss in anySqlStr)
                    {
                        if (Str.IndexOf(ss) >= 0)
                        {
                            ReturnValue = false;
                        }
                    }
                }
            }
            catch
            {
                ReturnValue = false;
            }
            return ReturnValue;
        }
    }
}
