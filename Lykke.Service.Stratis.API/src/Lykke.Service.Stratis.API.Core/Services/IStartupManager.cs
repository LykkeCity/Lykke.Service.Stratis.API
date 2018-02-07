using System.Threading.Tasks;

namespace Lykke.Service.Stratis.API.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}