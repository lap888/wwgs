using Gs.Domain.Models.Dto;
using Gs.Domain.Repository;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Gs.WebApi.Controllers
{
    /// <summary>
    /// 会员地址
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AddressController : BaseController
    {
        /// <summary>
        /// 
        /// </summary>
        public IAddressService AddressSub { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addressService"></param>
        public AddressController(IAddressService addressService)
        {
            AddressSub = addressService;
        }

        /// <summary>
        /// 获取地址列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<List<UserAddress>>> List()
        {
            return await AddressSub.AddressList(base.TokenModel.Id);
        }

        /// <summary>
        /// 删除地址
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> Del(int id)
        {
            return await AddressSub.DelAddress(base.TokenModel.Id, id);
        }

        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> Set(int id)
        {
            return await AddressSub.SetDefault(base.TokenModel.Id, id);
        }

        /// <summary>
        /// 编辑地址
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Edit([FromBody] UserAddress req)
        {
            return await AddressSub.SetAddress(base.TokenModel.Id, req);
        }

    }
}
