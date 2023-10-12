using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Dto
{
    public class AddShopPicModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PId { get; set; }
        public string Url { get; set; } = "";
        public int Sort { get; set; }
    }
}
