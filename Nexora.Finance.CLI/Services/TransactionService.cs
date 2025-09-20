using Nexora.Finance.CLI.Data;
using Nexora.Finance.CLI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexora.Finance.CLI.Services
{
    public class TransactionService
    {
        private readonly TransactionRepository _repo;

        public TransactionService(TransactionRepository repo) => _repo = repo;

        public int Add(string descricao, decimal valor, TransactionType tipo, DateTime? data = null)
        {
            if (string.IsNullOrWhiteSpace(descricao)) throw new ArgumentException("Descrição obrigatória");
            if (valor <= 0) throw new ArgumentException("Valor deve ser > 0");

            var t = new Transaction
            {
                Descricao = descricao.Trim(),
                Valor = valor,
                Tipo = tipo,
                Data = data ?? DateTime.Now
            };
            return _repo.Add(t);
        }

        public List<Transaction> GetAll() => _repo.GetAll();

        public void Update(int id, string descricao, decimal valor, TransactionType? tipo = null, DateTime? data = null)
        {
            var current = _repo.GetById(id) ?? throw new ArgumentException("Transação não encontrada");
            current.Descricao = descricao;
            current.Valor = valor;
            if (tipo.HasValue) current.Tipo = tipo.Value;
            if (data.HasValue) current.Data = data.Value;
            _repo.Update(current);
        }

        public bool Delete(int id) => _repo.Delete(id);

        public decimal GetBalance() => _repo.GetBalance();
    }
}
