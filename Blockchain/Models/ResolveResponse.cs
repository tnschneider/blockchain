using System.Collections.Generic;

namespace Blockchain.Models
{
    public class ResolveResponse
    {
        public IEnumerable<Block> Chain { get; set; }
        public bool ChainWasAuthoritative { get; set; }
    }
}