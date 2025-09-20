using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexora.Finance.CLI.Data
{
    public static class DbInitializer
    {
        public static void EnsureCreated(SqliteConnectionFactory factory)
        {
            using var conn = factory.Create();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Transacoes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Descricao TEXT NOT NULL,
                Valor NUMERIC NOT NULL,
                Tipo INTEGER NOT NULL,
                Data TEXT NOT NULL
            );
            """;
            cmd.ExecuteNonQuery();
        }
    }
}
