using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.Stratis.API.Core.Settings;

namespace Lykke.Service.Stratis.API.PeriodicalHandlers
{
    public class HistoryHandler : TimerPeriod
    {
        private ILog _log;
        private IStratisService _stratisService;

        public HistoryHandler(int trackingInterval, ILog log, IStratisService stratisService) :
            base(nameof(HistoryHandler), trackingInterval, log)
        {
            _log = log;
            _stratisService = stratisService;
        }

        public override async Task Execute()
        {
            try
            {
                await _stratisService.HandleHistoryAsync();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(HistoryHandler), nameof(Execute), null, ex);
            }
        }
    }
}
