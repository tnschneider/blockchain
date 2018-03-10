using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Blockchain.Models;

namespace Blockchain
{
    public class Blockchain 
    {
        private List<Block> _chain {get;set;} = new List<Block>();

        private List<Transaction> _currentTransactions {get;set;} = new List<Transaction>();

        public Blockchain() => AddBlock(100, "1");

        public Block LastBlock => _chain.Last();

        public List<Block> FullChain => _chain;

        public Block AddBlock(long proof, string previousHash = null)
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

            return block;
        }

        public long AddTransaction(Transaction transaction)
        {
            _currentTransactions.Add(transaction);

            return LastBlock.Index + 1;
        }

        public long ProofOfWork(long lastProof)
        {
            long proof = 0;
            while (!IsValidProof(lastProof, proof))
            {
                proof++;
            }
            return proof;
        }

        public static bool IsValidProof(long lastProof, long proof) 
        {
            var guess = GetSHA256Hash($"{lastProof}{proof}");
            return guess.Substring(guess.Length - 4, 4) == "0000";
        } 
            

        public static string HashBlock(Block block) => GetSHA256Hash(block);

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