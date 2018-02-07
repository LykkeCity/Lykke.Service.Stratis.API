using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.Stratis.API.Core.Services
{
    public interface IStratisService
    {
        bool IsValidPrivateKey(string privateKey);
        string GetPrivateKey();
        string GetPublicAddress(string privateKey);
       
    }
}
