using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Stratis.API.Core;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NBitcoin;
using Lykke.Service.BlockchainApi.Contract.Transactions;
namespace Lykke.Service.Stratis.API.Helper
{
    public static class ModelStateExtensions
    {

        public static bool IsValidOperationId(this ModelStateDictionary self, Guid operationId)
        {
            if (operationId == Guid.Empty)
            {
                self.AddModelError(nameof(operationId), "Operation identifier must not be empty GUID");
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool IsValidRequest(this ModelStateDictionary self,
    BuildTransactionWithManyInputsRequest request,
    out (BitcoinAddress from, BitcoinAddress to, Money amount)[] items,
    out Asset asset)
        {
            items = new(BitcoinAddress from, BitcoinAddress to, Money amount)[request.Inputs.Count];
            asset = null;

            if (!self.IsValid)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.AssetId) || !Constants.Assets.TryGetValue(request.AssetId, out asset))
            {
                self.AddModelError(
                    nameof(BuildSingleTransactionRequest.AssetId),
                    "Invalid asset");
            }

            if (!Core.Utils.ValidateAddress(request.ToAddress, out var toAddress))
            {
                self.AddModelError(
                    nameof(BuildSingleTransactionRequest.ToAddress),
                    "Invalid destination adddress");
            }

            for (int i = 0; i < request.Inputs.Count; i++)
            {
                if (Core.Utils.ValidateAddress(request.Inputs[i].FromAddress, out var fromAddress))
                {
                    items[i].from = fromAddress;
                }
                else
                {
                    self.AddModelError(
                        $"{nameof(BuildTransactionWithManyInputsRequest.Inputs)}[{i}].FromAddress",
                        "Invalid sender adddress");
                }

                items[i].to = toAddress;

                if (asset != null)
                {
                    try
                    {
                        var coins = Conversions.CoinsFromContract(request.Inputs[i].Amount, asset.Accuracy);
                        items[i].amount = Money.FromUnit(coins, asset.Unit);
                    }
                    catch (ConversionException ex)
                    {
                        self.AddModelError(
                            $"{nameof(BuildTransactionWithManyInputsRequest.Inputs)}[{i}].Amount",
                            ex.Message);
                    }
                }
            }

            return self.IsValid;
        }

    }
}
