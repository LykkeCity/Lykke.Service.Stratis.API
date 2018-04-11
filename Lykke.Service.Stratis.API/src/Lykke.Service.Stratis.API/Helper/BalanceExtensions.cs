using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Balances;
using Lykke.Service.Stratis.API.Core.Domain.Addresses;

namespace Lykke.Service.Stratis.API.Helper
{
    public static class BalanceExtensions
    {
        public static WalletBalanceContract ToWalletContract(this AddressBalance self)
        {
            return new WalletBalanceContract()
            {
                Address = self.Address,
                AssetId = self.Asset.Id,
                Balance = Conversions.CoinsToContract(self.Balance, self.Asset.Accuracy),
                Block = self.BlockTime
            };
        }
    }
}
