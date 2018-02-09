using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.BlockchainApi.Contract.Assets;
using Lykke.Service.Stratis.API.Core;

namespace Lykke.Service.Stratis.API.Helper
{
    public static class AssetExtensions
    {
        public static AssetResponse ToAssetResponse(this Asset self)
        {
            return new AssetResponse
            {
                Accuracy = self.Accuracy,
                Address = string.Empty,
                AssetId = self.Id,
                Name = self.Id
            };
        }

        public static AssetContract ToAssetContract(this Asset self)
        {
            return ToAssetResponse(self);
        }
    }
}
