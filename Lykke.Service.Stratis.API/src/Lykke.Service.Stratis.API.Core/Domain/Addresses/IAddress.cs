using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.Stratis.API.Core.Domain.Addresses
{
    public interface IAddress
    {
        ObservationCategory ObservationSubject { get; }
        string Address { get; }
    }
}
