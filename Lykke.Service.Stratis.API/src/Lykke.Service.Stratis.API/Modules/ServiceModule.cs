﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Service.Stratis.API.AzureRepositories.Addresses;
using Lykke.Service.Stratis.API.AzureRepositories.Balance;
using Lykke.Service.Stratis.API.AzureRepositories.Broadcast;
using Lykke.Service.Stratis.API.AzureRepositories.BroadcastInprogress;
using Lykke.Service.Stratis.API.AzureRepositories.Operations;
using Lykke.Service.Stratis.API.AzureRepositories.Settings;
using Lykke.Service.Stratis.API.Core.Repositories;
using Lykke.Service.Stratis.API.Core.Services;
using Lykke.Service.Stratis.API.Core.Settings;
using Lykke.Service.Stratis.API.Core.Settings.ServiceSettings;
using Lykke.Service.Stratis.API.Services;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using NBitcoin.RPC;

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
            var connectionStringManager = _settings.ConnectionString(x => x.Db.DataConnString);

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

            builder.RegisterType<BroadcastRepository>()
                .As<IBroadcastRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<BroadcastInProgressRepository>()
                .As<IBroadcastInProgressRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<BalanceRepository>()
                .As<IBalanceRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<BalancePositiveRepository>()
                .As<IBalancePositiveRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();


            builder.RegisterType<OperationRepository>()
                .As<IOperationRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<SettingsRepository>()
                           .As<ISettingsRepository>()
                           .WithParameter(TypedParameter.From(connectionStringManager))
                           .SingleInstance();

            builder.RegisterType<AddressRepository>()
                .As<IAddressRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager));



            builder.RegisterInstance(Network.GetNetwork(_settings.CurrentValue.NetworkType))
                .As<Network>();

            builder.RegisterType<RPCClient>()
                .AsSelf()
                .WithParameter("authenticationString", _settings.CurrentValue.RpcAuthenticationString)
                .WithParameter("hostOrUri", _settings.CurrentValue.RpcUrl);

            builder.RegisterType<BlockchainReader>()
                .As<IBlockchainReader>();



            builder.Populate(_services);
        }
    }
}
