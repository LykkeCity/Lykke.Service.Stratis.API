using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.Stratis.API.Core.Domain.Balance;
using Lykke.Service.Stratis.API.Core.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Service.Stratis.API.AzureRepositories.Balance
{
    public class BalanceRepository : IBalanceRepository
    {


        private INoSQLTableStorage<BalanceEntity> _table;
        private static string GetPartitionKey() => "";
        private static string GetRowKey(string address) => address;

        public BalanceRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _table = AzureTableStorage<BalanceEntity>.Create(connectionStringManager, "Balances", log);
        }

        public async Task<IBalance> GetAsync(string address)
        {
            return await _table.GetDataAsync(GetPartitionKey(), GetRowKey(address));
        }

        public async Task AddAsync(string address)
        {
            await _table.InsertOrReplaceAsync(new BalanceEntity
            {
                PartitionKey = GetPartitionKey(),
                RowKey = GetRowKey(address)
            });
        }
    }
}
