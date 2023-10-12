using Gs.Domain.Models.Admin;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Repository
{
    public interface IAdminService
    {
        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        MyResult<object> Login(AdminUser model);

        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        MyResult<object> UpdateAccount(AdminUser model);

        /// <summary>
        /// 添加后台管理员
        /// </summary>
        /// <param name="model">AccountAdd</param>
        /// <returns></returns>
        MyResult<object> AddAccount(AdminUser model);

        /// <summary>
        /// 密码修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        MyResult<object> UpdatePwd(AdminUser model);

        /// <summary>
        /// 获取后台用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        MyResult<object> GetBackstageUser(string id);

        /// <summary>
        /// 管理员列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        MyResult<object> GetBackstageUserList(AdminSearch model);

        /// <summary>
        /// 获取后台用户登录Cookie信息
        /// </summary>
        /// <returns></returns>
        AdminCookie GetUserCook();

        /// <summary>
        /// 用户退出
        /// </summary>
        /// <returns></returns>
        MyResult<object> LogoutUser();
    }
}
