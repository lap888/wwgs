using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class StoreCategory
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public int Sort { get; set; }
        public string Remark { get; set; }
    }
}
