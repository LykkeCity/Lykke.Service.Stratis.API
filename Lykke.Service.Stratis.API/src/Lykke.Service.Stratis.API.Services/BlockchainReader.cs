using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Stratis.API.Services.Models;
using NBitcoin;
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
            var result = await _rpcClient.SendCommandAsync(command, parameters);

            result.ThrowIfError();

            // NBitcoin can not deserialize shielded tx data,
            // that's why custom models are used widely instead of built-in NBitcoin commands;
            // additionaly in case of exception we save context to investigate later:

            try
            {
                return result.Result.ToObject<T>();
            }
            catch (JsonSerializationException jex)
            {
                await _log.WriteErrorAsync(nameof(SendRpcAsync), $"Command: {command}, Response: {result.ResultString}", jex);
                throw;
            }
        }

       
    }

}
