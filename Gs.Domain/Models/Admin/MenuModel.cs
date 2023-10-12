using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Admin
{
    public class MenuModel
    {
        public string ActionId { get; set; }
        public string ActionName { get; set; }
        public string ActionDescription { get; set; }
        public string Url { get; set; }
        public int Orders { get; set; }
        public string ParentId { get; set; }
        public string ParentName { get; set; }
        public string Icon { get; set; } = "glyphicon-asterisk";
    }
}
