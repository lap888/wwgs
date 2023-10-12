using System;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Gs.Domain.Models.Admin;
using Gs.Domain.Enums;

namespace Gs.WebAdmin.Controllers
{
    [Route("webapi/[action]")]
    public class WebApiController : WebBaseController
    {
        public IAdminService AdminService { get; set; }
        public IPermissionService PermissionService { get; set; }

        public WebApiController(IAdminService adminService, IPermissionService permissionService)
        {
            AdminService = adminService;
            PermissionService = permissionService;
        }

        #region 登录退出
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public MyResult<object> Login([FromBody] AdminUser model)
        {
            return AdminService.Login(model);
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public MyResult<object> Logout()
        {
            MyResult result = new MyResult();
            return AdminService.LogoutUser();
        }
        #endregion

        #region 管理员
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public MyResult<object> AddMemberAdd_Update([FromBody] AdminUser model)
        {
            if (!string.IsNullOrEmpty(model.Id))
            {
                return AdminService.UpdateAccount(model);
            }
            return AdminService.AddAccount(model);
        }
        /// <summary>
        /// 密码修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public MyResult<object> MemberPwd_Update([FromBody] AdminUser model)
        {
            return AdminService.UpdatePwd(model);
        }

        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public MyResult<object> GetBackstageUser(string id)
        {
            return AdminService.GetBackstageUser(id);
        }

        /// <summary>
        /// 禁用用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public MyResult<object> SetMemberState([FromBody] AdminUser model)
        {
            if (model.AccountStatus == AccountStatus.Normal)
            {
                model.AccountStatus = AccountStatus.Disabled;
            }
            else
            {
                model.AccountStatus = AccountStatus.Normal;
            }
            return AdminService.UpdateAccount(model);
        }

        #endregion

        #region 后台用户列表
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MyResult<object> BackstageUser([FromBody] AdminSearch model)
        {
            return AdminService.GetBackstageUserList(model);
        }
        #endregion

        #region 角色模块
        [HttpPost]
        public MyResult GetRoles()
        {
            return PermissionService.GetRoles();
        }
        [HttpPost]
        public MyResult<object> SaveRoles([FromBody]RoleModel model)
        {
            return PermissionService.SaveRoles(model);
        }
        [HttpPost]
        public MyResult<object> DeleteRoles([FromBody]RoleModel model)
        {
            return PermissionService.DeleteRoles(model);
        }
        #endregion
    }
}