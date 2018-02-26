using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.Stratis.API.Core;
using Lykke.Service.Stratis.API.Core.Domain.Broadcast;
using Lykke.Service.Stratis.API.Core.Domain.History;
using Lykke.Service.Stratis.API.Core.Domain.InsightClient;
using Lykke.Service.Stratis.API.Core.Domain.Operations;
using Lykke.Service.Stratis.API.Core.Exceptions;
using Lykke.Service.Stratis.API.Core.Repositories;
using Lykke.Service.Stratis.API.Core.Services;
using Lykke.Service.Stratis.API.Core.Settings;
using Lykke.Service.Stratis.API.Core.Settings.ServiceSettings;
using Lykke.Service.Stratis.API.Services.Models;
using NBitcoin;
using NBitcoin.JsonConverters;
using NBitcoin.Policy;

namespace Lykke.Service.Stratis.API.Services
{
    public class StratisService : IStratisService
    {
        private readonly IBlockchainReader _blockchainReader;
        private readonly IAddressRepository _addressRepository;
        private readonly IHistoryRepository _historyRepository;

        private readonly ILog _log;
        private readonly Network _network;
     //   private readonly IStratisInsightClient _stratisInsightClient;
        private readonly StratisAPISettings _apiSettings;
        private readonly IBroadcastRepository _broadcastRepository;
        private readonly IBroadcastInProgressRepository _broadcastInProgressRepository;
        private readonly IBalancePositiveRepository _balancePositiveRepository;
        private readonly IOperationRepository _operationRepository;
        private readonly ISettingsRepository _settingsRepository;
        private readonly ISettings _settings;

        public StratisService(ILog log, StratisAPISettings apiSettings,
           // IStratisInsightClient stratisInsightClient,
            IBroadcastRepository broadcastRepository,
            IBroadcastInProgressRepository broadcastInProgressRepository,
            IBalancePositiveRepository balancePositiveRepository,
            IOperationRepository operationRepository,
            ISettings settings,
            ISettingsRepository settingsRepository,
            IBlockchainReader blockchainReader,
            IAddressRepository addressRepository,
            IHistoryRepository historyRepository)
        {
            _apiSettings = apiSettings;
          //  _stratisInsightClient = stratisInsightClient;
            _log = log;
            _broadcastRepository = broadcastRepository;
            _broadcastInProgressRepository = broadcastInProgressRepository;
            _balancePositiveRepository = balancePositiveRepository;
            _operationRepository = operationRepository;
            _settings = settings;
            _settingsRepository = settingsRepository;
            _blockchainReader = blockchainReader;
            _addressRepository = addressRepository;
            _historyRepository = historyRepository;
            _network = Network.GetNetwork(apiSettings.Network);
        }

        //public async Task<decimal> GetAddressBalance(string address)
        //{
        //    var balanceSatoshis = await _stratisInsightClient.GetBalanceSatoshis(address);
        //    var balance = Money.Satoshis(balanceSatoshis).ToDecimal(Asset.Stratis.Unit);
        //    return balance;
        //}

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



        //public async Task<string> BuildTransactionAsync(Guid operationId, BitcoinAddress fromAddress,
        //    BitcoinAddress toAddress, decimal amount, bool includeFee)
        //{
        //    var sendAmount = Money.FromUnit(amount, Asset.Stratis.Unit);
        //    var txsUnspent = await _stratisInsightClient.GetTxsUnspentAsync(fromAddress.ToString());

        //    var builder = new TransactionBuilder()
        //        .Send(toAddress, sendAmount)
        //        .SetChange(fromAddress)
        //        .SetTransactionPolicy(new StandardTransactionPolicy
        //        {
        //            CheckFee = false
        //        });

        //    if (includeFee)
        //    {
        //        builder.SubtractFees();
        //    }

        //    foreach (var txUnspent in txsUnspent)
        //    {
        //        var coin = new Coin(
        //            fromTxHash: uint256.Parse(txUnspent.Txid),
        //            fromOutputIndex: txUnspent.Vout,
        //            amount: Money.Coins(txUnspent.Amount),
        //            scriptPubKey: fromAddress.ScriptPubKey);

        //        builder.AddCoins(coin);
        //    }

        //    var feeMoney = Money.FromUnit(_apiSettings.Fee, Asset.Stratis.Unit);

        //    var tx = builder
        //        .SendFees(feeMoney)
        //        .BuildTransaction(false);

        //    var coins = builder.FindSpentCoins(tx);

