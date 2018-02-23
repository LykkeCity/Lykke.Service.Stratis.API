using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.Stratis.API.Core.Repositories;
using Lykke.Service.Stratis.API.Core.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.Stratis.API.AzureRepositories.Settings
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly INoSQLTableStorage<SettingsEntity> _tableStorage;
        private static string GetPartitionKey() => "Settings";
        private static string GetRowKey() => "";

        public SettingsRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _tableStorage = AzureTableStorage<SettingsEntity>.Create(connectionStringManager, "StratisSettings", log);
        }

        public async Task<ISettings> GetAsync()
        {
            return await _tableStorage.GetDataAsync(GetPartitionKey(), GetRowKey());
        }

        public async Task UpsertAsync(ISettings settings)
        {
            var entity = new SettingsEntity(GetPartitionKey(), GetRowKey())
            {
                ConfirmationLevel = settings.ConfirmationLevel,
                LastBlockHash = settings.LastBlockHash,
                FeePerKb = settings.FeePerKb,
                MaxFee = settings.MaxFee,
                MinFee = settings.MinFee,
                UseDefaultFee = settings.UseDefaultFee
            };

            await _tableStorage.InsertOrReplaceAsync(entity);
        }
    }
}
