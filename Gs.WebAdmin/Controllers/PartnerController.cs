using Gs.Domain.Entity;
using Gs.Domain.Models;
using Gs.Domain.Models.City;
using Gs.Domain.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gs.WebAdmin.Controllers
{
    public class PartnerController : Controller
    {
        private readonly ICityService CitySub;
        public PartnerController(ICityService cityService)
        {
            CitySub = cityService;
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 添加合伙人
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> Add([FromBody] PartnerInfo city)
        {
            if (city.CityId > 0)
            {
                return await CitySub.Edit(city);
            }
            return await CitySub.Add(city);
        }

        /// <summary>
        /// 合伙人列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<PartnerInfo>>> List([FromBody] QueryModel query)
        {
            return await CitySub.List(query);
        }
    }
}
