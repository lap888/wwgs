using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class CommunityCenter
    {
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
