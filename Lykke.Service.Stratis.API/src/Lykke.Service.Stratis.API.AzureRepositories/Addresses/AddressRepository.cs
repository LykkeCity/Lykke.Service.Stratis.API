using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.Stratis.API.Core;
using Lykke.Service.Stratis.API.Core.Domain.Addresses;
using Lykke.Service.Stratis.API.Core.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Service.Stratis.API.AzureRepositories.Addresses
{
    public class AddressRepository : IAddressRepository
    {
        private INoSQLTableStorage<AddressEntity> _tableStorage;
        private static string GetPartitionKey(ObservationCategory category) => Enum.GetName(typeof(ObservationCategory), category);
        private static string GetRowKey(string address) => address;

        public AddressRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _tableStorage = AzureTableStorage<AddressEntity>.Create(connectionStringManager, "StratisObservableAddresses", log);
        }

        public async Task DeleteAsync(ObservationCategory category, string address)
        {
            var partitionKKey = GetPartitionKey(category);
            var rowKey = GetRowKey(address);

            await _tableStorage.DeleteAsync(partitionKKey, rowKey);
        }

        public async Task<IAddress> GetAsync(ObservationCategory category, string address)
        {
            var partitionKKey = GetPartitionKey(category);
            var rowKey = GetRowKey(address);

            return await _tableStorage.GetDataAsync(partitionKKey, rowKey);
        }

        public async Task CreateAsync(ObservationCategory category, string address)
        {
            var partitionKey = GetPartitionKey(category);
            var rowKey = GetRowKey(address);

            await _tableStorage.InsertAsync(new AddressEntity(partitionKey, rowKey));
        }

        public async Task<(IEnumerable<IAddress> items, string continuation)> GetByCategoryAsync(ObservationCategory category, string continuation = null, int take = 100)
        {
            return await _tableStorage.GetDataWithContinuationTokenAsync(GetPartitionKey(category), take, continuation);
        }
    }
}
