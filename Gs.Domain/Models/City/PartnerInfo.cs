using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.City
{
    public class PartnerInfo
    {
        public int CityId { get; set; }
        public long UserId { get; set; }
        public string Nick { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string AreaCode { get; set; }
        public string AreaName { get; set; }
        public string WeChat { get; set; }
        public string Mobile { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }

    }
}
