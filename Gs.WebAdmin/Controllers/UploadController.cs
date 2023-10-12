using Gs.Application;
using Gs.Application.Models;
using Gs.Core.Utils;
using Gs.Domain.Models.Dto;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gs.WebAdmin.Controllers
{
    /// <summary>
    /// 上传
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class UploadController : Controller
    {
        private readonly IQCloudPlugin QCloud;
        private readonly IHostingEnvironment HostEnv;
        private readonly AppSetting AppSettings;
        public UploadController(IQCloudPlugin cloudPlugin, IHostingEnvironment environment, IOptionsMonitor<AppSetting> monitor)
        {
            QCloud = cloudPlugin;
            HostEnv = environment;
            AppSettings = monitor.CurrentValue;
        }

        /// <summary>
        /// 上传图至COS
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Image([FromBody] AddShopPicModel model)
        {
            MyResult result = new MyResult();
            if (!string.IsNullOrEmpty(model.Url) && model.Url != "/images/add_pic.png" && model.Url.Length > 1000)
            {
                try
                {
                    String BasePic = model.Url;
                    var fileName = DateTime.Now.GetTicket().ToString();
                    String FilePath = PathUtil.Combine("Upload", "Mall", SecurityUtil.MD5(fileName).Substring(0, 16).ToLower() + ".png");
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
            result.Data = new { Url = model.Url };
            return result;
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public async Task<IActionResult> UEditor(String action)
        {
            if (action.Equals("config", StringComparison.OrdinalIgnoreCase))
            {
                return new JsonResult(new
                {
                    imageUrl = "http://localhost/ueditor/php/controller.php?action=uploadimage",
                    imagePath = "/ueditor/php/",
                    imageFieldName = "upfile",
                    imageMaxSize = 2048,
                    imageAllowFiles = new String[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp" }
                });
            }
            var files = Request.Form.Files;
            string callback = Request.Query["callback"];
            string editorId = Request.Query["editorid"];
            if (files != null && files.Count > 0)
            {
                var file = files[0];
                string contentPath = HostEnv.WebRootPath;
                string fileDir = Path.Combine(contentPath, "upload");
                if (!Directory.Exists(fileDir))
                {
                    Directory.CreateDirectory(fileDir);
                }
                string fileExt = Path.GetExtension(file.FileName);
                string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + fileExt;
                string filePath = Path.Combine(fileDir, newFileName);
                String QCloudPath = $"/Upload/Mall/{DateTime.Now.ToString("yyyyMMdd")}/{newFileName}";
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                    fs.Position = 0;
                    byte[] bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, bytes.Length);
                    await QCloud.PutObject(QCloudPath, new MemoryStream(bytes));
                }
                var fileInfo = getUploadInfo(AppSettings.QCloudUrl + QCloudPath, file.FileName,
                    Path.GetFileName(filePath), file.Length, fileExt);
                string json = BuildJson(fileInfo);

                Response.ContentType = "text/html";
                if (callback != null)
                {
                    await Response.WriteAsync(String.Format("<script>{0}(JSON.parse(\"{1}\"));</script>", callback, json));
                }
                else
                {
                    await Response.WriteAsync(json);
                }
                return View();
            }
            return NoContent();
        }

        /// <summary>
        /// 构建JSON
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private string BuildJson(Hashtable info)
        {
            List<string> fields = new List<string>();
            string[] keys = new string[] { "originalName", "name", "url", "size", "state", "type" };
            for (int i = 0; i < keys.Length; i++)
            {
                fields.Add(String.Format("\"{0}\": \"{1}\"", keys[i], info[keys[i]]));
            }
            return "{" + String.Join(",", fields) + "}";
        }

        /// <summary>
        /// 获取上传信息
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="originalName"></param>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <param name="type"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private Hashtable getUploadInfo(string URL, string originalName, string name, long size, string type, string state = "SUCCESS")
        {
            Hashtable infoList = new Hashtable();

            infoList.Add("state", state);
            infoList.Add("url", URL);
            infoList.Add("originalName", originalName);
            infoList.Add("name", Path.GetFileName(URL));
            infoList.Add("size", size);
            infoList.Add("type", Path.GetExtension(originalName));

            return infoList;
        }
    }
}
