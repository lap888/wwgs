using Gs.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Community
{
    public class CommunityInfo
    {
        /// <summary>
        /// 手机号
        /// </summary>
        public String Mobile { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public String Nick { get; set; }
        /// <summary>
        /// 图片列表
        /// </summary>
        public List<String> ListImgs { get; set; }
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Doorhead { get; set; }
        public string Company { get; set; }
        public string Describe { get; set; }
        public string Qq { get; set; }
        public string WeChat { get; set; }
        public string Website { get; set; }
        public string Contacts { get; set; }
        public string ContactTel { get; set; }
        public string AreaCode { get; set; }
        public string CityCode { get; set; }
        public string Address { get; set; }
        public decimal Lng { get; set; }
        public decimal Lat { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreateTime { get; set; }
        public int IsDel { get; set; }
        public string Remark { get; set; }

    }
}
