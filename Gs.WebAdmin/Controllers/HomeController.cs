using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gs.Application.Services;
using Gs.Core.Action;
using Gs.Core.Extensions;
using Gs.Core.Utils;
using Gs.Domain.Configs;
using Gs.Domain.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gs.WebAdmin.Controllers
{
    public class HomeController : WebBaseController
    {
        /// <summary>
        /// 入口
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                string path = HttpContext.Request.Query["from"];
                if (string.IsNullOrEmpty(path))
                {
                    path = CookieUtil.GetCookie(Constants.LAST_LOGIN_PATH);
                }
                if (!string.IsNullOrEmpty(path) && path != "/")
                {
                    return Redirect(System.Web.HttpUtility.UrlDecode(path));
                }
            }
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult ValidateCode()
        {
            ValidateCode _vierificationCodeServices = new ValidateCode();
            string code = "";
            System.IO.MemoryStream ms = _vierificationCodeServices.Create(out code);
            CookieUtil.AppendCookie(Constants.WEBSITE_VERIFICATION_CODE, DataProtectionUtil.Protect(code));
            return File(ms.ToArray(), @"image/png");
        }

        /// <summary>
        /// 欢迎页
        /// </summary>
        /// <returns></returns>
        [Route("Welcome")]
        public ViewResult Welcome()
        {
            var cookies = ServiceExtension.HttpContext.Request.Cookies;
            return View();
        }

        /// <summary>
        /// 失败页
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("Denied")]
        public ViewResult Denied() { return View(); }

        /// <summary>
        /// 角色管理
        /// </summary>
        /// <returns></returns>
        [Action("角色管理", ActionType.SystemManager, 1)]
        public ViewResult Roles()
        {
            var result = PermissionService.Menus;
            return View(result);
        }

        /// <summary>
        /// 操作员管理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Action("操作员管理", ActionType.SystemManager, 2)]
        public ViewResult BackstageUser(AdminViewModel model) { return View(); }

        /// <summary>
        /// 修改管理员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ViewResult AddMember(string id = "0")
        {
            if (id.Equals("0"))
            {
                ViewBag.title = "添加管理员";
            }
            else
            {
                ViewBag.title = "修改管理员";
            }
            return View();
        }

        /// <summary>
        /// APP管理
        /// </summary>
        /// <returns></returns>
        [Action("消息管理", ActionType.APPManager, 1)]
        public ViewResult NoticeManager() { return View(); }

        /// <summary>
        /// APP管理
        /// </summary>
        /// <returns></returns>
        [Action("轮播管理", ActionType.APPManager, 2)]
        public ViewResult BannerManager() { return View(); }

        /// <summary>
        /// APP管理
        /// </summary>
        /// <returns></returns>
        [Action("会员反馈", ActionType.APPManager, 3)]
        public ViewResult FeedbackManager() { return View(); }

        /// <summary>
        /// 会员管理
        /// </summary>
        /// <returns></returns>
        [Action("会员管理", ActionType.UsersManager, 1)]
        public ViewResult MemberManage() { return View(); }

        /// <summary>
        /// 会员量化宝
        /// </summary>
        /// <returns></returns>
        [Action("量化宝管理", ActionType.UsersManager, 3)]
        public ViewResult MemberMining() { return View(); }

        /// <summary>
        /// 商城管理
        /// </summary>
        /// <returns></returns>
        [Action("商品管理", ActionType.MallManager, 1)]
        public ViewResult MallGoods() { return View(); }

        /// <summary>
        /// 商城管理
        /// </summary>
        /// <returns></returns>
        [Action("订单管理", ActionType.MallManager, 1)]
        public ViewResult MallOrders() { return View(); }

        /// <summary>
        /// 运营中心管理
        /// </summary>
        /// <returns></returns>
        [Action("社区运营", ActionType.CommunityManager, 1)]
        public ViewResult Community() { return View(); }

        /// <summary>
        /// 评估订单
        /// </summary>
        /// <returns></returns>
        [Action("评估订单", ActionType.CommunityManager, 2)]
        public ViewResult AssessOrder() { return View(); }

        /// <summary>
        /// 城市合伙人
        /// </summary>
        /// <returns></returns>
        [Action("城市合伙人", ActionType.PartnerManager, 1)]
        public ViewResult CityPartner() { return View(); }

        /// <summary>
        /// 区县合伙人
        /// </summary>
        /// <returns></returns>
        [Action("区县合伙人", ActionType.PartnerManager, 2)]
        public ViewResult AreaPartner() { return View(); }



    }
}