using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class PhoneAttribution
    {
        public long Id { get; set; }
        public string Prefix { get; set; }
        public string Phone { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string Isp { get; set; }
        public string Postcode { get; set; }
        public string AreaCode { get; set; }
        public string ChinaCode { get; set; }
    }
}
