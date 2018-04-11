using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Stratis.API.Services.Models;
using NBitcoin ;
using NBitcoin.RPC;
using Newtonsoft.Json;

namespace Lykke.Service.Stratis.API.Services
{
    public class BlockchainReader : IBlockchainReader
    {
        private readonly ILog _log;
        private readonly RPCClient _rpcClient;

        public BlockchainReader(ILog log, RPCClient rpcClient)
        {
            _log = log;
            _rpcClient = rpcClient;
        }

        public async Task<Utxo[]> ListUnspentAsync(int confirmationLevel, params string[] addresses)
        {
            return await SendRpcAsync<Utxo[]>(RPCOperations.listunspent, confirmationLevel, int.MaxValue, addresses);
        }
        
        public async Task<T> SendRpcAsync<T>(RPCOperations command, params object[] parameters)
        {
          
            //var comm = command.ToString();
            //var result = await _rpcClient.SendCommandAsync(command, parameters);
            //var result = await _rpcClient.SendCommandAsync(new RPCRequest(comm, parameters), false);
            //int timeout = 10000;
            //var result = _rpcClient.SendCommandAsync(command, parameters);
            //if (await Task.WhenAny(result, Task.Delay(timeout)) == result)
            //{
            //    // Task completed within timeout.
            //    // Consider that the task may have faulted or been canceled.
            //    // We re-await the task so that any exceptions/cancellation is rethrown.
            //    await result;
            //}
            //else
            //{
            //result.ThrowIfError();
            //    // timeout/cancellation logic
            //}

            // NBitcoin can not deserialize shielded tx data,
            // that's why custom models are used widely instead of built-in NBitcoin commands;
            // additionaly in case of exception we save context to investigate later:

            try
            {
                Network rpcNetwork = Network.StratisTest;
                NetworkCredential credentials = new NetworkCredential("stratisuser", "lykkelykke");
                RPCClient rpc = new RPCClient(credentials, new Uri("http://51.144.161.23:5333"), rpcNetwork);

                var result = await rpc.SendCommandAsync(new RPCRequest(command.ToString(), parameters), false);

                result.ThrowIfError();
                return result.Result.ToObject<T>();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(SendRpcAsync), $"Command: {command}, Response: {ex.Message}", ex);
                throw;
            }
            //catch (JsonSerializationException jex)
            //{
            //    await _log.WriteErrorAsync(nameof(SendRpcAsync), $"Command: {command}, Response: {result.ResultString}", jex);
            //    throw;
            //}
        }

        public async Task ImportAddressAsync(string address)
        {
            await SendRpcAsync(RPCOperations.importaddress, address, string.Empty, false);
        }

        public async Task<AddressInfo> ValidateAddressAsync(string address)
        {
            return await SendRpcAsync<AddressInfo>(RPCOperations.validateaddress, address);
        }

        public async Task<RPCResponse> SendRpcAsync(RPCOperations command, params object[] parameters)
        {
            var commandName = Enum.GetName(typeof(RPCOperations), command);
            var result = await _rpcClient.SendCommandAsync(new RPCRequest(commandName , parameters), false);

            result.ThrowIfError();

            return result;
        }

        public async Task<string> SendRawTransactionAsync(Transaction transaction)
        {
            return (await SendRpcAsync(RPCOperations.sendrawtransaction, transaction.ToHex())).ResultString;
        }

        public async Task<RawTransaction> GetRawTransactionAsync(string hash)
        {
            return await SendRpcAsync<RawTransaction>(RPCOperations.getrawtransaction, hash, 1);
        }

        public async Task<string[]> GetAddresssesAsync()
        {
            return await SendRpcAsync<string[]>(RPCOperations.getaddressesbyaccount, string.Empty);
        }

    }

}
