using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class SysClientVersions
    {
        public uint Id { get; set; }
        public string CurrentVersion { get; set; }
        public bool IsSilent { get; set; }
        public bool IsHotReload { get; set; }
        public string DeviceSystem { get; set; }
        public string UpdateContent { get; set; }
        public bool Production { get; set; }
        public bool? IsNecessary { get; set; }
        public string DownloadUrl { get; set; }
    }
}
