using Gs.Domain.Configs;
using Gs.Domain.Models;
using Gs.Domain.Models.Admin;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gs.WebAdmin.Controllers
{
    /// <summary>
    /// 会员量化宝
    /// </summary>
    public class MiningController : WebBaseController
    {
        private readonly IMiningService MiningSub;
        public MiningController(IMiningService miningService)
        {
            MiningSub = miningService;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 量化宝列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<MiningBase>>> SysMinings()
        {
            MyResult<List<MiningBase>> result = new MyResult<List<MiningBase>>();
            return await Task.Run(() =>
            {
                result.Data = Constants.BaseSetting.ToList();
                return result;
            });
        }

        /// <summary>
        /// 量化宝列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<MiningInfo>>> MiningList([FromBody] QueryMining query)
        {
            return await MiningSub.BaseList(query);
        }

        /// <summary>
        /// 添加量化宝
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Grant([FromBody] QueryMining info)
        {
            return await MiningSub.Grant(info);
        }

        /// <summary>
        /// 量化宝延期
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> Extension([FromBody] MiningInfo info)
        {
            return await MiningSub.Extension(info);
        }

        /// <summary>
        /// 关闭量化宝
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> Colse([FromBody] MiningInfo info)
        {
            return await MiningSub.Colse(info);
        }

    }
}
