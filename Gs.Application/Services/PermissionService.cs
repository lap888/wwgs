using System;
using System.Linq;
using Gs.Core.Mvc;
using Gs.Core.Utils;
using Gs.Core.Action;
using Gs.Domain.Enums;
using Gs.Domain.Entity;
using Gs.Domain.Context;
using Gs.Core.Extensions;
using Gs.Domain.Repository;
using Gs.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;
using Gs.Core;

namespace Gs.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly WwgsContext context;
        public static object locker = new object();
        static List<MenuModel> _menus = new List<MenuModel>();
        List<RoleModel> _RolePermissions = new List<RoleModel>();
        static Dictionary<int, List<MenuModel>> currentMenus = new Dictionary<int, List<MenuModel>>();
        public PermissionService(WwgsContext mySql)
        {
            context = mySql;
        }

        public static IReadOnlyList<MenuModel> Menus { get { return _menus.AsReadOnly(); } }

        /// <summary>
        /// 获取角色所拥有的权限
        /// </summary>
        /// <returns></returns>
        public MyResult GetRoles()
        {
            MyResult result = new MyResult();
            var list = context.SystemRoles.ToList();
            list.ForEach(t =>
            {
                t.Menus = context.SystemRolePermission.Where(x => x.RoleId == t.Id).Select(x => x.ActionId).ToList();
            });
            result.Data = list;
            return result;
        }
        /// <summary>
        /// 删除指定角色
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MyResult<object> DeleteRoles(RoleModel model)
        {
            MyResult result = new MyResult();
            //
            var backageUser = context.SystemUser.FirstOrDefault(t => t.RoleId == model.Id);
            if (backageUser != null)
            {
                return result.SetError("该角色下有管理人员,禁止删除该角色！");
            }
            var role = context.SystemRoles.SingleOrDefault(t => t.Id == model.Id);
            if (role != null)
            {
                var o1 = context.SystemRolePermission.Where(t => t.RoleId == role.Id).ToList();
                o1.ForEach(t =>
                {
                    if (!model.Menus.Contains(t.ActionId))
                    {
                        context.Remove(t);
                    }
                });
                context.Remove(role);
                context.SaveChanges();
            }
            return result;
        }
        /// <summary>
        /// 保存,添加,修改角色
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MyResult<object> SaveRoles(RoleModel model)
        {
            MyResult result = new MyResult();
            var list = context.SystemRoles.ToList();
            if (list.Any(t => t.Name == model.Name && t.Id != model.Id))
            {
                return result.SetError("已存在同名的角色定义");
            }
            SystemRoles role;
            if (model.Id < 1)
            {

                int newId = 1;
                while (list.Any(t => t.Id == newId))
                {
                    newId = newId * 2;
                }
                role = new SystemRoles { Id = newId };
                context.SystemRoles.Add(role);
            }
            else
            {
                role = context.SystemRoles.SingleOrDefault(t => t.Id == model.Id);
                if (role == null)
                {
                    return result.SetStatus(ErrorCode.NotFound, "角色不存在");
                }
                context.Update(role);
            }
            role.Name = model.Name;

            if (model.Menus != null)
            {
                var o1 = context.SystemRolePermission.Where(t => t.RoleId == role.Id).ToList();
                o1.ForEach(t =>
                {
                    if (!model.Menus.Contains(t.ActionId))
                    {
                        context.Remove(t);
                    }
                });
                var _actions = o1.Select(t => t.ActionId).ToList();
                model.Menus.ForEach(t =>
                {
                    if (!_actions.Contains(t))
                    {
                        context.Add(new SystemRolePermission { RoleId = role.Id, ActionId = t, CreateTime = DateTime.Now });
                    }
                });
            }
            context.SaveChanges();
            #region 清除缓存
            //
            _RolePermissions.Clear();
            currentMenus = new Dictionary<int, List<MenuModel>>();
            #endregion
            role.Menus = model.Menus;
            result.Data = role;
            return result;
        }

        public IReadOnlyList<RoleModel> RolePermissions
        {
            get
            {
                if (_RolePermissions.Count == 0)
                {
                    _RolePermissions = context.SystemRoles.Select(t => new RoleModel { Id = t.Id, Name = t.Name }).ToList();
                    _RolePermissions.ForEach(t =>
                    {
                        t.Menus = context.SystemRolePermission.Where(x => x.RoleId == t.Id).Select(x => x.ActionId).ToList();
                    });
                }
                return _RolePermissions.AsReadOnly();
            }
        }

        #region Action
        /// <summary>
        /// 当前用户权限,菜单
        /// </summary>
        public List<MenuModel> CurrentMenus
        {
            get
            {
                var identity = (ServiceExtension.HttpContext.User.Identity as MvcIdentity);
                if (currentMenus == null)
                {
                    return _menus = new List<MenuModel>();
                }
                if (!currentMenus.ContainsKey(identity.RoleId) || currentMenus[identity.RoleId].Count == 0)
                {
                    lock (locker)
                    {
                        if (!currentMenus.ContainsKey(identity.RoleId) || currentMenus[identity.RoleId].Count == 0)
                        {
                            var role = RolePermissions.SingleOrDefault(x => x.Id == identity.RoleId);
                            List<MenuModel> list = new List<MenuModel>();
                            if (role != null)
                            {
                                role.Menus.ForEach(x =>
                                {
                                    var menu = Menus.SingleOrDefault(t => t.ActionId == x);
                                    if (menu == null)
                                    {
                                        return;
                                    }
                                    list.Add(menu);
                                    if (!string.IsNullOrEmpty(menu.ParentId) && !list.Exists(t => t.ActionId == menu.ParentId))
                                    {
                                        var m = Menus.SingleOrDefault(t => t.ActionId == menu.ParentId);
                                        if (m != null)
                                        {
                                            list.Add(m);
                                        }
                                    }
                                });
                            }
                            currentMenus[identity.RoleId] = list;
                        }
                    }
                }
                return currentMenus[identity.RoleId];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionDescriptor"></param>
        /// <returns></returns>
        public object RegistAction(List<ControllerActionDescriptor> actionDescriptor)
        {
            try
            {
                var actionLists = actionDescriptor.Select(t => t.DisplayName.ToString()).ToList();
                var systemActionLists = context.SystemActions.Where(t => !actionLists.Contains(t.ActionName)).ToList();
                if (systemActionLists.Count > 0)
                {
                    context.RemoveRange(systemActionLists);
                    context.SaveChanges();
                }
                ParentAtions.ParentAtionsList.ForEach(t =>
                {
                    context.Add(new SystemActions { ActionId = t.Id, ActionName = t.Name, ActionDescription = t.Name, Orders = t.Order, Url = t.Url, CreateTime = DateTime.Now, Icon = t.Icon });
                });
                var actions = context.SystemActions.ToList();
                foreach (var a in actionDescriptor)
                {
                    var dbAction = actions.SingleOrDefault(t => t.ActionName == a.DisplayName);
                    if (dbAction != null)
                    {
                        context.Update(dbAction);
                    }
                    else
                    {
                        dbAction = new SystemActions
                        {

                            ActionId = Guid.NewGuid().ToString("N"),
                            CreateTime = DateTime.Now,
                            ActionName = a.DisplayName
                        };
                        context.Add(dbAction);
                    }
                    a.SetFieldValue("Id", dbAction.ActionId.ToString());
                    var attr = a.MethodInfo.GetCustomAttribute<ActionAttribute>();
                    dbAction.ActionDescription = attr.Name;
                    dbAction.ParentAction = attr.ParentId;
                    dbAction.Orders = attr.Order;
                    dbAction.Icon = attr.Icon;
                    dbAction.Url = PathUtil.Combine(a.ControllerName, a.ActionName);
                }
                context.SaveChanges();
                _menus = context.SystemActions.Select(t => new MenuModel
                {
                    ActionName = t.ActionName,
                    ActionId = t.ActionId,
                    ActionDescription = t.ActionDescription,
                    Url = t.Url,
                    Orders = t.Orders ?? 0,
                    ParentId = t.ParentAction,
                    Icon = t.Icon

                }).ToList();
                _menus.ForEach(t =>
                {
                    var p = ParentAtions.ParentAtionsList.SingleOrDefault(x => x.Id == t.ParentId);
                    if (p != null)
                    {
                        t.ParentName = p.Name;
                    }
                });

                return _menus;
            }
            catch (Exception ex)
            {
                SystemLog.Debug(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 注册角色 为管理员赋予全不权限
        /// </summary>
        public void RegistRole()
        {
            var accountTypeLists = JsonExtension.GetEnumToString<AccountType>();
            var rolePermmissionLists = context.SystemRolePermission.ToList();
            var actionIds = Menus.Select(t => t.ActionId).ToList();
            //获取最新的角色权限关系
            rolePermmissionLists.Where(t => !actionIds.Contains(t.ActionId)).ToList().ForEach(x =>
            {
                context.Remove(x);
            });
            foreach (var accountType in accountTypeLists)
            {
                var role = context.SystemRoles.SingleOrDefault(t => t.Id == accountType.Item1);
                if (role == null)
                {
                    context.Add(new SystemRoles { Id = accountType.Item1, Name = accountType.Item2 });
                }
                else
                {
                    role.Name = accountType.Item2;
                    context.Update(role);
                }
                //超级管理员 赋予所有权限
                if (accountType.Item1 == (int)AccountType.Admin)
                {
                    foreach (var m in Menus)
                    {
                        if (!rolePermmissionLists.Any(x => x.RoleId == accountType.Item1 && x.ActionId == m.ActionId))
                        {
                            context.Add(new SystemRolePermission { RoleId = accountType.Item1, ActionId = m.ActionId, CreateTime = DateTime.Now });
                        }
                    }
                }
            }
            context.SaveChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="actionId"></param>
        /// <returns></returns>
        public bool HasPermission(ActionExecutingContext context, string actionId)
        {
            var identity = (context.HttpContext.User.Identity as MvcIdentity);
            if (identity.IsAuthenticated)
            {
                var role = RolePermissions.SingleOrDefault(x => x.Id == identity.RoleId);
                if (role != null && role.Menus.Contains(actionId))
                {
                    return true;
                }
                if (context.HttpContext.IsAjaxRequest())
                {
                    MyResult result = new MyResult();
                    result.SetStatus(ErrorCode.Forbidden);
                    context.Result = new ObjectResult(result);
                }
                else
                {
                    var view = new ViewResult();
                    view.ViewName = "~/Views/Home/Welcome.cshtml";
                    context.Result = view;
                }
                return false;
            }
            return true;
        }
        #endregion
    }
}
