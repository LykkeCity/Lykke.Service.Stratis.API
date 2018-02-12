using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.Stratis.API.Core.Domain.Broadcast;
using Lykke.Service.Stratis.API.Core.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Service.Stratis.API.AzureRepositories.BroadcastInprogress
{
    public class BroadcastInProgressRepository : IBroadcastInProgressRepository
    {
        private readonly INoSQLTableStorage<BroadcastInProgressEntity> _table;
        private static string GetPartitionKey() => "";
        private static string GetRowKey(Guid operationId) => operationId.ToString();

        public BroadcastInProgressRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _table = AzureTableStorage<BroadcastInProgressEntity>.Create(connectionStringManager, "BroadcastsInProgress", log);
        }

        public async Task AddAsync(Guid operationId, string hash)
        {
            await _table.InsertOrReplaceAsync(new BroadcastInProgressEntity
            {
                PartitionKey = GetPartitionKey(),
                RowKey = GetRowKey(operationId),
                Hash = hash
            });
        }
    }
}
