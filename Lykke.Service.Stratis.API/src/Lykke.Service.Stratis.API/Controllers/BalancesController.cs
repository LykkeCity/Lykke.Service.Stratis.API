using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Balances;
using Lykke.Service.Stratis.API.AzureRepositories.Balance;
using Lykke.Service.Stratis.API.Core.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.Stratis.API.Models;
using Lykke.Service.Stratis.API.Core.Repositories;
using System.Linq;
using Lykke.Common.ApiLibrary.Contract;
using Lykke.Service.Stratis.API.Core;
using Lykke.Service.Stratis.API.Helper;

namespace Lykke.Service.Stratis.API.Controllers
{
    [Route("/api/balances")]
    public class BalancesController : Controller
    {

        private readonly IStratisService _stratisService;
        private readonly IBalanceRepository _balanceRepository;
        private readonly IBalancePositiveRepository _balancePositiveRepository;

        public BalancesController(IStratisService stratisService,
            IBalanceRepository balanceRepository,
            IBalancePositiveRepository balancePositiveRepository)
        {
            _stratisService = stratisService;
            _balanceRepository = balanceRepository;
            _balancePositiveRepository = balancePositiveRepository;
        }

        //[HttpPost("{address}/observation")]
        //[ProducesResponseType((int)HttpStatusCode.OK)]
        //public async Task<IActionResult> AddToObservations([Required] string address)
        //{
        //    if (string.IsNullOrEmpty(address))
        //    {
        //        return BadRequest(ErrorResponse.Create($"{nameof(address)} is null or empty"));
        //    }

        //    var validAddress = _stratisService.GetBitcoinAddress(address) != null;
        //    if (!validAddress)
        //    {
        //        return BadRequest(ErrorResponse.Create($"{nameof(address)} is not valid"));
        //    }

        //    var balance = await _balanceRepository.GetAsync(address);
        //    if (balance != null)
        //    {
        //        return new StatusCodeResult(StatusCodes.Status409Conflict);
        //    }

        //    await _balanceRepository.AddAsync(address);
        //    await _stratisService.RefreshAddressBalance(address);

        //    return Ok();
        //}

        [HttpPost("{address}/observation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromRoute]string address)
        {
            if (!ModelState.IsValid ||
                !ModelState.IsValidAddress(address))
            {
                return BadRequest(ErrorResponseFactory.Create(ModelState));
            }

            if (await _stratisService.TryCreateObservableAddressAsync(ObservationCategory.Balance, address))
                return Ok();
            else
                return StatusCode(StatusCodes.Status409Conflict);
        }


        //[HttpDelete("{address}/observation")]
        //[ProducesResponseType((int)HttpStatusCode.OK)]
        //public async Task<IActionResult> DeleteFromObservations([Required] string address)
        //{
        //    if (string.IsNullOrEmpty(address))
        //    {
        //        return BadRequest(ErrorResponse.Create($"{nameof(address)} is null or empty"));
        //    }

        //    var balance = await _balanceRepository.GetAsync(address);
        //    if (balance == null)
        //    {
        //        return new StatusCodeResult(StatusCodes.Status204NoContent);
        //    }

        //    await _balanceRepository.DeleteAsync(address);
        //    await _balancePositiveRepository.DeleteAsync(address);

        //    return Ok();
        //}

        [HttpDelete("{address}/observation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> Delete([FromRoute]string address)
        {
            if (!ModelState.IsValid ||
                !ModelState.IsValidAddress(address))
            {
                return BadRequest(ErrorResponseFactory.Create(ModelState));
            }

            if (await _stratisService.TryDeleteObservableAddressAsync(ObservationCategory.Balance, address))
                return Ok();
            else
                return NoContent();
        }

        [HttpGet]
        public async Task<PaginationResponse<WalletBalanceContract>> Get([Required, FromQuery] int take, [FromQuery] string continuation)
        {
            var result = await _balancePositiveRepository.GetAsync(take, continuation);

            return PaginationResponse.From(
                result.Continuation,
                result.Items.Select(f => f.ToWalletBalanceContract()).ToArray()
            );
        }
    }
}
