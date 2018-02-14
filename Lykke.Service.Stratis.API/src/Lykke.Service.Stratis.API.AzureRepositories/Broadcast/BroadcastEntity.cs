﻿using System;
using System.Collections.Generic;
using System.Text;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.Stratis.API.Core.Domain.Broadcast;

namespace Lykke.Service.Stratis.API.AzureRepositories.Broadcast
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    public class BroadcastEntity : AzureTableEntity, IBroadcast
    {
        public Guid OperationId => Guid.Parse(RowKey);

        public BroadcastState State { get; set; }

        public string Hash { get; set; }

        public decimal? Amount { get; set; }

        public decimal? Fee { get; set; }

        public string Error { get; set; }

        public DateTime? BroadcastedUtc { get; set; }

        public DateTime? CompletedUtc { get; set; }

        public DateTime? FailedUtc { get; set; }

        public long Block { get; set; }
    }

}
