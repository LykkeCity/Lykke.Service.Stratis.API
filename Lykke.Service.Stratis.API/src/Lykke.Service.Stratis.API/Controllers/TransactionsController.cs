using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.ApiLibrary.Contract;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Stratis.API.Core;
using Lykke.Service.Stratis.API.Core.Domain.Broadcast;
using Lykke.Service.Stratis.API.Core.Domain.InsightClient;
using Lykke.Service.Stratis.API.Core.Domain.Operations;
using Lykke.Service.Stratis.API.Core.Exceptions;
using Lykke.Service.Stratis.API.Core.Services;
using Lykke.Service.Stratis.API.Core.Settings;
using Lykke.Service.Stratis.API.Helper;
using Lykke.Service.Stratis.API.Models;
using Lykke.Service.Stratis.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using Lykke.Service.Stratis.API.Helper;

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
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> GetBroadcast([Required] Guid operationId)
        {
            var broadcast = await _stratisService.GetBroadcastAsync(operationId);
            if (broadcast == null)
            {
                return NoContent();
            }

            return await Get(operationId, op => op.ToSingleResponse());

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

        [NonAction]
        public async Task<IActionResult> Get<TResponse>(Guid operationId, Func<IOperation, TResponse> toResponse)
        {
            if (!ModelState.IsValid ||
                !ModelState.IsValidOperationId(operationId))
            {
                return BadRequest(ErrorResponseFactory.Create(ModelState));
            }

            var operation = await _stratisService.GetOperationAsync(operationId);
            if (operation != null)
                return Ok(toResponse(operation));
            else
                return NoContent();
        }

        [HttpPost("many-inputs")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BuildTransactionResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BlockchainErrorResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> Build([FromBody]BuildTransactionWithManyInputsRequest request)
        {
            if (!ModelState.IsValid ||
                !ModelState.IsValidRequest(request, out var items, out var asset))
            {
                return BadRequest(ErrorResponseFactory.Create(ModelState));
            }

            return await Get(request.OperationId, op => op.ToManyInputsResponse());
        }

        [NonAction]
        public async Task<IActionResult> Build(Guid operationId, OperationType type, Asset asset, bool subtractFees, params (BitcoinAddress from, BitcoinAddress to, Money amount)[] items)
        {
            var operation = await _stratisService.GetOperationAsync(operationId, false);

            if (operation != null && operation.State != OperationState.Built)
            {
                return StatusCode(StatusCodes.Status409Conflict,
                    ErrorResponse.Create($"Operation is already {Enum.GetName(typeof(OperationState), operation.State).ToLower()}"));
            }

            var signContext = string.Empty;

            try
            {
                signContext = await _stratisService.BuildAsync(operationId, OperationType.SingleFromSingleTo, asset, subtractFees, items);
            }
            catch (DustException)
            {
                return BadRequest(BlockchainErrorResponse.FromKnownError(BlockchainErrorCode.AmountIsTooSmall));
            }
            catch (NotEnoughFundsException)
            {
                return BadRequest(BlockchainErrorResponse.FromKnownError(BlockchainErrorCode.NotEnoughtBalance));
            }

            return Ok(new BuildTransactionResponse
            {
                TransactionContext = signContext.ToBase64()
            });
        }



        [HttpPost("many-outputs")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BuildTransactionResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BlockchainErrorResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> Build([FromBody]BuildTransactionWithManyOutputsRequest request)
        {
            if (!ModelState.IsValid ||
                !ModelState.IsValidRequest(request, out var items, out var asset))
            {
                return BadRequest(ErrorResponseFactory.Create(ModelState));
            }

            // return await Build(request.OperationId, OperationType.SingleFromMultiTo, asset, false, items);
            return await Get(request.OperationId, op => op.ToManyOutputsResponse());
        }


        [HttpGet("broadcast/many-inputs/{operationId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BroadcastedTransactionWithManyInputsResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> GetManyInputs([FromRoute]Guid operationId)
        {
            return await Get(operationId, op => op.ToManyInputsResponse());
        }


        [HttpDelete("history/{category}/{address}/observation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> DeleteObservation(
            [FromRoute]HistoryObservationCategory category,
            [FromRoute]string address)
        {
            if (!ModelState.IsValid ||
                !ModelState.IsValidAddress(address))
            {
                return BadRequest(ErrorResponseFactory.Create(ModelState));
            }

            if (await _stratisService.TryDeleteObservableAddressAsync((ObservationCategory)category, address))
                return Ok();
            else
                return NoContent();
        }


        [HttpGet("history/{category}/{address}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(HistoricalTransactionContract[]))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> GetHistory(
            [FromRoute]HistoryObservationCategory category,
            [FromRoute]string address,
            [FromQuery]string afterHash,
            [FromQuery]int take)
        {
            if (!ModelState.IsValid ||
                !ModelState.IsValidAddress(address))
            {
                return BadRequest(ErrorResponseFactory.Create(ModelState));
            }

            var txs = await _stratisService.GetHistoryAsync((ObservationCategory)category, address, afterHash, take);

            return Ok(txs
                .Select(tx => tx.ToHistoricalContract())
                .ToArray());
        }

        [HttpPost("history/{category}/{address}/observation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Observe(
            [FromRoute]HistoryObservationCategory category,
            [FromRoute]string address)
        {
            if (!ModelState.IsValid ||
                !ModelState.IsValidAddress(address))
            {
                return BadRequest(ErrorResponseFactory.Create(ModelState));
            }

            if (await _stratisService.TryCreateObservableAddressAsync((ObservationCategory)category, address))
                return Ok();
            else
                return StatusCode(StatusCodes.Status409Conflict);
        }

    }

}
