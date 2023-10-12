using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class UserAddress
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string Area { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public int IsDefault { get; set; }
        public int IsDel { get; set; }
    }
}
