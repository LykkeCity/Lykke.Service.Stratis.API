using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Stratis.API.Core;
using Lykke.Service.Stratis.API.Core.Domain.Broadcast;
using Lykke.Service.Stratis.API.Core.Domain.InsightClient;
using Lykke.Service.Stratis.API.Core.Services;
using Lykke.Service.Stratis.API.Core.Settings;
using Lykke.Service.Stratis.API.Helper;
using Lykke.Service.Stratis.API.Models;
using Lykke.Service.Stratis.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;

namespace Lykke.Service.Stratis.API.Controllers
{
    [Route("api/transactions")]
    public class TransactionsController : Controller
    {
        private readonly IStratisService _stratisService;
        private readonly ILog _log;
        private readonly IStratisInsightClient _stratisInsightClient;

        public TransactionsController(ILog log, IStratisService stratisService, IStratisInsightClient stratisInsightClient)
        {
            _log = log;
            _stratisInsightClient = stratisInsightClient;
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


        [HttpPost("broadcast")]
        public async Task<IActionResult> Broadcast([Required, FromBody] BroadcastTransactionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponse.Create("ValidationError", ModelState));
            }

            var broadcast = await _stratisService.GetBroadcastAsync(request.OperationId);
            if (broadcast != null)
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            var transaction = _stratisService.GetTransaction(request.SignedTransaction);
            if (transaction == null)
            {
                return BadRequest(ErrorResponse.Create($"{nameof(request.SignedTransaction)} is not a valid"));
            }

            await _stratisService.BroadcastAsync(transaction, request.OperationId);

            return Ok();
        }

        [HttpGet("broadcast/single/{operationId}")]
        [ProducesResponseType(typeof(BroadcastedSingleTransactionResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBroadcast([Required] Guid operationId)
        {
            var broadcast = await _stratisService.GetBroadcastAsync(operationId);
            if (broadcast == null)
            {
                return NoContent();
            }

            var amount = broadcast.Amount.HasValue ?
                Conversions.CoinsToContract(broadcast.Amount.Value, Asset.Stratis.Accuracy) : "";

            var fee = broadcast.Fee.HasValue ?
                Conversions.CoinsToContract(broadcast.Fee.Value, Asset.Stratis.Accuracy) : "";

            return Ok(new BroadcastedSingleTransactionResponse
            {
                OperationId = broadcast.OperationId,
                Hash = broadcast.Hash,
                State = broadcast.State.ToBroadcastedTransactionState(),
                Amount = amount,
                Fee = fee,
                Error = broadcast.Error,
                Timestamp = broadcast.GetTimestamp(),
                Block = broadcast.Block
            });
        }

        [HttpDelete("broadcast/{operationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteBroadcast([Required] Guid operationId)
        {
            var broadcast = await _stratisService.GetBroadcastAsync(operationId);
            if (broadcast == null)
            {
                return NoContent();
            }

            await _stratisService.DeleteBroadcastAsync(broadcast);

            return Ok();
        }

    }

}
