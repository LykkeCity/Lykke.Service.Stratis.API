namespace Lykke.Service.Stratis.API.Core.Settings.ServiceSettings
{
    public class StratisAPISettings
    {
        public DbSettings Db { get; set; }
        public string Network { get; set; }

        public string InsightApiUrl { get; set; }
        public decimal Fee { get; set; }
        public string RpcAuthenticationString { get; set; }
        public string NetworkType { get; set; }
        public string RpcUrl { get; set; }
    }
}
