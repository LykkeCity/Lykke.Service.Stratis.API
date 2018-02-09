using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Stratis.API.Core;
using Lykke.Service.Stratis.API.Core.Services;
using Lykke.Service.Stratis.API.Core.Settings;
using Lykke.Service.Stratis.API.Core.Settings.ServiceSettings;
using NBitcoin;
using NBitcoin.JsonConverters;
using NBitcoin.Policy;

namespace Lykke.Service.Stratis.API.Services
{
    public class StratisService : IStratisService
    {
        private readonly ILog _log;
        private readonly Network _network;
        private readonly IStratisInsightClient _stratisInsightClient;
        private readonly StratisAPISettings _apiSettings;
        public StratisService(ILog log, StratisAPISettings apiSettings, IStratisInsightClient stratisInsightClient)
        {
            _apiSettings = apiSettings;
            _stratisInsightClient = stratisInsightClient;
            _log = log;
            _network = Network.GetNetwork(apiSettings.Network);
        }

        public async Task<decimal> GetAddressBalance(string address)
        {
            var balanceSatoshis = await _stratisInsightClient.GetBalanceSatoshis(address);
            var balance = Money.Satoshis(balanceSatoshis).ToDecimal(Asset.Stratis.Unit);

            return balance;
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

        public decimal GetFee()
        {
            return _apiSettings.Fee;
        }

        public async Task<string> BuildTransactionAsync(Guid operationId, BitcoinAddress fromAddress,
            BitcoinAddress toAddress, decimal amount, bool includeFee)
        {
            var sendAmount = Money.FromUnit(amount, Asset.Stratis.Unit);
            var txsUnspent = await _stratisInsightClient.GetTxsUnspentAsync(fromAddress.ToString());

            var builder = new TransactionBuilder()
                .Send(toAddress, sendAmount)
                .SetChange(fromAddress)
                .SetTransactionPolicy(new StandardTransactionPolicy
                {
                    CheckFee = false
                });

            if (includeFee)
            {
                builder.SubtractFees();
            }

            foreach (var txUnspent in txsUnspent)
            {
                var coin = new Coin(
                    fromTxHash: uint256.Parse(txUnspent.Txid),
                    fromOutputIndex: txUnspent.Vout,
                    amount: Money.Coins(txUnspent.Amount),
                    scriptPubKey: fromAddress.ScriptPubKey);

                builder.AddCoins(coin);
            }

            var feeMoney = Money.FromUnit(_apiSettings.Fee, Asset.Stratis.Unit);

            var tx = builder
                .SendFees(feeMoney)
                .BuildTransaction(false);

            var coins = builder.FindSpentCoins(tx);

            return Serializer.ToString((tx: tx, coins: coins));
        }

    }
}
