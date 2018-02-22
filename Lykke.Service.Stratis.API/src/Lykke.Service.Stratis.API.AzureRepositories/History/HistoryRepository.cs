using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.AzureStorage.Tables.Paging;
using Lykke.Service.Stratis.API.Core;
using Lykke.Service.Stratis.API.Core.Domain.History;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.Stratis.API.AzureRepositories.History
{
    public class HistoryRepository : IHistoryRepository
    {
        private INoSQLTableStorage<HistoryItemEntity> _historyStorage;
        private INoSQLTableStorage<IndexEntity> _indexStorage;
        private static string GetHistoryPartitionKey(ObservationCategory category, string address) => $"{Enum.GetName(typeof(ObservationCategory), category)}_{address}";
        private static string GetHistoryRowKey(DateTime timestamp, string hash) => $"{timestamp.ToString("yyyyMMddHHmmss")}_{hash}";
        private static string GetIndexPartitionKey(string hash) => hash;
        private static string GetIndexRowKey() => string.Empty;

        public HistoryRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _historyStorage = AzureTableStorage<HistoryItemEntity>.Create(connectionStringManager, "StratishHistory", log);
            _indexStorage = AzureTableStorage<IndexEntity>.Create(connectionStringManager, "StratisHistoryIndex", log);
        }

        public async Task<IEnumerable<IHistoryItem>> GetByAddressAsync(ObservationCategory category, string address, string afterHash = null, int take = 100)
        {
            var partitionKey = GetHistoryPartitionKey(category, address);

            if (!string.IsNullOrWhiteSpace(afterHash))
            {
                var index = await _indexStorage.GetDataAsync(GetIndexPartitionKey(afterHash), GetIndexRowKey());
                if (index != null)
                {
                    var rowKey = GetHistoryRowKey(index.TimestampUtc, afterHash);
                    var page = new PagingInfo { ElementCount = take };
                    var query = new TableQuery<HistoryItemEntity>()
                        .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey))
                        .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, rowKey));

                    return await _historyStorage.ExecuteQueryWithPaginationAsync(query, page);
                }
            }

            return await _historyStorage.GetTopRecordsAsync(partitionKey, take);
        }

        public class IndexEntity : TableEntity
        {
            public DateTime TimestampUtc { get; set; }
        }
    }
}