        //    return Serializer.ToString((tx: tx, coins: coins));
        //}

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
            var hash = await _blockchainReader.SendRawTransactionAsync(transaction);

            await _operationRepository.UpdateAsync(operationId, sentUtc: DateTime.UtcNow, hash: hash);
        }

        public async Task DeleteBroadcastAsync(IBroadcast broadcast)
        {
            await _broadcastRepository.DeleteAsync(broadcast.OperationId);

            if (broadcast.State == BroadcastState.Broadcasted)
            {
                await _broadcastInProgressRepository.DeleteAsync(broadcast.OperationId);
            }
        }

        //public async Task<decimal> RefreshAddressBalance(string address, long? block = null)
        //{
        //    var balance = await GetAddressBalance(address);

        //    if (balance > 0)
        //    {
        //        var balancePositive = await _balancePositiveRepository.GetAsync(address);
        //        if (balancePositive != null && balancePositive.Amount == balance)
        //        {
        //            return balance;
        //        }

        //        if (!block.HasValue)
        //        {
        //            block = await _stratisInsightClient.GetLatestBlockHeight();
        //        }

        //        await _balancePositiveRepository.SaveAsync(address, balance, block.Value);
        //    }
        //    else
        //    {
        //        await _balancePositiveRepository.DeleteAsync(address);
        //    }

        //    return balance;
        //}

        public async Task<IOperation> GetOperationAsync(Guid operationId, bool loadItems = true)
        {
            return await _operationRepository.GetAsync(operationId, loadItems);
        }

        public async Task<string> BuildAsync(Guid operationId, OperationType type, Asset asset, bool subtractFee,
            (BitcoinAddress from, BitcoinAddress to, Money amount)[] items)
        {
            var settings = await LoadStoredSettingsAsync();

            var relayFee = new FeeRate(Money.Coins(settings.FeePerKb));

            var inputs =
                items.GroupBy(x => x.from)
                    .Select(g => new
                    {
                        Address = g.Key,
                        Amount = g.Select(x => x.amount).Sum()
                    })
                    .ToList();

            var outputs =
                items.GroupBy(x => x.to)
                    .Select(g => new
                    {
                        Address = g.Key,
                        Amount = g.Select(x => x.amount).Sum()
                    })
                    .ToList();

            var utxo = await _blockchainReader.ListUnspentAsync(
                settings.ConfirmationLevel,
                inputs.Select(from => from.Address.ToString()).ToArray());

            var unspentOutputs =
                inputs.ToDictionary(from => from.Address,
                    from => new Stack<Utxo>(utxo.Where(x => x.Address == from.Address.ToString())
                        .OrderBy(x => x.Confirmations)));

            var spentOutputs =
                inputs.ToDictionary(from => from.Address, from => new Stack<Utxo>());

            var oddOutputs =
                inputs.ToDictionary(from => from.Address, from => (TxOut) null);

            var tx = new Transaction();

            foreach (var from in inputs)
            {
                var amount = from.Amount;

                if (amount > Money.Zero)
                {
                    while (amount > Money.Zero && unspentOutputs[from.Address].TryPop(out var vout))
                    {
                        tx.AddInput(vout.AsTxIn());
                        spentOutputs[from.Address].Push(vout);
                        amount -= vout.Money;
                    }
                }

                if (amount > Money.Zero)
                {
                    throw new NotEnoughFundsException("Not enough funds", from.Address.ToString(), amount);
                }

                if (amount < Money.Zero)
                {
                    oddOutputs[from.Address] = tx.AddOutput(amount.Abs(), from.Address);
                }
            }

            foreach (var to in outputs)
            {
                var txout = tx.AddOutput(to.Amount, to.Address);
                if (txout.IsDust(relayFee))
                {
                    throw new DustException("Output amount is too small", to.Amount, to.Address);
                }
            }

            var fee = CalcFee(tx, settings);
            var totalAmount = items.Select(x => x.amount).Sum();

            if (subtractFee)
            {
                foreach (var vout in tx.Outputs.Except(oddOutputs.Where(x => x.Value != null).Select(x => x.Value)))
                {
                    vout.Value -= CalcFeeSplit(fee, totalAmount, vout.Value);
                }
            }
            else
            {
                foreach (var from in inputs)
                {
                    var inputAmount = spentOutputs[from.Address].Select(x => x.Money).Sum();
                    var operationAndFeeAmount = from.Amount + CalcFeeSplit(fee, totalAmount, from.Amount);

                    if (inputAmount < operationAndFeeAmount)
                    {
                        while (inputAmount < operationAndFeeAmount && unspentOutputs[from.Address].TryPop(out var vout))
                        {
                            tx.AddInput(vout.AsTxIn());
                            spentOutputs[from.Address].Push(vout);
                            inputAmount += vout.Money;
                        }
                    }

                    if (inputAmount < operationAndFeeAmount)
                    {
                        throw new NotEnoughFundsException("Not enough funds", from.ToString(),
                            operationAndFeeAmount - inputAmount);
                    }

                    if (inputAmount > operationAndFeeAmount)
                    {
                        oddOutputs[from.Address] = oddOutputs[from.Address] ?? tx.AddOutput(0, from.Address);
                        oddOutputs[from.Address].Value = inputAmount - operationAndFeeAmount;
                    }
                    else if (oddOutputs.TryGetValue(from.Address, out var vout)) // must always be true here
                    {
                        tx.Outputs.Remove(vout);
                        oddOutputs[from.Address] = null;
                    }
                }
            }

            await _operationRepository.UpsertAsync(operationId, type,
                items.Select(x => (x.from.ToString(), x.to.ToString(), x.amount.ToUnit(asset.Unit))).ToArray(),
                fee.ToUnit(asset.Unit), subtractFee, asset.Id);

            var coins = spentOutputs.Values
                .SelectMany(v => v)
                .Select(x => x.AsCoin())
                .ToList();

            return Serializer.ToString((tx, coins));
        }

