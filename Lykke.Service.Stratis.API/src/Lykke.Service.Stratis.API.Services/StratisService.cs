using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Stratis.API.Core;
using Lykke.Service.Stratis.API.Core.Domain.Broadcast;
using Lykke.Service.Stratis.API.Core.Domain.InsightClient;
using Lykke.Service.Stratis.API.Core.Repositories;
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
        private readonly IBroadcastRepository _broadcastRepository;
        private readonly IBroadcastInProgressRepository _broadcastInProgressRepository;

        public StratisService(ILog log, StratisAPISettings apiSettings,
            IStratisInsightClient stratisInsightClient,
            IBroadcastRepository broadcastRepository,
            IBroadcastInProgressRepository broadcastInProgressRepository)
        {
            _apiSettings = apiSettings;
            _stratisInsightClient = stratisInsightClient;
            _log = log;
            _broadcastRepository = broadcastRepository;
            _broadcastInProgressRepository = broadcastInProgressRepository;
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

        public async Task<IBroadcast> GetBroadcastAsync(Guid operationId)
        {
            return await _broadcastRepository.GetAsync(operationId);
        }

        public Transaction GetTransaction(string transactionHex)
        {
            try
            {
                return Transaction.Parse(transactionHex);
            }
            catch
            {
                return null;
            }
        }

        public async Task BroadcastAsync(Transaction transaction, Guid operationId)
        {
            TxBroadcast response;

            try
            {
                response = await _stratisInsightClient.BroadcastTxAsync(transaction.ToHex());

                if (response == null)
                {
                    throw new ArgumentException($"{nameof(response)} can not be null");
                }
                if (string.IsNullOrEmpty(response.Txid))
                {
                    throw new ArgumentException($"{nameof(response)}{nameof(response.Txid)} can not be null or empty");
                }
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(StratisService), nameof(BroadcastAsync),
                    $"transaction={transaction}, operationId={operationId}", ex);
                await _broadcastRepository.AddFailedAsync(operationId, transaction.GetHash().ToString(),
                    ex.ToString());

                return;
            }

            await _broadcastRepository.AddAsync(operationId, response.Txid);
            await _broadcastInProgressRepository.AddAsync(operationId, response.Txid);
        }

        public async Task DeleteBroadcastAsync(IBroadcast broadcast)
        {
            await _broadcastRepository.DeleteAsync(broadcast.OperationId);

            if (broadcast.State == BroadcastState.Broadcasted)
            {
                await _broadcastInProgressRepository.DeleteAsync(broadcast.OperationId);
            }
        }

    }
}
