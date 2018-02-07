using System.Threading.Tasks;

namespace Lykke.Service.Stratis.API.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}