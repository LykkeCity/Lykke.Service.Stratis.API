using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.Stratis.API.Core.Domain.Balance;

namespace Lykke.Service.Stratis.API.Core.Repositories
{
    public interface IBalanceRepository
    {
        Task AddAsync(string address);
       
        Task<IBalance> GetAsync(string address);
        Task DeleteAsync(string address);

    }
}
