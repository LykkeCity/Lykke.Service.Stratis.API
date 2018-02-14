using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.Stratis.API.Core.Repositories
{
    public interface IBroadcastInProgressRepository
    {
        Task AddAsync(Guid operationId, string hash);
        Task DeleteAsync(Guid operationId);
    }
}
