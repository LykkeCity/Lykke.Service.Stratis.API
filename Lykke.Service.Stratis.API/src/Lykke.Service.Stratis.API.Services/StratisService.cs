using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Log;
using Lykke.Service.Stratis.API.Core.Services;
using NBitcoin;
using NBitcoin.Policy;

namespace Lykke.Service.Stratis.API.Services
{
    public class StratisService: IStratisService
    {
        private readonly ILog _log;
        private readonly Network _network;

        public StratisService(ILog log,string network)
        {
            _log = log;
            _network = Network.GetNetwork(network);
        }

        public bool IsValidPrivateKey(string privateKey)
        {
            try
            {
                Key.Parse(privateKey, _network);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public string GetPrivateKey()
        {
            var key = new Key();

            return key.GetWif(_network).ToString();
        }

        public string GetPublicAddress(string privateKey)
        {
            var wallet = new BitcoinSecret(privateKey);

            return wallet.PubKey.GetAddress(_network).ToString();
        }

      

    }
}
