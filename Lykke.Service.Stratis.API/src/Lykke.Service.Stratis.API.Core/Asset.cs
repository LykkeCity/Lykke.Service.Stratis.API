using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;

namespace Lykke.Service.Stratis.API.Core
{
    public class Asset
    {
        public Asset(string id, int decimalPlaces, MoneyUnit unit) => (Id, Accuracy, Unit) = (id, decimalPlaces, unit);

        public string Id { get; }
        public int Accuracy { get; }
        public MoneyUnit Unit { get; }

        // static instances (constants)

        public static Asset Stratis { get; } = new Asset("STRAT", 8, MoneyUnit.BTC);
    }
}
