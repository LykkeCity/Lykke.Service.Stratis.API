using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.BlockchainApi.Contract.Assets;
using Lykke.Service.Stratis.API.Core;
using Lykke.Service.Stratis.API.Helper;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Stratis.API.Controllers
{
    [Route("/api/assets")]
    public class AssetsController:Controller
    {

        [HttpGet("{assetId}")]
        public AssetResponse GetAsset(string assetId)
        {
            return Constants.Assets.TryGetValue(assetId, out var asset) ? asset.ToAssetResponse() : null;
        }
    }
}
