using System;
using System.Collections.Generic;
using System.Text;
using Common.Log;
using Lykke.Service.Stratis.API.Core.Settings;
using Lykke.Service.Stratis.API.Core.Settings.ServiceSettings;
using NBitcoin;

namespace Lykke.Service.Stratis.API.Services
{
    public class StratisService : IStratisService
    {
        private readonly ILog _log;
        private readonly Network _network;


        public StratisService(ILog log,
            StratisAPISettings apiSettings
            )
        {

            _log = log;
            _network = Network.GetNetwork(apiSettings.Network);
        }
        public BitcoinAddress GetBitcoinAddress(string address)
        {
            try
            {
                return BitcoinAddress.Create(address, _network);
            }
            catch
            {
                return null;
            }
        }

    }
}
