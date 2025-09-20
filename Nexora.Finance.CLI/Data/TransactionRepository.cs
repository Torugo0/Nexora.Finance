using Nexora.Finance.CLI.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexora.Finance.CLI.Data
{
    public class TransactionRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public TransactionRepository(SqliteConnectionFactory factory) => _factory = factory;

        public int Add(Transaction t)
        {
            using var conn = _factory.Create();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =
            """
            INSERT INTO Transacoes (Descricao, Valor, Tipo, Data)
            VALUES ($d, $v, $t, $dt);
            SELECT last_insert_rowid();
            """;
            cmd.Parameters.AddWithValue("$d", t.Descricao);
            cmd.Parameters.AddWithValue("$v", t.Valor);
            cmd.Parameters.AddWithValue("$t", (int)t.Tipo);
            cmd.Parameters.AddWithValue("$dt", t.Data.ToString("O", CultureInfo.InvariantCulture)); // ISO 8601

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<Transaction> GetAll()
        {
            using var conn = _factory.Create();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Descricao, Valor, Tipo, Data FROM Transacoes ORDER BY Data DESC, Id DESC";

            var list = new List<Transaction>();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Transaction
                {
                    Id = r.GetInt32(0),
                    Descricao = r.GetString(1),
                    Valor = r.GetDecimal(2),
                    Tipo = (TransactionType)r.GetInt32(3),
                    Data = DateTime.Parse(r.GetString(4), null, DateTimeStyles.RoundtripKind)
                });
            }
            return list;
        }

        public Transaction? GetById(int id)
        {
            using var conn = _factory.Create();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Descricao, Valor, Tipo, Data FROM Transacoes WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new Transaction
            {
                Id = r.GetInt32(0),
                Descricao = r.GetString(1),
                Valor = r.GetDecimal(2),
                Tipo = (TransactionType)r.GetInt32(3),
                Data = DateTime.Parse(r.GetString(4), null, DateTimeStyles.RoundtripKind)
            };
        }

        public void Update(Transaction t)
        {
            using var conn = _factory.Create();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =
            """
            UPDATE Transacoes
               SET Descricao = $d, Valor = $v, Tipo = $t, Data = $dt
             WHERE Id = $id;
            """;
            cmd.Parameters.AddWithValue("$d", t.Descricao);
            cmd.Parameters.AddWithValue("$v", t.Valor);
            cmd.Parameters.AddWithValue("$t", (int)t.Tipo);
            cmd.Parameters.AddWithValue("$dt", t.Data.ToString("O"));
            cmd.Parameters.AddWithValue("$id", t.Id);

            cmd.ExecuteNonQuery();
        }

        public bool Delete(int id)
        {
            using var conn = _factory.Create();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Transacoes WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);

            var rows = cmd.ExecuteNonQuery();
            return rows > 0; // true = excluiu, false = não achou
        }

        public decimal GetBalance()
        {
            using var conn = _factory.Create();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =
            """
            SELECT 
              COALESCE(SUM(CASE WHEN Tipo = 1 THEN Valor ELSE 0 END),0) -
              COALESCE(SUM(CASE WHEN Tipo = 2 THEN Valor ELSE 0 END),0)
            FROM Transacoes;
            """;
            var result = cmd.ExecuteScalar();
            return result is DBNull ? 0m : Convert.ToDecimal(result);
        }
    }

}
