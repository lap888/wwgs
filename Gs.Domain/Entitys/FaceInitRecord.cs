using System;
using System.Collections.Generic;

namespace Gs.Domain.Entity
{
    public partial class FaceInitRecord
    {
        public long Id { get; set; }
        public string CertifyId { get; set; }
        public string CertifyUrl { get; set; }
        public string TrueName { get; set; }
        public string IdcardNum { get; set; }
        public string Alipay { get; set; }
        public int IsUsed { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
