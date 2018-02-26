using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.Stratis.API.Core.Domain.InsightClient
{
    public class BlocksInfo
    {
        public Block[] Blocks { get; set; }
    }

    public class Block
    {
        public long Height { get; set; }
    }
}
