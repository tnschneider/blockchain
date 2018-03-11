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
        public async Task<IActionResult> Mine()
        {
            var proof = await _blockchain.ProofOfWorkAsync(_blockchain.LastBlock.Proof);
            
            await _blockchain.AddTransactionAsync(new Transaction {
                Sender = "0",
                Recipient = _settings.NodeIdentifier,
                Amount = 1
            });

            var block = await _blockchain.AddBlockAsync(proof, Blockchain.HashBlock(_blockchain.LastBlock));

            return Ok(block);
        }

        [HttpPost("transactions/new")]
        public async Task<IActionResult> NewTransaction([FromBody] Transaction transaction) => 
            Ok(await _blockchain.AddTransactionAsync(transaction));

        [HttpGet("chain")]
        public IActionResult Chain()
        {
            var chain = _blockchain.FullChain;

            var response = new ChainResponse {
                 Chain = chain,
                 Length = chain.Count
            };

            return Ok(response);
        }

        [HttpPost("nodes/register")]
        public async Task<IActionResult> RegisterNodes([FromBody] IEnumerable<Node> nodes)
        {
            if (nodes == null) return BadRequest();

            var tasks = nodes.Select(x => _blockchain.RegisterNodeAsync(x.Address));
            
            await Task.WhenAll(tasks);

            return Ok();
        }

        [HttpGet("nodes/resolve")]
        public async Task<IActionResult> Resolve()
        {
            var replaced = await _blockchain.ResolveConflictsAsync();

            return Ok(new ResolveResponse 
                {
                    Chain = _blockchain.FullChain,
                    ChainWasAuthoritative = !replaced
                });
        }
    }
}
