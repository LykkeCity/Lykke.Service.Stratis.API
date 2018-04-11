using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Service.Stratis.API.AzureRepositories.Addresses;
using Lykke.Service.Stratis.API.AzureRepositories.Balance;
using Lykke.Service.Stratis.API.AzureRepositories.Broadcast;
using Lykke.Service.Stratis.API.AzureRepositories.BroadcastInprogress;
using Lykke.Service.Stratis.API.AzureRepositories.History;
using Lykke.Service.Stratis.API.AzureRepositories.Operations;
using Lykke.Service.Stratis.API.AzureRepositories.Settings;
using Lykke.Service.Stratis.API.Core.Domain.History;
using Lykke.Service.Stratis.API.Core.Repositories;
using Lykke.Service.Stratis.API.Core.Services;
using Lykke.Service.Stratis.API.Core.Settings;
using Lykke.Service.Stratis.API.Core.Settings.ServiceSettings;
using Lykke.Service.Stratis.API.PeriodicalHandlers;
using Lykke.Service.Stratis.API.Services;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.Crypto;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
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

            //builder.RegisterType<StratisInsightClient>()
            //    .As<IStratisInsightClient>()
            //    .WithParameter("url", _settings.CurrentValue.InsightApiUrl)
            //    .SingleInstance();

            builder.RegisterType<BroadcastRepository>()
                .As<IBroadcastRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            //builder.RegisterType<BroadcastInProgressRepository>()
            //    .As<IBroadcastInProgressRepository>()rotected override void Load(ContainerBuilder builder)
            //    .WithParameter(TypedParameter.From(connectionStringManager))
            //    .SingleInstance();

            //builder.RegisterType<BalanceRepository>()
            //    .As<IBalanceRepository>()
            //    .WithParameter(TypedParameter.From(connectionStringManager))
            //    .SingleInstance();

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

            builder.RegisterType<HistoryRepository>()
                .As<IHistoryRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();
            //NBitcoin.Network.TestNet..EnsureRegistered();

             

            Network network = Network.StratisTest;
            
            //if (config.IsTestNet)
            //{
            //    network = Network.TestNet;
            //}

            //RPCHelper stratisHelper = null;
            //RPCClient stratisRpc = null;
            //BitcoinSecret privateKeyEcdsa = null;

            //try
            //{
            //    stratisHelper = new RPCHelper(network);
            //    stratisRpc = stratisHelper.GetClient(config.RpcUser, config.RpcPassword, config.RpcUrl);
            //    privateKeyEcdsa = stratisRpc.DumpPrivKey(BitcoinAddress.Create(config.TumblerEcdsaKeyAddress));
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("ERROR: Unable to retrieve private key to fund registration transaction");
            //    Console.WriteLine("Is the wallet unlocked & RPC enabled?");
            //    Console.WriteLine(e);
            //    Environment.Exit(0);
            //}

            builder.RegisterInstance(network)
                .As<Network>();

            //builder.RegisterType<RPCClient>()
            //    .AsSelf()
            //    .WithParameter("authenticationString", _settings.CurrentValue.RpcAuthenticationString)
            //    .WithParameter("hostOrUri", _settings.CurrentValue.RpcUrl);

            builder.RegisterType<RPCClient>()
                .AsSelf()
                .WithParameter("authenticationString", _settings.CurrentValue.RpcAuthenticationString)
                .WithParameter("hostOrUri", _settings.CurrentValue.RpcUrl)
                .WithParameter("network", network);

            builder.RegisterType<BlockchainReader>()
                .As<IBlockchainReader>();

            //RegisterPeriodicalHandlers(builder);

            builder.Populate(_services);
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<HistoryHandler>()
                .As<IStartable>()
                .AutoActivate()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.IndexInterval))
                .SingleInstance();
        }
    }



    public class StratisNetworks
    {
        private static Tuple<byte[], int>[] pnSeed6_main = { };
        private static Tuple<byte[], int>[] pnSeed6_test = { };
        private static Network _mainnet;
        private static Network _testnet;
        private static object _sync = new object();

        private static uint256 GetPoWHash(BlockHeader header)
        {
            var headerBytes = header.ToBytes();
            var h = SCrypt.ComputeDerivedKey(headerBytes, headerBytes, 1024, 1, 1, null, 32);
            return new uint256(h);
        }

        private static IEnumerable<NetworkAddress> ToSeed(Tuple<byte[], int>[] tuples)
        {
            return tuples
                .Select(t => new NetworkAddress(new IPAddress(t.Item1), t.Item2))
                .ToArray();
        }

        private static Network RegisterMainnet()
        {
            lock (_sync)
            {
                if (_mainnet == null)
                {
                    _mainnet = new NetworkBuilder()
                        .SetConsensus(new Consensus()
                        {
                            SubsidyHalvingInterval = 840000,
                            MajorityEnforceBlockUpgrade = 750,
                            MajorityRejectBlockOutdated = 950,
                            MajorityWindow = 4000,
                            BIP34Hash = new uint256("fa09d204a83a768ed5a7c8d441fa62f2043abf420cff1226c7b4329aeb9d51cf"),
                            PowLimit = new Target(new uint256("0007ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                            PowTargetTimespan = TimeSpan.FromSeconds(3.5 * 24 * 60 * 60),
                            PowTargetSpacing = TimeSpan.FromSeconds(2.5 * 60),
                            PowAllowMinDifficultyBlocks = false,
                            PowNoRetargeting = false,
                            RuleChangeActivationThreshold = 6048,
                            MinerConfirmationWindow = 8064,
                            CoinbaseMaturity = 100,
                            HashGenesisBlock = new uint256("0x00040fe8ec8471911baa1db1266ea15dd06b4a8a5c453883c000b031973dce08"),
                            GetPoWHash = GetPoWHash,
                            LitecoinWorkCalculation = true
                        })
                        .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 0x1C, 0xB8 })
                        .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 0x1C, 0xBD })
                        .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 0x80 })
                        .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x88, 0xB2, 0x1E })
                        .SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x88, 0xAD, 0xE4 })
                        .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("zec"))
                        .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("zec"))
                        .SetMagic(0xdbb6c0fb)
                        .SetPort(8233)
                        .SetRPCPort(8232)
                        .SetName("stratis-mainnet")
                        .AddDNSSeeds(new[]
                        {
                                new DNSSeedData("stra.tis", "dnsseed.stra.tis")
                        })
                        .SetGenesis(new Block(new BlockHeader()
                        {
                            BlockTime = DateTimeOffset.FromUnixTimeSeconds(1477641360),
                            Nonce = new uint256("0x0000000000000000000000000000000000000000000000000000000000001257").GetLow32(),
                        }))
                        .AddSeeds(ToSeed(pnSeed6_main))
                        .BuildAndRegister();
                }

                return _mainnet;
            }
        }

        /// <summary> Bitcoin maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        public const int BitcoinMaxTimeOffsetSeconds = 70 * 60;

        /// <summary> Stratis maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        public const int StratisMaxTimeOffsetSeconds = 25 * 60;

        /// <summary> Bitcoin default value for the maximum tip age in seconds to consider the node in initial block download (24 hours). </summary>
        public const int BitcoinDefaultMaxTipAgeInSeconds = 24 * 60 * 60;

        /// <summary> Stratis default value for the maximum tip age in seconds to consider the node in initial block download (2 hours). </summary>
        public const int StratisDefaultMaxTipAgeInSeconds = 2 * 60 * 60;

        /// <summary> The name of the root folder containing the different Bitcoin blockchains (Main, TestNet, RegTest). </summary>
        public const string BitcoinRootFolderName = "bitcoin";

        /// <summary> The default name used for the Bitcoin configuration file. </summary>
        public const string BitcoinDefaultConfigFilename = "bitcoin.conf";

        /// <summary> The name of the root folder containing the different Stratis blockchains (StratisMain, StratisTest, StratisRegTest). </summary>
        public const string StratisRootFolderName = "stratis";

        /// <summary> The default name used for the Stratis configuration file. </summary>
        public const string StratisDefaultConfigFilename = "stratis.conf";

        //private static Network RegisterTestnet()
        //{
        //    lock (_sync)
        //    {
        //        if (_testnet == null)
        //        {
                   
                     
        //            var consensus = Network.StratisMain.Consensus.Clone();
        //            consensus.PowLimit = new Target(uint256.Parse("0000ffff00000000000000000000000000000000000000000000000000000000"));

        //            // The message start string is designed to be unlikely to occur in normal data.
        //            // The characters are rarely used upper ASCII, not valid as UTF-8, and produce
        //            // a large 4-byte int at any alignment.
        //            var messageStart = new byte[4];
        //            messageStart[0] = 0x71;
        //            messageStart[1] = 0x31;
        //            messageStart[2] = 0x21;
        //            messageStart[3] = 0x11;
        //            var magic = BitConverter.ToUInt32(messageStart, 0); //0x5223570; 

        //            var genesis = Network.StratisMain.GetGenesis();
        //            genesis.Header.Time = 1493909211;
        //            genesis.Header.Nonce = 2433759;
        //            genesis.Header.Bits = consensus.PowLimit;
        //            consensus.HashGenesisBlock = genesis.GetHash(consensus.NetworkOptions);

        //            Assert(consensus.HashGenesisBlock == uint256.Parse("0x00000e246d7b73b88c9ab55f2e5e94d9e22d471def3df5ea448f5576b1d156b9"));

        //            consensus.DefaultAssumeValid = new uint256("0x12ae16993ce7f0836678f225b2f4b38154fa923bd1888f7490051ddaf4e9b7fa"); // 218810

        //            var builder = new NetworkBuilder()
        //                .SetName("StratisTest")
        //                .SetRootFolderName(StratisRootFolderName)
        //                .SetDefaultConfigFilename(StratisDefaultConfigFilename)
        //                .SetConsensus(consensus)
        //                .SetMagic(magic)
        //                .SetGenesis(genesis)
        //                .SetPort(26178)
        //                .SetRPCPort(26174)
        //                .SetMaxTimeOffsetSeconds(StratisMaxTimeOffsetSeconds)
        //                .SetMaxTipAge(StratisDefaultMaxTipAgeInSeconds)
        //                .SetTxFees(10000, 60000, 10000)
        //                .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { (65) })
        //                .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { (196) })
        //                .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { (65 + 128) })
        //                .SetBase58Bytes(Base58Type.ENCRYPTED_SECRET_KEY_NO_EC, new byte[] { 0x01, 0x42 })
        //                .SetBase58Bytes(Base58Type.ENCRYPTED_SECRET_KEY_EC, new byte[] { 0x01, 0x43 })
        //                .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x04), (0x88), (0xB2), (0x1E) })
        //                .SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x04), (0x88), (0xAD), (0xE4) })

        //                .AddDNSSeeds(new[]
        //                {
        //            new DNSSeedData("testnet1.stratisplatform.com", "testnet1.stratisplatform.com"),
        //            new DNSSeedData("testnet2.stratisplatform.com", "testnet2.stratisplatform.com"),
        //            new DNSSeedData("testnet3.stratisplatform.com", "testnet3.stratisplatform.com"),
        //            new DNSSeedData("testnet4.stratisplatform.com", "testnet4.stratisplatform.com")
        //                });

        //            builder.AddSeeds(new[]
        //            {
        //        new NetworkAddress(IPAddress.Parse("51.140.231.125"), builder.Port), // danger cloud node
        //        new NetworkAddress(IPAddress.Parse("13.70.81.5"), 3389), // beard cloud node  
        //        new NetworkAddress(IPAddress.Parse("191.235.85.131"), 3389), // fassa cloud node  
        //        new NetworkAddress(IPAddress.Parse("52.232.58.52"), 26178), // neurosploit public node
        //    });

        //            _testnet = builder.BuildAndRegister();
        //        }

        //        return _testnet;
        //    }
        //}


        //private static Network InitStratisMain()
        //{
        //    Block.BlockSignature = true;
        //    Transaction.TimeStamp = true;

        //    var consensus = new Consensus();

        //    consensus.NetworkOptions = new NetworkOptions() { IsProofOfStake = true };
        //    consensus.GetPoWHash = (n, h) => Crypto.HashX13.Instance.Hash(h.ToBytes(options: n));

        //    consensus.SubsidyHalvingInterval = 210000;
        //    consensus.MajorityEnforceBlockUpgrade = 750;
        //    consensus.MajorityRejectBlockOutdated = 950;
        //    consensus.MajorityWindow = 1000;
        //    consensus.BuriedDeployments[BuriedDeployments.BIP34] = 227931;
        //    consensus.BuriedDeployments[BuriedDeployments.BIP65] = 388381;
        //    consensus.BuriedDeployments[BuriedDeployments.BIP66] = 363725;
        //    consensus.BIP34Hash = new uint256("0x000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8");
        //    consensus.PowLimit = new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));
        //    consensus.PowTargetTimespan = TimeSpan.FromSeconds(14 * 24 * 60 * 60); // two weeks
        //    consensus.PowTargetSpacing = TimeSpan.FromSeconds(10 * 60);
        //    consensus.PowAllowMinDifficultyBlocks = false;
        //    consensus.PowNoRetargeting = false;
        //    consensus.RuleChangeActivationThreshold = 1916; // 95% of 2016
        //    consensus.MinerConfirmationWindow = 2016; // nPowTargetTimespan / nPowTargetSpacing

        //    consensus.BIP9Deployments[BIP9Deployments.TestDummy] = new BIP9DeploymentsParameters(28, 1199145601, 1230767999);
        //    consensus.BIP9Deployments[BIP9Deployments.CSV] = new BIP9DeploymentsParameters(0, 1462060800, 1493596800);
        //    consensus.BIP9Deployments[BIP9Deployments.Segwit] = new BIP9DeploymentsParameters(1, 0, 0);

        //    consensus.LastPOWBlock = 12500;

        //    consensus.ProofOfStakeLimit = new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false));
        //    consensus.ProofOfStakeLimitV2 = new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false));

        //    consensus.CoinType = 105;

        //    consensus.DefaultAssumeValid = new uint256("0x8c2cf95f9ca72e13c8c4cdf15c2d7cc49993946fb49be4be147e106d502f1869"); // 642930

        //    Block genesis = CreateStratisGenesisBlock(1470467000, 1831645, 0x1e0fffff, 1, Money.Zero);
        //    consensus.HashGenesisBlock = genesis.GetHash(consensus.NetworkOptions);

        //    // The message start string is designed to be unlikely to occur in normal data.
        //    // The characters are rarely used upper ASCII, not valid as UTF-8, and produce
        //    // a large 4-byte int at any alignment.
        //    var messageStart = new byte[4];
        //    messageStart[0] = 0x70;
        //    messageStart[1] = 0x35;
        //    messageStart[2] = 0x22;
        //    messageStart[3] = 0x05;
        //    var magic = BitConverter.ToUInt32(messageStart, 0); //0x5223570; 
             
        //    var builder = new NetworkBuilder()
        //        .SetName("StratisMain")
        //        .SetRootFolderName(StratisRootFolderName)
        //        .SetDefaultConfigFilename(StratisDefaultConfigFilename)
        //        .SetConsensus(consensus)
        //        .SetMagic(magic)
        //        .SetGenesis(genesis)
        //        .SetPort(16178)
        //        .SetRPCPort(16174)
                
        //        .SetTxFees(10000, 60000, 10000)
        //        .SetMaxTimeOffsetSeconds(StratisMaxTimeOffsetSeconds)
        //        .SetMaxTipAge(StratisDefaultMaxTipAgeInSeconds)

        //        .AddDNSSeeds(new[]
        //        {
        //            new DNSSeedData("seednode1.stratisplatform.com", "seednode1.stratisplatform.com"),
        //            new DNSSeedData("seednode2.stratis.cloud", "seednode2.stratis.cloud"),
        //            new DNSSeedData("seednode3.stratisplatform.com", "seednode3.stratisplatform.com"),
        //            new DNSSeedData("seednode4.stratis.cloud", "seednode4.stratis.cloud")
        //        })

        //        .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { (63) })
        //        .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { (125) })
        //        .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { (63 + 128) })
        //        .SetBase58Bytes(Base58Type.ENCRYPTED_SECRET_KEY_NO_EC, new byte[] { 0x01, 0x42 })
        //        .SetBase58Bytes(Base58Type.ENCRYPTED_SECRET_KEY_EC, new byte[] { 0x01, 0x43 })
        //        .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x04), (0x88), (0xB2), (0x1E) })
        //        .SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x04), (0x88), (0xAD), (0xE4) })
        //        .SetBase58Bytes(Base58Type.PASSPHRASE_CODE, new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 })
        //        .SetBase58Bytes(Base58Type.CONFIRMATION_CODE, new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A })
        //        .SetBase58Bytes(Base58Type.STEALTH_ADDRESS, new byte[] { 0x2a })
        //        .SetBase58Bytes(Base58Type.ASSET_ID, new byte[] { 23 })
        //        .SetBase58Bytes(Base58Type.COLORED_ADDRESS, new byte[] { 0x13 })
        //        .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, "bc")
        //        .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, "bc");

        //    var seed = new[] { "101.200.198.155", "103.24.76.21", "104.172.24.79" };
        //    var fixedSeeds = new List<NetworkAddress>();
        //    // Convert the pnSeeds array into usable address objects.
        //    Random rand = new Random();
        //    TimeSpan oneWeek = TimeSpan.FromDays(7);
        //    for (int i = 0; i < seed.Length; i++)
        //    {
        //        // It'll only connect to one or two seed nodes because once it connects,
        //        // it'll get a pile of addresses with newer timestamps.                
        //        NetworkAddress addr = new NetworkAddress();
        //        // Seed nodes are given a random 'last seen time' of between one and two
        //        // weeks ago.
        //        addr.Time = DateTime.UtcNow - (TimeSpan.FromSeconds(rand.NextDouble() * oneWeek.TotalSeconds)) - oneWeek;
        //        addr.Endpoint = Utils.ParseIpEndpoint(seed[i], builder.Port);
        //        fixedSeeds.Add(addr);
        //    }

        //    builder.AddSeeds(fixedSeeds);
        //    return builder.BuildAndRegister();
        //}

        //public static void Register()
        //{
        //    if (_mainnet == null)
        //    {
        //        RegisterMainnet();
        //    }

        //    if (_testnet == null)
        //    {
        //        RegisterTestnet();
        //    }
        //}

        //public static Network Mainnet
        //{
        //    get
        //    {
        //        return _mainnet ?? RegisterMainnet();
        //    }
        //}

        //public static Network Testnet
        //{
        //    get
        //    {
        //        return _testnet ?? RegisterTestnet();
        //    }
        //}
    }
}
