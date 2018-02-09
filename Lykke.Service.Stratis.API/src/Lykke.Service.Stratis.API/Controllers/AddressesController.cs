using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using Lykke.Service.BlockchainApi.Contract.Addresses;
using Lykke.Service.Stratis.API.Core.Settings;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Stratis.API.Controllers
{

    [Route("api/addresses")]
    public class AddressesController : Controller
    {
        private readonly IStratisService _stratisService;

        public AddressesController(IStratisService stratisService)
        {
            _stratisService = stratisService;
        }

        [HttpGet("{address}/validity")]
        [ProducesResponseType(typeof(AddressValidationResponse), (int)HttpStatusCode.OK)]
        public IActionResult GetAddressValidity([Required] string address)
        {
            return Ok(new AddressValidationResponse()
            {
                IsValid = _stratisService.GetBitcoinAddress(address) != null
            });
        }
    }
}
