using Nexora.Finance.CLI.Domain;
using Nexora.Finance.CLI.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexora.Finance.CLI
{
    public class App
    {
        private readonly TransactionService _service;
        private readonly FileService _files;

        public App(TransactionService service, FileService files)
        {
            _service = service;
            _files = files;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Nexora • Mini Central Financeira ===");
                Console.WriteLine("1) Adicionar transação");
                Console.WriteLine("2) Listar transações");
                Console.WriteLine("3) Editar transação");
                Console.WriteLine("4) Remover transação");
                Console.WriteLine("5) Resumo (saldo)");
                Console.WriteLine("6) Baixar extrato JSON");
                Console.WriteLine("0) Sair");
                Console.Write("Escolha: ");
                var op = Console.ReadLine()?.Trim();

                try
                {
                    switch (op)
                    {
                        case "1": Add(); break;
                        case "2": List(); break;
                        case "3": Edit(); break;
                        case "4": Remove(); break;
                        case "5": Summary(); break;
                        case "6": _files.ExportToJson(_service.GetAll()); break;
                        case "0": return;
                        default: Console.WriteLine("Opção inválida."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro: {ex.Message}");
                }

                Console.WriteLine("\nENTER para continuar...");
                Console.ReadLine();
            }
        }

        private void Add()
        {
            Console.Write("Descrição: ");
            var desc = Console.ReadLine() ?? "";

            Console.Write("Valor (use ponto): ");
            if (!decimal.TryParse(Console.ReadLine(), NumberStyles.Number, CultureInfo.InvariantCulture, out var valor))
            {
                Console.WriteLine("Valor inválido."); return;
            }

            Console.Write("Tipo (1=Entrada, 2=Saída): ");
            var tipoStr = Console.ReadLine();
            var tipo = (tipoStr == "1") ? TransactionType.Entrada : TransactionType.Saida;

            Console.Write("Data (vazio = agora) [yyyy-MM-dd HH:mm]: ");
            var dataStr = Console.ReadLine();
            DateTime? data = string.IsNullOrWhiteSpace(dataStr)
                ? null
                : DateTime.Parse(dataStr);

            var id = _service.Add(desc, valor, tipo, data);
            Console.WriteLine($"Adicionada. Id = {id}");
        }

        private void List()
        {
            var list = _service.GetAll();
            if (list.Count == 0) { Console.WriteLine("Sem transações."); return; }

            Console.WriteLine("\nId | Data                | Tipo    | Valor      | Descrição");
            Console.WriteLine("---------------------------------------------------------------");
            foreach (var t in list)
                Console.WriteLine($"{t.Id,2} | {t.Data:yyyy-MM-dd HH:mm} | {t.TipoLabel,-7} | {t.Valor,9:F2} | {t.Descricao}");
        }

        private void Edit()
        {
            var list = _service.GetAll();
            if (list.Count == 0)
            {
                Console.WriteLine("Nenhuma transação cadastrada para editar.");
                return;
            }

            Console.WriteLine("\nTransações existentes:");
            foreach (var t in list)
                Console.WriteLine($"{t.Id} | {t.Data:yyyy-MM-dd} | {t.TipoLabel} | {t.Valor:F2} | {t.Descricao}");

            Console.Write("\nDigite o ID da transação que deseja editar: ");
            if (!int.TryParse(Console.ReadLine(), out var id))
            {
                Console.WriteLine("ID inválido.");
                return;
            }

            var existente = list.FirstOrDefault(x => x.Id == id);
            if (existente == null)
            {
                Console.WriteLine("Nenhuma transação encontrada com esse ID.");
                return;
            }

            Console.Write($"Nova descrição (ENTER para manter: {existente.Descricao}): ");
            var desc = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(desc))
                desc = existente.Descricao; // mantém a descrição anterior

            Console.Write("Novo valor (ENTER para manter): ");
            var valorStr = Console.ReadLine();
            decimal valor = existente.Valor;
            if (!string.IsNullOrWhiteSpace(valorStr) && decimal.TryParse(valorStr, out var parsedValor))
                valor = parsedValor;

            Console.Write("Novo tipo (1=Entrada, 2=Saída, ENTER para manter): ");
            var tipoStr = Console.ReadLine();
            TransactionType? tipo = null;
            if (tipoStr == "1") tipo = TransactionType.Entrada;
            else if (tipoStr == "2") tipo = TransactionType.Saida;

            Console.Write("Nova data (yyyy-MM-dd HH:mm, ENTER para manter): ");
            var dataStr = Console.ReadLine();
            DateTime? data = string.IsNullOrWhiteSpace(dataStr) ? null : DateTime.Parse(dataStr);

            // Atualiza com validação
            if (string.IsNullOrWhiteSpace(desc))
            {
                Console.WriteLine("A descrição não pode ser vazia.");
                return;
            }

            _service.Update(id, desc, valor, tipo, data);
            Console.WriteLine("Transação atualizada.");


        }

        private void Remove()
        {
            var list = _service.GetAll();
            if (list.Count == 0)
            {
                Console.WriteLine("Nenhuma transação para excluir.");
                return;
            }

            Console.WriteLine("\nTransações existentes:");
            foreach (var t in list)
                Console.WriteLine($"{t.Id} | {t.Data:yyyy-MM-dd} | {t.TipoLabel} | {t.Valor:F2} | {t.Descricao}");

            Console.Write("\nDigite o ID da transação para excluir: ");
            if (!int.TryParse(Console.ReadLine(), out var id))
            {
                Console.WriteLine("ID inválido.");
                return;
            }

            var ok = _service.Delete(id);
            if (ok)
                Console.WriteLine("Transação excluída.");
            else
                Console.WriteLine("Nenhuma transação encontrada com esse ID.");
        }

        private void Summary()
        {
            var saldo = _service.GetBalance();
            Console.WriteLine($"Saldo atual: {saldo:F2}");
        }
    }

}
