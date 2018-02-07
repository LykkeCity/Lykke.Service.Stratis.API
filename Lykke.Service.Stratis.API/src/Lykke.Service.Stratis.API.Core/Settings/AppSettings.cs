using Lykke.Service.Stratis.API.Core.Settings.ServiceSettings;
using Lykke.Service.Stratis.API.Core.Settings.SlackNotifications;

namespace Lykke.Service.Stratis.API.Core.Settings
{
    public class AppSettings
    {
        public StratisAPISettings StratisAPIService { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
