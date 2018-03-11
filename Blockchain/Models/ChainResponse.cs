using System.Collections.Generic;

namespace Blockchain.Models
{
    public class ChainResponse
    {
        public IEnumerable<Block> Chain { get; set; }
        public long Length { get; set; }
    }
}