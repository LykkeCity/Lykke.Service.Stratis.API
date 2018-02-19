using Common;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Stratis.API.Core;
using Lykke.Service.Stratis.API.Core.Domain.Operations;
using System;
using System.Linq;

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

        public static BroadcastedTransactionWithManyInputsResponse ToManyInputsResponse(this IOperation self)
        {
            self.EnsureType(OperationType.MultiFromSingleTo);

            return new BroadcastedTransactionWithManyInputsResponse
            {
                Error = self.Error,
                Hash = self.Hash,
                OperationId = self.OperationId,
                State = self.State.ToBroadcastedState(),
                Timestamp = self.TimestampUtc,
                Block = Convert.ToInt64(self.TimestampUtc.ToUnixTime()),
                Fee = Conversions.CoinsToContract(self.Amount, Constants.Assets[self.AssetId].Accuracy),
                Inputs = self.Items
                    .Select(x => new BroadcastedTransactionInputContract { Amount = Conversions.CoinsToContract(x.Amount, Constants.Assets[self.AssetId].Accuracy), FromAddress = x.FromAddress })
                    .ToArray()
            };
        }

        public static BroadcastedTransactionWithManyOutputsResponse ToManyOutputsResponse(this IOperation self)
        {
            self.EnsureType(OperationType.SingleFromMultiTo);

            return new BroadcastedTransactionWithManyOutputsResponse
            {
                Error = self.Error,
                Hash = self.Hash,
                OperationId = self.OperationId,
                State = self.State.ToBroadcastedState(),
                Timestamp = (self.SentUtc ?? self.CompletedUtc ?? self.FailedUtc).Value,
                Block = Convert.ToInt64(self.TimestampUtc.ToUnixTime()),
                Fee = Conversions.CoinsToContract(self.Amount, Constants.Assets[self.AssetId].Accuracy),
                Outputs = self.Items
                    .Select(x => new BroadcastedTransactionOutputContract { Amount = Conversions.CoinsToContract(x.Amount, Constants.Assets[self.AssetId].Accuracy), ToAddress = x.ToAddress })
                    .ToArray()
            };
        }

    }
}
