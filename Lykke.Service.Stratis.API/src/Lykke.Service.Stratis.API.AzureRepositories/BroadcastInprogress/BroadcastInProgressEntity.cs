using System;
using System.Collections.Generic;
using System.Text;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.Stratis.API.Core.Domain.Broadcast;

namespace Lykke.Service.Stratis.API.AzureRepositories.BroadcastInprogress
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    internal class BroadcastInProgressEntity : AzureTableEntity, IBroadcastInProgress
    {
        public Guid OperationId
        {
            get => Guid.Parse(RowKey);
        }

        public string Hash { get; set; }
    }
}
