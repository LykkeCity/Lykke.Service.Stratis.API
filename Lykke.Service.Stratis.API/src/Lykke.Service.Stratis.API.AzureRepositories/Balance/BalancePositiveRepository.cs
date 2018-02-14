﻿using System;
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
    public class BalancePositiveRepository : IBalancePositiveRepository
    {
        private INoSQLTableStorage<BalancePositiveEntity> _table;
        private static string GetPartitionKey() => "";
        private static string GetRowKey(string address) => address;

        public BalancePositiveRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _table = AzureTableStorage<BalancePositiveEntity>.Create(connectionStringManager, "BalancesPositive", log);
        }

        public async Task SaveAsync(string address, decimal amount, long block)
        {
            await _table.InsertOrReplaceAsync(new BalancePositiveEntity
            {
                PartitionKey = GetPartitionKey(),
                RowKey = GetRowKey(address),
                Amount = amount,
                Block = block
            });
        }

        public async Task DeleteAsync(string address)   
        {
            await _table.DeleteIfExistAsync(GetPartitionKey(), GetRowKey(address));
        }

        public async Task<IBalancePositive> GetAsync(string address)
        {
            return await _table.GetDataAsync(GetPartitionKey(), GetRowKey(address));
        }

        public async Task<(IEnumerable<IBalancePositive> Items, string Continuation)> GetAsync(int take, string continuation)
        {
            var result = await _table.GetDataWithContinuationTokenAsync(GetPartitionKey(), take, continuation);

            return (result.Entities, result.ContinuationToken);
        }
    }
}
