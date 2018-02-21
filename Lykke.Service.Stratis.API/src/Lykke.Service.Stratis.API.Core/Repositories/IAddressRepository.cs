using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.Stratis.API.Core.Domain.Addresses;

namespace Lykke.Service.Stratis.API.Core.Repositories
{
    public interface IAddressRepository
    {

        Task DeleteAsync(ObservationCategory category, string address);
        Task<IAddress> GetAsync(ObservationCategory category, string address);
            
    }
}
