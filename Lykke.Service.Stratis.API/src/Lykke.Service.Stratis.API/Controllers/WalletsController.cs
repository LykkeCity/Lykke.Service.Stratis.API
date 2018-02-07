using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.Stratis.API.Core.Services;
using Lykke.Service.Stratis.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Stratis.API.Controllers
{
    [Route("api/wallets")]
    public class WalletsController : Controller
    {
        private readonly IStratisService _stratisService;

        public WalletsController(IStratisService stratisService)
        {
            _stratisService = stratisService;
        }

        [HttpPost]
        public WalletResponse Post()
        {
            var privateKey = _stratisService.GetPrivateKey();
            var publicAddress = _stratisService.GetPublicAddress(privateKey);

            return new WalletResponse()
            {
                PrivateKey = privateKey,
                PublicAddress = publicAddress
            };
        }
    }
}
