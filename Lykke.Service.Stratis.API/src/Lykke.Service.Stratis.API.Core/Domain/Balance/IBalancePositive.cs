using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.Stratis.API.Core.Domain.Balance
{
    public interface IBalancePositive
    {
        string Address { get;  }
        decimal Amount { get; set; }
        long Block { get; set; }
    }
}
