using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using Blockchain.Models;
using System.Threading.Tasks;

namespace Blockchain
{
    public class Blockchain 
    {
        private List<Block> _chain = new List<Block>();
        private List<Transaction> _currentTransactions = new List<Transaction>();
        private HashSet<Node> _nodes = new HashSet<Node>();
        private HttpClient _client = new HttpClient();

        public Blockchain() 
        {
            AddBlockAsync(100, "1").Wait();
        }

        public Block LastBlock => _chain.Last();

        public List<Block> FullChain => _chain;

        public Task RegisterNodeAsync(string address)
        {
            _nodes.Add(new Node { Address = address });

            return Task.CompletedTask;
        }

        public async Task<bool> ResolveConflictsAsync()
        {
            IEnumerable<Block> newChain = null;

            long maxLen = _chain.Count;

            foreach (var node in _nodes)
            {
                var resp = await _client.GetAsync(new Uri(new Uri(node.Address), "/api/blockchain/chain"));

                if (resp.IsSuccessStatusCode)
                {
                    var model = JsonConvert.DeserializeObject<ChainResponse>(await resp.Content.ReadAsStringAsync());

                    if (model != null && model.Length > maxLen && IsValidChain(model.Chain))
                    {
                        maxLen = model.Length;
                        newChain = model.Chain;
                    }
                }

                if (newChain != null)
                {
                    _chain = newChain.ToList();
                    return true;
                }
            }

            return false;
            
        }

        public Task<Block> AddBlockAsync(long proof, string previousHash = null)
        {
            var block = new Block {
                Index = _chain.Count + 1,
                Timestamp = DateTimeOffset.Now,
                Transactions = _currentTransactions.ToList(),
                Proof = proof,
                PreviousHash = previousHash ?? HashBlock(LastBlock)
            };

            _chain.Add(block);

            _currentTransactions.Clear();

            return Task.FromResult(block);
        }

        public Task<long> AddTransactionAsync(Transaction transaction)
        {
            _currentTransactions.Add(transaction);

            return Task.FromResult(LastBlock.Index + 1);
        }

        public Task<long> ProofOfWorkAsync(long lastProof)
        {
            long proof = 0;
            while (!IsValidProof(lastProof, proof))
            {
                proof++;
            }
            return Task.FromResult(proof);
        }

        public static string HashBlock(Block block) => GetSHA256Hash(block);

        private static bool IsValidChain(IEnumerable<Block> chain) 
        {
            var chainArr = chain.ToArray();
            
            var index = 0;
            var lastBlock = chainArr[index];

            while (++index < chainArr.Length)
            {
                var block = chainArr[index];
                
                if (block.PreviousHash != HashBlock(lastBlock)) return false;

                if (!IsValidProof(lastBlock.Proof, block.Proof)) return false;

                lastBlock = block;
            }

            return true;
        }

        private static bool IsValidProof(long lastProof, long proof) 
        {
            var guess = GetSHA256Hash($"{lastProof}{proof}");
            return guess.Substring(guess.Length - 4, 4) == "0000";
        }

        private static string GetSHA256Hash(object value) => 
            GetSHA256Hash(JsonConvert.SerializeObject(value));

        private static string GetSHA256Hash(string value)
        {
            var crypt = new SHA256Managed();
            var hashedValue = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(value));

            foreach (var cryptoByte in crypto)
            {
                hashedValue.Append(cryptoByte.ToString("x2"));
            }
            return hashedValue.ToString();
        }        
    }
}