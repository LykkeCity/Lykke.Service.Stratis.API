using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Stratis.API.Core;
using Lykke.Service.Stratis.API.Core.Settings;
using Lykke.Service.Stratis.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Stratis.API.Controllers
{
    [Route("api/transactions")]
    public class TransactionsController : Controller
    {
        private readonly IStratisService _stratisService;

        public TransactionsController(ILog log, IStratisService stratisService)
        {
            _stratisService = stratisService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BuildTransactionResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Build([Required, FromBody]  BuildSingleTransactionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponse.Create("ValidationError", ModelState));
            }

            var fromAddress = _stratisService.GetBitcoinAddress(request.FromAddress);
            if (fromAddress == null)
            {
                return BadRequest(ErrorResponse.Create($"{nameof(request.FromAddress)} is not a valid"));
            }

            var toAddress = _stratisService.GetBitcoinAddress(request.ToAddress);
            if (toAddress == null)
            {
                return BadRequest(ErrorResponse.Create($"{nameof(request.ToAddress)} is not a valid"));
            }

            if (request.AssetId != Asset.Stratis.Id)
            {
                return BadRequest(ErrorResponse.Create($"{nameof(request.AssetId)} was not found"));
            }

            var amount = Conversions.CoinsFromContract(request.Amount, Asset.Stratis.Accuracy);
            var fromAddressBalance = await _stratisService.GetAddressBalance(request.FromAddress);
            var fee = _stratisService.GetFee();
            var requiredBalance = request.IncludeFee ? amount : amount + fee;

            if (amount < fee)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable,
                    ErrorResponse.Create($"{nameof(amount)}={amount} can not be less then {fee}"));
            }
            if (requiredBalance > fromAddressBalance)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable,
                    ErrorResponse.Create($"There no enough funds on {nameof(request.FromAddress)}. " +
                    $"Required Balance={requiredBalance}. Available balance={fromAddressBalance}"));
            }

            var transactionContext = await _stratisService.BuildTransactionAsync(request.OperationId, fromAddress,
                toAddress, amount, request.IncludeFee);

            return Ok(new BuildTransactionResponse()
            {
                TransactionContext = transactionContext
            });
        }

       

       

      
       
    }

}
