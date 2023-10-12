using Gs.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Domain.Models.Dto
{
    public class QueryCoinRecordDto:QueryModel
    {
        public CottonModifyType ModifyType { get; set; }
        /// <summary>
        /// 资产名称
        /// </summary>
        public string CoinType { get; set; }
        /// <summary>
        /// accountId
        /// </summary>
        public long AccountId { get; set; }
    }
}
