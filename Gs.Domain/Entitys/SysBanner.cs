using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class SysBanner
    {
        public long Id { get; set; }
        public int Queue { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public int Type { get; set; }
        public string Source { get; set; }
        public string Params { get; set; }
        public string CityCode { get; set; }
        public bool IsDel { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Remark { get; set; }
    }
}
