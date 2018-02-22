using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.Stratis.API.Core.Domain.History
{
    public interface IHistoryRepository
    {

        Task<IEnumerable<IHistoryItem>> GetByAddressAsync(ObservationCategory category, string address, string afterHash = null, int take = 100);
    }
}
