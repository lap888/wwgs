using Gs.Domain.Repository;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Gs.Domain.Models;
using Gs.Domain.Models.Dto;
using Gs.Domain.Enums;
using System.Collections.Generic;
using Gs.Core;
using System;
using Gs.Core.Action;
using Gs.Domain.Models.Admin;

namespace Gs.WebAdmin.Controllers
{
    /// <summary>
    /// 会员管理
    /// </summary>
    public class MemberController : WebBaseController
    {
        private readonly IUserSerivce UserSub;
        public MemberController(IUserSerivce userSerivce)
        {
            UserSub = userSerivce;
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 会员管理
        /// </summary>
        /// <returns></returns>
        [Action("账户流水", Core.Action.ActionType.UsersManager, 2)]
        public ViewResult AccountRecord() { return View(); }

        /// <summary>
        /// 会员列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<ListModel<UserDto>>> List([FromBody] QueryUser query)
        {
            MyResult<ListModel<UserDto>> result = new MyResult<ListModel<UserDto>>();

            result.Data = await UserSub.UserList(query);
            result.RecordCount = result.Data.Total;
            result.PageCount = (result.Data.Total + query.PageSize - 1) / query.PageSize;

            return result;
        }

        /// <summary>
        /// 会员账务流水
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> AccountInfo([FromBody] QueryModel query)
        {
            query.UserId = query.Id;
            return await UserSub.AccountInfo(query);
        }

        /// <summary>
        /// 会员账务流水
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<AdminAccountRecord>>> AccountRecords([FromBody] QueryModel query)
        {
            return await UserSub.AccountRecord(query);
        }

        /// <summary>
        /// 冻结会员
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Freeze([FromBody] UserDto user)
        {
            MyResult<object> Rult = new MyResult<object>();
            Rult.Data = await UserSub.Freeze(user);
            if (Rult.Data == null) { Rult.SetStatus(ErrorCode.InvalidData, "解冻失败"); }
            return Rult;
        }

        /// <summary>
        /// 解冻会员
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Unfreeze([FromBody] UserDto user)
        {
            MyResult<object> Rult = new MyResult<object>();
            Rult.Data = await UserSub.Unfreeze(user);
            if (Rult.Data == null) { Rult.SetStatus(ErrorCode.InvalidData, "解冻失败"); }
            return Rult;
        }

        /// <summary>
        /// 修改会员信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Modify([FromBody] UserDto user)
        {
            MyResult<object> Rult = new MyResult<object>();
            Rult.Data = await UserSub.Modify(user);
            if (Rult.Data == null) { Rult.SetStatus(ErrorCode.InvalidData, "修改失败"); }
            return Rult;
        }

        /// <summary>
        /// 修改会员信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> AuthInfo([FromBody] UserDto user)
        {
            MyResult<object> Rult = new MyResult<object>();
            Rult.Data = await UserSub.AuthInfo(user);
            if (Rult.Data == null) { Rult.SetStatus(ErrorCode.InvalidData, "认证信息不存在"); }
            return Rult;
        }

        public async Task<MyResult<object>> PaidCoin([FromBody] PaidDto model)
        {
            return await UserSub.PaidCoin(model, 1);
        }


    }
}
