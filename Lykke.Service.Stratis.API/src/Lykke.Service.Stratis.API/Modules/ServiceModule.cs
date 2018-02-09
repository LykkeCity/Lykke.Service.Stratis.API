using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Service.Stratis.API.Core.Services;
using Lykke.Service.Stratis.API.Core.Settings;
using Lykke.Service.Stratis.API.Core.Settings.ServiceSettings;
using Lykke.Service.Stratis.API.Services;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.Stratis.API.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<StratisAPISettings> _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<StratisAPISettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            // TODO: Do not register entire settings in container, pass necessary settings to services which requires them
            // ex:
            //  builder.RegisterType<QuotesPublisher>()
            //      .As<IQuotesPublisher>()
            //      .WithParameter(TypedParameter.From(_settings.CurrentValue.QuotesPublication))

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            // TODO: Add your dependencies here
            builder.RegisterType<StratisService>()
                            .As<IStratisService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue))
                            .SingleInstance();

            builder.RegisterType<StratisInsightClient>()
                .As<IStratisInsightClient>()
                .WithParameter("url", _settings.CurrentValue.InsightApiUrl)
                .SingleInstance();

            builder.Populate(_services);
        }
    }
}
