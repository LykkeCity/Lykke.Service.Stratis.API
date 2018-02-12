using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.Stratis.API.Core.Domain.Broadcast
{
    public interface IBroadcastInProgress
    {
        Guid OperationId { get; }
        string Hash { get; }
    }
}
