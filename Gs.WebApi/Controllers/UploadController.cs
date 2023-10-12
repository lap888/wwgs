using Gs.Application;
using Gs.Application.Models;
using Gs.Core.Utils;
using Gs.Domain.Models;
using Gs.Domain.Models.Dto;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gs.WebApi.Controllers
{
    /// <summary>
    /// 图片上传
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UploadController : BaseController
    {
        private readonly IQCloudPlugin QCloud;
        private readonly AppSetting AppSettings;
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="cloudPlugin"></param>
        /// <param name="monitor"></param>
        public UploadController(IQCloudPlugin cloudPlugin, IOptionsMonitor<AppSetting> monitor)
        {
            QCloud = cloudPlugin;
            AppSettings = monitor.CurrentValue;
        }

        /// <summary>
        /// 上传图至COS
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Image([FromBody] UploadModel model)
        {
            MyResult result = new MyResult();
            Int64 Uid = base.TokenModel.Id;
            if (!string.IsNullOrEmpty(model.Url) && model.Url.Length > 1000)
            {
                try
                {
                    String BasePic = model.Url;
                    var fileName = DateTime.Now.GetTicket().ToString();
                    String FilePath = PathUtil.Combine("Upload", "Feedback", Uid.ToString(), SecurityUtil.MD5(fileName).Substring(0, 16).ToLower() + ".png");
                    Regex reg1 = new Regex("%2B", RegexOptions.IgnoreCase);
                    Regex reg2 = new Regex("%2F", RegexOptions.IgnoreCase);
                    Regex reg3 = new Regex("%3D", RegexOptions.IgnoreCase);
                    Regex reg4 = new Regex("(data:([^;]*);base64,)", RegexOptions.IgnoreCase);

                    var newBase64 = reg1.Replace(BasePic, "+");
                    newBase64 = reg2.Replace(newBase64, "/");
                    newBase64 = reg3.Replace(newBase64, "=");
                    BasePic = reg4.Replace(newBase64, "");

                    byte[] bt = Convert.FromBase64String(BasePic);
                    await QCloud.PutObject(FilePath, new System.IO.MemoryStream(bt));
                    model.Url = FilePath;
                }
                catch (Exception ex)
                {
                    return new MyResult<object>() { Code = -1, Message = $"图上传失败{ex}" };
                }
            }
            result.Data = new { Url = AppSettings.QCloudUrl + model.Url };
            return result;
        }
    }
}
