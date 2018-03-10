using System;
using System.Collections.Generic;

namespace Blockchain.Models 
{
    public class Block
    {
        public long Index {get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public IEnumerable<Transaction> Transactions {get;set;}
        public long Proof {get;set;}
        public string PreviousHash {get;set;}
    }
}