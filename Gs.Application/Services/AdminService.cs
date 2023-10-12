using Gs.Core;
using Gs.Core.Extensions;
using Gs.Core.Mvc;
using Gs.Core.Utils;
using Gs.Domain.Configs;
using Gs.Domain.Context;
using Gs.Domain.Entity;
using Gs.Domain.Enums;
using Gs.Domain.Models;
using Gs.Domain.Models.Admin;
using Gs.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gs.Application.Services
{
    /// <summary>
    /// 后台管理
    /// </summary>
    public class AdminService : IAdminService
    {
        private readonly WwgsContext context;
        public AdminService(WwgsContext mySql)
        {
            context = mySql;
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MyResult<object> Login(AdminUser model)
        {
            MyResult result = new MyResult();
            string sessionCode = string.Empty;
            try
            {
                var code = CookieUtil.GetCookie(Constants.WEBSITE_VERIFICATION_CODE);
                if (code != null)
                {
                    sessionCode = DataProtectionUtil.UnProtect(code);
                }
            }
            catch (Exception ex)
            {
                SystemLog.Debug(ex.Message, ex);
            }
            if (model.ErrCount >= 3)
            {
                if (!model.VerCode.ToString().ToLower().Equals(sessionCode.ToLower()))
                {
                    return result.SetStatus(ErrorCode.NotFound, "验证码输入不正确！");
                }
            }

            SystemUser account = context.SystemUser.FirstOrDefault(t => t.LoginName == model.LoginName);
            if (account == null)
            {
                return result.SetStatus(ErrorCode.NotFound, "账号不存在！");
            }
            string pwd = SecurityUtil.MD5(model.Password);
            if (!account.Password.Equals(pwd, StringComparison.OrdinalIgnoreCase))
            {
                return result.SetStatus(ErrorCode.InvalidPassword);
            }
            switch (account.AccountStatus)
            {
                case (int)AccountStatus.Disabled:
                    return result.SetStatus(ErrorCode.AccountDisabled, "账号不可用！");
            }

            account.LastLoginTime = DateTime.Now;
            account.LastLoginIp = "";//MvcHelper.ClientIP;
            context.Update(account);
            context.SaveChanges();
            MvcIdentity identity = new MvcIdentity(account.Id, account.LoginName, account.LoginName, account.Email, (int)account.RoleId, null, account.LastLoginTime);
            identity.Login(Constants.WEBSITE_AUTHENTICATION_SCHEME, x =>
            {
                x.Expires = DateTime.Now.AddHours(5);//滑动过期时间
                x.HttpOnly = true;
            });

            return result;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MyResult<object> UpdateAccount(AdminUser model)
        {
            MyResult result = new MyResult();
            SystemUser account = context.SystemUser.FirstOrDefault(t => string.IsNullOrEmpty(model.Id) && t.LoginName.Equals(model.LoginName));
            if (account != null)
            {
                return result.SetStatus(ErrorCode.NotFound, "登录名称已经存在！");
            }
            else
            {
                account = context.SystemUser.FirstOrDefault(t => t.Id.Equals(model.Id));
                if (account == null)
                {
                    return result.SetStatus(ErrorCode.NotFound, "用户异常操作失败！");
                }
            }
            if (!string.IsNullOrEmpty(model.Password))
            {
                string pwd = SecurityUtil.MD5(model.Password);
                account.Password = pwd;
            }
            if (!DataValidUtil.IsIDCard(model.IdCard))
            {
                return result.SetStatus(ErrorCode.InvalidData, "身份证非法！");
            }
            account.LoginName = model.LoginName;
            account.AccountStatus = (int)model.AccountStatus;
            account.FullName = model.FullName;
            account.RoleId = (int)model.AccountType;
            account.Mobile = model.Mobile;
            account.UpdateTime = DateTime.Now;
            account.Gender = model.Gender;
            account.AccountType = (int)model.AccountType;
            account.IdCard = model.IdCard;
            context.Update(account);
            context.SaveChanges();
            return result;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MyResult<object> AddAccount(AdminUser model)
        {
            MyResult result = new MyResult();
            SystemUser account = context.SystemUser.FirstOrDefault(t => t.LoginName == model.LoginName);
            if (account != null)
            {
                return result.SetStatus(ErrorCode.NotFound, "登录名称已经存在！");
            }
            else
            {
                account = new SystemUser();
            }
            if (!DataValidUtil.IsIDCard(model.IdCard))
            {
                return result.SetStatus(ErrorCode.InvalidData, "身份证非法！");
            }
            model.Password = model.Password == "" ? "123456" : model.Password;
            string pwd = SecurityUtil.MD5(model.Password);
            account.Id = Guid.NewGuid().ToString("N");
            account.LoginName = model.LoginName;
            account.FullName = model.FullName;
            account.CreateTime = DateTime.Now;
            account.AccountType = (int)model.AccountType;
            account.RoleId = (int)model.AccountType;
            account.Password = pwd;
            account.Mobile = model.Mobile;
            account.AccountStatus = (int)AccountStatus.Normal;
            account.SourceType = (int)SourceType.Web;
            account.Gender = model.Gender;
            account.IdCard = model.IdCard;
            context.Add(account);
            context.SaveChanges();
            return result;
        }

        /// <summary>
        /// 更新密码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MyResult<object> UpdatePwd(AdminUser model)
        {
            MyResult result = new MyResult();
            AdminCookie backUser = GetUserCook();
            SystemUser backstageModel = context.SystemUser.FirstOrDefault(t => t.Id == backUser.Id);
            if (backstageModel == null)
            {
                return result.SetStatus(ErrorCode.NotFound, "登录名称不存在！");
            }
            string pwd = SecurityUtil.MD5(model.OldPassword);
            if (pwd.Equals(backstageModel.Password))
            {
                string pwdNew = SecurityUtil.MD5(model.ConfirmPassword);
                backstageModel.Password = pwdNew;
            }
            else
            {
                return result.SetStatus(ErrorCode.NotFound, "您输入的密码不正确！");
            }
            context.Update(backstageModel);
            context.SaveChanges();
            return result;
        }

        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MyResult<object> GetBackstageUser(string id)
        {
            MyResult result = new MyResult();
            SystemUser SystemUser = context.SystemUser.FirstOrDefault(t => t.Id == id);
            if (SystemUser == null)
            {
                SystemUser = new SystemUser();
            }
            else
            {
                SystemUser.Password = "";
            }
            result.Data = SystemUser;
            return result;
        }

        /// <summary>
        /// 管理列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MyResult<object> GetBackstageUserList(AdminSearch model)
        {
            MyResult result = new MyResult();
            var query = context.SystemUser.Include(r => r.Role).ToList();

            if (!string.IsNullOrEmpty(model.FullName))
            {
                query = query.Where(t => t.FullName.Contains(model.FullName)).ToList();
            }
            if (!string.IsNullOrEmpty(model.Mobile))
            {
                query = query.Where(t => t.Mobile == model.Mobile).ToList();
            }
            if (model.AccountStatus != null && (int)model.AccountStatus > 0)
            {
                query = query.Where(t => t.AccountStatus == (int)model.AccountStatus).ToList();
            }
            if (model.BeginTime.HasValue)
            {
                query = query.Where(t => t.CreateTime >= model.BeginTime).ToList();
            }
            if (model.EndTime.HasValue)
            {
                query = query.Where(t => t.CreateTime <= model.EndTime).ToList();
            }
            int count = query.Count;
            var objList = query.OrderByDescending(t => t.CreateTime).Select(t => new
            {
                t.Id,
                roleName = t.Role.Name,
                t.AccountStatus,
                t.FullName,
                t.SourceType,
                t.CreateTime,
                t.LastLoginTime,
                t.LastLoginIp,
                t.Mobile,
                t.LoginName,
                t.UpdateTime,
                t.IdCard,
                t.AccountType
            }).ToList();
            List<AdminSearch> _list = new List<AdminSearch>();
            objList.ForEach(t => _list.Add(
                new AdminSearch
                {
                    Id = t.Id,
                    RoleName = t.roleName,
                    AccountStatusName = ((AccountStatus)t.AccountStatus).GetEnumToString(),
                    FullName = t.FullName,
                    SourceTypeName = ((SourceType)t.SourceType).GetEnumToString(),
                    CreateTime = t.CreateTime,
                    LastLoginTime = t.LastLoginTime,
                    LastLoginIp = t.LastLoginIp,
                    Mobile = t.Mobile,
                    LoginName = t.LoginName,
                    UpdateTime = t.UpdateTime,
                    AccountStatus = (AccountStatus)t.AccountStatus,
                    IdCard = t.IdCard,
                    AccountType = (AccountType)t.AccountType
                }
                ));
            result.Data = _list;
            result.RecordCount = count;
            return result;
        }

        /// <summary>
        /// 获取cookie
        /// </summary>
        /// <returns></returns>
        public AdminCookie GetUserCook()
        {
            string cookie = DataProtectionUtil.UnProtect(CookieUtil.GetCookie(Constants.WEBSITE_AUTHENTICATION_SCHEME));
            AdminCookie back = new AdminCookie();
            back = cookie.GetModel<AdminCookie>();
            return back;
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <returns></returns>
        public MyResult<object> LogoutUser()
        {
            MyResult result = new MyResult();
            CookieUtil.RemoveCookie(Constants.WEBSITE_AUTHENTICATION_SCHEME);
            return result;
        }

    }
}
