using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nexora.Finance.CLI.Domain
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public TransactionType Tipo { get; set; }
        public DateTime Data { get; set; } = DateTime.Now;

        [JsonIgnore]
        public string TipoLabel => Tipo == TransactionType.Entrada ? "Entrada" : "Saída";
    }

}
