using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;

namespace Lykke.Service.Stratis.API.Core.Settings
{
    public interface IStratisService
    {
        BitcoinAddress GetBitcoinAddress(string address);
        decimal GetFee();
        Task<decimal> GetAddressBalance(string fromAddress);
        Task<string> BuildTransactionAsync(Guid requestOperationId, BitcoinAddress fromAddress, BitcoinAddress toAddress, decimal amount, bool requestIncludeFee);
    }
}
