using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Assets;
using Lykke.Service.BlockchainApi.Contract.Balances;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Stratis.API.Core;
using Lykke.Service.Stratis.API.Core.Domain.Balance;
using Lykke.Service.Stratis.API.Core.Domain.Broadcast;

namespace Lykke.Service.Stratis.API.Helper
{
    public static class Extensions
    {
        public static AssetResponse ToAssetResponse(this Asset self)
        {
            return new AssetResponse
            {
                Accuracy = self.Accuracy,
                Address = string.Empty,
                AssetId = self.Id,
                Name = self.Id
            };
        }

        public static BroadcastedTransactionState ToBroadcastedTransactionState(this BroadcastState self)
        {
            switch (self)
            {
                case BroadcastState.Broadcasted:
                    return BroadcastedTransactionState.InProgress;
                case BroadcastState.Completed:
                    return BroadcastedTransactionState.Completed;
                case BroadcastState.Failed:
                    return BroadcastedTransactionState.Failed;
                default:
                    throw new ArgumentException($"Failed to convert " +
                                                $"{nameof(BroadcastState)}.{Enum.GetName(typeof(BroadcastState), self)} " +
                                                $"to {nameof(BroadcastedTransactionState)}");
            }
        }
        public static AssetContract ToAssetContract(this Asset self)
        {
            return ToAssetResponse(self);
        }

        public static DateTime GetTimestamp(this IBroadcast self)
        {
            switch (self.State)
            {
                case BroadcastState.Broadcasted:
                    return self.BroadcastedUtc.Value;
                case BroadcastState.Completed:
                    return self.CompletedUtc.Value;
                case BroadcastState.Failed:
                    return self.FailedUtc.Value;
                default:
                    throw new ArgumentException($"Unsupported IBroadcast.State={Enum.GetName(typeof(BroadcastState), self.State)}");
            }
        }

        public static WalletBalanceContract ToWalletBalanceContract(this IBalancePositive self)
        {
            return new WalletBalanceContract
            {
                Address = self.Address,
                AssetId = Asset.Stratis.Id,
                Balance = Conversions.CoinsToContract(self.Amount, Asset.Stratis.Accuracy),
                Block = self.Block
            };
        }
    }
}
