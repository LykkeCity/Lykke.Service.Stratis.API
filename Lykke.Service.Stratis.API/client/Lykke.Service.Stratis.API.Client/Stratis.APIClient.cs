using System;
using Common.Log;

namespace Lykke.Service.Stratis.API.Client
{
    public class StratisAPIClient : IStratisAPIClient, IDisposable
    {
        private readonly ILog _log;

        public StratisAPIClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
