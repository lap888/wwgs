﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Entity
{
    public partial class SystemActions
    {
        public string ActionId { get; set; }
        public string ActionDescription { get; set; }
        public string ActionName { get; set; }
        public DateTime CreateTime { get; set; }
        public string Url { get; set; }
        public int? Orders { get; set; }
        public string ParentAction { get; set; }
        public string Icon { get; set; }
    }
}
