using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.Stratis.API.Core;

namespace Lykke.Service.Stratis.API.Core.Domain.Addresses
{
    public class AddressBalance
    {
        public string Address { get; set; }
        public Asset Asset { get; set; }
        public decimal Balance { get; set; }
        public long BlockTime { get; set; }
    }
}
