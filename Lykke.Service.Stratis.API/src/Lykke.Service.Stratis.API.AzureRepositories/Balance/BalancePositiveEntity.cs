using System;
using System.Collections.Generic;
using System.Text;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.Stratis.API.Core.Domain.Balance;

namespace Lykke.Service.Stratis.API.AzureRepositories.Balance
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    internal class BalancePositiveEntity : AzureTableEntity, IBalancePositive
    {
        public string Address => RowKey;

        public decimal Amount { get; set; }

        public long Block { get; set; }
    }
}
