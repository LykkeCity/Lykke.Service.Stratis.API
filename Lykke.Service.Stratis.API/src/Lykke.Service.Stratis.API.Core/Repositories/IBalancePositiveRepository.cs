using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.Stratis.API.Core.Domain.Balance;

namespace Lykke.Service.Stratis.API.Core.Repositories
{
   public interface IBalancePositiveRepository
    {
        Task SaveAsync(string address, decimal amount, long block);
        Task DeleteAsync(string address);
        Task<IBalancePositive> GetAsync(string address);
    }
}
