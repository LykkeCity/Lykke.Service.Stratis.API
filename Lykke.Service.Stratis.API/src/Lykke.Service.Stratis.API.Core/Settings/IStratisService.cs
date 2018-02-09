using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;

namespace Lykke.Service.Stratis.API.Core.Settings
{
    public interface IStratisService
    {
        BitcoinAddress GetBitcoinAddress(string address);

    }
}
