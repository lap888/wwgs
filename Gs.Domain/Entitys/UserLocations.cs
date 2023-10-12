using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class UserLocations
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Province { get; set; }
        public string ProvinceCode { get; set; }
        public string City { get; set; }
        public string CityCode { get; set; }
        public string Area { get; set; }
        public string AreaCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
