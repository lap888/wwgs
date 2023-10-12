using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gs.Domain.Models.Dto
{
    [Table("yoyo_member_address")]
    public class UserAddress
    {
        [Key]
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
    }
}
