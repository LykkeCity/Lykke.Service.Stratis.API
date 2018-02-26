﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.Stratis.API.Core.Domain.Operations;

namespace Lykke.Service.Stratis.API.Core.Repositories
{
    public interface IOperationRepository
    {
        Task<IOperation> UpsertAsync(Guid operationId, OperationType type, (string fromAddress, string toAddress, decimal amount)[] items,
            decimal fee, bool subtractFee, string assetId);

        Task<IOperation> UpdateAsync(Guid operationId,
            DateTime? sentUtc = null, DateTime? completedUtc = null, DateTime? failedUtc = null, DateTime? deletedUtc = null,
            string hash = null, string error = null);

        Task<IOperation> GetAsync(Guid operationId, bool loadItems = true);

        
    }
}
