using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class SystemCopywriting
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int Key { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
    }
}