        public async Task<ISettings> LoadStoredSettingsAsync()
        {
            return (await _settingsRepository.GetAsync()) ?? _settings;
        }

        public Money CalcFee(Transaction tx, ISettings settings)
        {
            if (settings.UseDefaultFee)
            {
                return Constants.DefaultFee;
            }
            else
            {
                var fee = new FeeRate(Money.Coins(settings.FeePerKb)).GetFee(tx);
                var min = Money.Coins(settings.MinFee);
                var max = Money.Coins(settings.MaxFee);

                return Money.Max(Money.Min(fee, max), min);
            }
        }

        public Money CalcFeeSplit(Money fee, Money totalOutput, Money output)
        {
            var unit = Asset.Stratis.Unit;
            var decimalFee = fee.ToUnit(unit);
            var decimalTotalOutput = totalOutput.ToUnit(unit);
            var decimalOutput = output.ToUnit(unit);
            var decimalResult = decimalFee * (decimalOutput / decimalTotalOutput);

            return Money.FromUnit(decimalResult, unit);
        }

        public async Task<bool> TryDeleteObservableAddressAsync(ObservationCategory category, string address)
        {
            var observableAddress = await _addressRepository.GetAsync(category, address);

            if (observableAddress != null)
            {
                await _addressRepository.DeleteAsync(category, address);
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<IHistoryItem>> GetHistoryAsync(ObservationCategory category, string address,
            string afterHash = null, int take = 100)
        {
            if (await IsObservableAsync(category, address))
            {
                return await _historyRepository.GetByAddressAsync(category, address, afterHash, take);
            }
            else
            {
                return new IHistoryItem[0];
            }
        }

        public async Task<bool> IsObservableAsync(ObservationCategory category, string address)
        {
            return (await _addressRepository.GetAsync(category, address)) != null;
        }

        public async Task<bool> TryCreateObservableAddressAsync(ObservationCategory category, string address)
        {
            var addressInfo = await _blockchainReader.ValidateAddressAsync(address);

            if (!addressInfo.IsValid)
            {
                throw new InvalidOperationException($"Invalid Stratis address: {address}");
            }

            if (!addressInfo.IsMine && !addressInfo.IsWatchOnly)
            {
                await _blockchainReader.ImportAddressAsync(address);
            }

            var observableAddress = await _addressRepository.GetAsync(category, address);

            if (observableAddress == null)
            {
                await _addressRepository.CreateAsync(category, address);
                return true;
            }

            return false;
        }

        public void EnsureSigned(Transaction transaction, ICoin[] coins)
        {
            // checking fees or dust thresholds doesn't make sense here because 
            // exact fee rate was used to build the transaction

            if (!new TransactionBuilder()
                .AddCoins(coins)
                .SetTransactionPolicy(new StandardTransactionPolicy { CheckFee = false, MaxTxFee = null, MinRelayTxFee = null })
                .Verify(transaction, out var errors))
            {
                throw new InvalidOperationException(errors.ToStringViaSeparator(Environment.NewLine));
            }
        }
    }
}
