using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.Stratis.API.Core.Domain.Broadcast;

namespace Lykke.Service.Stratis.API.Core.Repositories
{
    public interface IBroadcastRepository
    {
        Task<IBroadcast> GetAsync(Guid operationId);
        Task AddAsync(Guid operationId, string hash);
        Task AddFailedAsync(Guid operationId, string hash, string error);
    }
}
