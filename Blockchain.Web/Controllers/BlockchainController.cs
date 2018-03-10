using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Blockchain;
using Blockchain.Models;
using Blockchain.Web.Models;

namespace Blockchain.Web.Controllers
{
    [Route("api/[controller]")]
    public class BlockchainController : Controller
    {
        private Blockchain _blockchain;
        private BlockchainNodeSettings _settings;

        public BlockchainController(Blockchain blockchain, BlockchainNodeSettings settings) {
            _blockchain = blockchain;
            _settings = settings;
        }

        [HttpGet("mine")]
        public IActionResult Mine()
        {
            var proof = _blockchain.ProofOfWork(_blockchain.LastBlock.Proof);
            _blockchain.AddTransaction(new Transaction {
                Sender = "0",
                Recipient = _settings.NodeIdentifier,
                Amount = 1
            });

            var block = _blockchain.AddBlock(proof, Blockchain.HashBlock(_blockchain.LastBlock));

            return Ok(block);
        }

        [HttpPost("transactions/new")]
        public IActionResult NewTransaction([FromBody] Transaction transaction) => Ok(_blockchain.AddTransaction(transaction));

        [HttpGet("chain")]
        public IActionResult Chain()
        {
            var chain = _blockchain.FullChain;

            var response = new {
                 Chain = chain,
                 Length = chain.Count
            };

            return Ok(response);
        }
    }
}
