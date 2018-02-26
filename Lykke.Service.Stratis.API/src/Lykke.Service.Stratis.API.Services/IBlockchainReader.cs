using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.Stratis.API.Services.Models;
using NBitcoin;

namespace Lykke.Service.Stratis.API.Services
{
    public interface IBlockchainReader
    {
        Task<Utxo[]> ListUnspentAsync(int confirmationLevel, params string[] addresses);
        Task ImportAddressAsync(string address);
        Task<AddressInfo> ValidateAddressAsync(string address);
        Task<string> SendRawTransactionAsync(Transaction transaction);
    }
}
