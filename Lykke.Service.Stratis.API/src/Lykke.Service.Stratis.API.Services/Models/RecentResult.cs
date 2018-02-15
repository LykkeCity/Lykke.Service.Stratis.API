using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.Stratis.API.Services.Models
{
    public class RecentResult
    {
        public RecentTransaction[] Transactions { get; set; }

        public string LastBlock { get; set; }

        public class RecentTransaction
        {
            public string Category { get; set; }
            public string TxId { get; set; }
            public uint BlockTime { get; set; }
            public long Confirmations { get; set; } // can be -1 for unconfirmed
        }
    }
}
