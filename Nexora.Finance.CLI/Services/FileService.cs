using System.Text;
using System.Text.Json;
using Nexora.Finance.CLI.Domain;

namespace Nexora.Finance.CLI.Services
{
    public class FileService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        public void ExportToJson(List<Transaction> items)
        {
            Console.Write("Digite o caminho completo para salvar o JSON (ex: C:/Users/Professor/OneDrive/Área de Trabalho/nome_arquivo.json): ");
            var path = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("Caminho inválido, exportação cancelada.");
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(items, _jsonOptions);
                File.WriteAllText(path, json, Encoding.UTF8);
                Console.WriteLine($"Exportado com sucesso para: {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar: {ex.Message}");
            }
        }
    }
}
