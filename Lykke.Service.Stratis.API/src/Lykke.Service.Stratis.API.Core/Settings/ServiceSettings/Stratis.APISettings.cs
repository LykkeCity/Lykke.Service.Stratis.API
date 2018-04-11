namespace Lykke.Service.Stratis.API.Core.Settings.ServiceSettings
{
    public class StratisAPISettings:ISettings
    {
        public DbSettings Db { get; set; }
        public string Network { get; set; }

        public decimal Fee { get; set; }
        public string RpcAuthenticationString { get; set; }
        public string NetworkType { get; set; }
        public string RpcUrl { get; set; }
        public int IndexInterval { get; set; }
        public int ConfirmationLevel { get; }
        public string LastBlockHash { get; }
        public decimal FeePerKb { get; }
        public decimal MaxFee { get; }
        public decimal MinFee { get; }
        public bool UseDefaultFee { get; }
        public bool SkipNodeCheck { get; }
    }
}
