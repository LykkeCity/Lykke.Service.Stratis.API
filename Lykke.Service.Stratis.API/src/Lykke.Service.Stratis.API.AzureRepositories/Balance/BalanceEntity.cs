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
    internal class BalanceEntity : AzureTableEntity, IBalance
    {
        public string Address => RowKey;
    }
}
