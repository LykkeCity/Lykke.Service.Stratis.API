using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Flurl.Http;
using Lykke.Service.Stratis.API.Core.Services;
using Lykke.Service.Stratis.API.Services.Helper;

namespace Lykke.Service.Stratis.API.Services
{
    public class StratisInsightClient:IStratisInsightClient 
    {
        private readonly ILog _log;
        private readonly string _url;

        public StratisInsightClient(ILog log, string url)
        {
            _log = log;
            _url = url;
        }

        public async Task<ulong> GetBalanceSatoshis(string address)
        {
            var url = $"{_url}/addr/{address}/balance";

            try
            {
                return await GetJson<ulong>(url);
            }
            catch (FlurlHttpException ex) when (ex.Call.Response.StatusCode == HttpStatusCode.NotFound)
            {
                return 0;
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(StratisInsightClient), nameof(GetBalanceSatoshis),
                    $"Failed to get json for url='{url}'", ex);

                throw;
            }
        }


        public async Task<TxUnspent[]> GetTxsUnspentAsync(string address)
        {
            var url = $"{_url}/addr/{address}/utxo";

            try
            {
                return await GetJson<TxUnspent[]>(url);
            }
            catch (FlurlHttpException ex) when (ex.Call.Response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(StratisInsightClient), nameof(GetTxsUnspentAsync),
                    $"Failed to get json for url='{url}'", ex);

                throw;
            }
        }
        private async Task<T> GetJson<T>(string url, int tryCount = 3)
        {
            bool NeedToRetryException(Exception ex)
            {
                if (!(ex is FlurlHttpException flurlException))
                {
                    return false;
                }

                var isTimeout = flurlException is FlurlHttpTimeoutException;
                if (isTimeout)
                {
                    return true;
                }

                if (flurlException.Call.HttpStatus == HttpStatusCode.ServiceUnavailable ||
                    flurlException.Call.HttpStatus == HttpStatusCode.InternalServerError)
                {
                    return true;
                }

                return false;
            }

            return await Retry.Try(() => url.GetJsonAsync<T>(), NeedToRetryException, tryCount, _log);
        }
    }
}
