using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.Stratis.API.Core.Services
{
    public interface IStratisInsightClient
    {
        Task<ulong> GetBalanceSatoshis(string address);
        Task<TxUnspent[]> GetTxsUnspentAsync(string address);

    }
}
