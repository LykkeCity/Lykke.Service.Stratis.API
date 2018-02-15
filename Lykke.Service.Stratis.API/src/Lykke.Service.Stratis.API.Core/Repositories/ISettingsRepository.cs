using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.Stratis.API.Core.Settings;

namespace Lykke.Service.Stratis.API.Core.Repositories
{
    public interface ISettingsRepository
    {
        Task<ISettings> GetAsync();

        Task UpsertAsync(ISettings settings);
    }
}
