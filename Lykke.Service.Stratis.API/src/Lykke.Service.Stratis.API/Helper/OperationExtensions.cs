using Common;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Stratis.API.Core;
using Lykke.Service.Stratis.API.Core.Domain.Operations;
using System;

namespace Lykke.Service.Stratis.API.Helper
{
    public static class OperationExtensions
    {
        public static BroadcastedSingleTransactionResponse ToSingleResponse(this IOperation self)
        {
            self.EnsureType(OperationType.SingleFromSingleTo);

            return new BroadcastedSingleTransactionResponse
            {
                Amount = Conversions.CoinsToContract(self.Amount, Constants.Assets[self.AssetId].Accuracy),
                Fee = Conversions.CoinsToContract(self.Amount, Constants.Assets[self.AssetId].Accuracy),
                Error = self.Error,
                Hash = self.Hash,
                OperationId = self.OperationId,
                State = self.State.ToBroadcastedState(),
                Timestamp = self.TimestampUtc,
                Block = Convert.ToInt64(self.TimestampUtc.ToUnixTime()),
            };
        }

        public static void EnsureType(this IOperation self, OperationType expected)
        {
            if (self.Type != expected)
            {
                throw new InvalidOperationException($"{expected} operation type was expected, {self.Type} operation was fetched");
            }
        }

        public static BroadcastedTransactionState ToBroadcastedState(this OperationState self)
        {
            return (BroadcastedTransactionState)((int)self + 1);
        }
    }
}
