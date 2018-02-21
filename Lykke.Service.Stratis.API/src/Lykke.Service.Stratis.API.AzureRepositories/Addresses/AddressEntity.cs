﻿using System;
using System.Collections.Generic;
using System.Text;
using Lykke.AzureStorage.Tables;
using Lykke.Service.Stratis.API.Core;
using Lykke.Service.Stratis.API.Core.Domain.Addresses;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.Stratis.API.AzureRepositories.Addresses
{
    public class AddressEntity : AzureTableEntity, IAddress
    {
        public AddressEntity()
        {
        }

        public AddressEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        [IgnoreProperty]
        public ObservationCategory ObservationSubject
        {
            get => (ObservationCategory)Enum.Parse(typeof(ObservationCategory), PartitionKey);
        }

        [IgnoreProperty]
        public string Address
        {
            get => RowKey;
        }
    }
}
