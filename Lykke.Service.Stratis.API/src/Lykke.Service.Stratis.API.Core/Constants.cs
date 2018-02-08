using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;
using Lykke.Service.Stratis.API.Core.Domain;

namespace Lykke.Service.Stratis.API.Core
{
    public static class Constants
    {
        public static readonly IReadOnlyDictionary<string, Asset> Assets = new Dictionary<string, Asset>
        {
            [Asset.Stratis.Id] = Asset.Stratis
        };

        public static readonly Money DefaultFee = Money.Coins(0.1M);
    }

   
}
