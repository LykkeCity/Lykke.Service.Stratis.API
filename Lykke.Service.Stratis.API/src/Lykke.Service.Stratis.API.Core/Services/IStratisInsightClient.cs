using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.Stratis.API.Core.Domain.InsightClient;

namespace Lykke.Service.Stratis.API.Core.Services
{
    public interface IStratisInsightClient
    {
        Task<ulong> GetBalanceSatoshis(string address);
        Task<TxUnspent[]> GetTxsUnspentAsync(string address);
        Task<TxBroadcast> BroadcastTxAsync(string v);
        Task<long?> GetLatestBlockHeight();
    }
}
