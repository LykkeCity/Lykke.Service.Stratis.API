using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.Stratis.API.Core.Services
{
    public class TxUnspent
    {
        public string Txid { get; set; }
        public uint Vout { get; set; }
        public string ScriptPubKey { get; set; }
        public decimal Amount { get; set; }
        public ulong Satoshis { get; set; }
        public int Confirmations { get; set; }
    }
}
