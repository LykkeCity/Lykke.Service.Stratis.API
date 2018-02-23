using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.Stratis.API.Core.Domain.Broadcast;
using Lykke.Service.Stratis.API.Core.Domain.History;
using Lykke.Service.Stratis.API.Core.Domain.Operations;
using NBitcoin;

namespace Lykke.Service.Stratis.API.Core.Settings
{
    public interface IStratisService
    {
        BitcoinAddress GetBitcoinAddress(string address);
        decimal GetFee();
        Task<decimal> GetAddressBalance(string fromAddress);
        Task<string> BuildTransactionAsync(Guid requestOperationId, BitcoinAddress fromAddress, BitcoinAddress toAddress, decimal amount, bool requestIncludeFee);
        Task<IBroadcast> GetBroadcastAsync(Guid operationId);
        Transaction GetTransaction(string signedTransaction);
        Task BroadcastAsync(Transaction transaction, Guid operationId);
        Task DeleteBroadcastAsync(IBroadcast broadcast);

        Task<decimal> RefreshAddressBalance(string address, long? block = null);
        Task<IOperation> GetOperationAsync(Guid operationId, bool loadItems = true);

        Task<string> BuildAsync(Guid operationId, OperationType type, Asset asset, bool subtractFee,
            (BitcoinAddress from, BitcoinAddress to, Money amount)[] items);

        Task<bool> TryDeleteObservableAddressAsync(ObservationCategory category, string address);
        Task<IEnumerable<IHistoryItem>> GetHistoryAsync(ObservationCategory category, string address, string afterHash = null, int take = 100);

        Task<bool> TryCreateObservableAddressAsync(ObservationCategory category, string address);
    }
}
