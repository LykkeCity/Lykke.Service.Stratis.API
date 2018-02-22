using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Stratis.API.Core;
using Lykke.Service.Stratis.API.Core.Domain.History;
using System;

namespace Lykke.Service.Stratis.API.Helper
{
    public static class HistoryExtensions
    {
        public static HistoricalTransactionContract ToHistoricalContract(this IHistoryItem self)
        {
            return new HistoricalTransactionContract
            {
                Amount = Conversions.CoinsToContract(self.Amount, Constants.Assets[self.AssetId].Accuracy),
                AssetId = self.AssetId,
                FromAddress = self.FromAddress,
                Hash = self.Hash,
                OperationId = self.OperationId ?? Guid.Empty,
                Timestamp = self.TimestampUtc,
                ToAddress = self.ToAddress
            };
        }
    }
}
