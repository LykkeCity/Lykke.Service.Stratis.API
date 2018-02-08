using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lykke.Service.BlockchainApi.Contract;
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

        [HttpGet]
        public PaginationResponse<AssetResponse> Get([Required, FromQuery] int take, [FromQuery] string continuation)
        {
            var assets = new[] { Asset.Stratis.ToAssetResponse() };

            return PaginationResponse.From("", assets);
        }
    }
}
