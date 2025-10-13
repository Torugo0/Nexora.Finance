using Microsoft.EntityFrameworkCore;
using Nexora.Finance.API.Data;
using Nexora.Finance.CLI.Domain;
using System.Net.Http.Json;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Nexora");
Directory.CreateDirectory(baseDir);
var dbPath = Path.Combine(baseDir, "nexora.db");
var cs = $"Data Source={dbPath}";

builder.Services.AddDbContext<TransactionDbContext>(o => o.UseSqlite(cs));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
    db.Database.EnsureCreated();
}

// CRUD API

// GET: lista todas as transações (ordenado por Data desc, Id desc)
app.MapGet("/api/transactions", async (TransactionDbContext db) =>
    await db.Transacoes
            .OrderByDescending(x => x.Data)
            .ThenByDescending(x => x.Id)
            .ToListAsync());

// GET by id
app.MapGet("/api/transactions/{id:int}", async (int id, TransactionDbContext db) =>
{
    var t = await db.Transacoes.FindAsync(id);
    return t is null ? Results.NotFound() : Results.Ok(t);
});

// POST: cria transação (validação simples)
app.MapPost("/api/transactions", async (Transaction input, TransactionDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.Descricao))
        return Results.BadRequest("Descrição obrigatória.");
    if (input.Valor <= 0)
        return Results.BadRequest("Valor deve ser > 0.");

    if (input.Data == default) input.Data = DateTime.Now;

    db.Transacoes.Add(input);
    await db.SaveChangesAsync();
    return Results.Created($"/api/transactions/{input.Id}", input);
});

// PUT: atualiza transação
app.MapPut("/api/transactions/{id:int}", async (int id, Transaction upd, TransactionDbContext db) =>
{
    var cur = await db.Transacoes.FindAsync(id);
    if (cur is null) return Results.NotFound();

    if (!string.IsNullOrWhiteSpace(upd.Descricao)) cur.Descricao = upd.Descricao;
    if (upd.Valor > 0) cur.Valor = upd.Valor;
    cur.Tipo = upd.Tipo; // enum, se vier 0 você mantém regra do cliente
    if (upd.Data != default) cur.Data = upd.Data;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE: remove transação
app.MapDelete("/api/transactions/{id:int}", async (int id, TransactionDbContext db) =>
{
    var cur = await db.Transacoes.FindAsync(id);
    if (cur is null) return Results.NotFound();

    db.Transacoes.Remove(cur);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// SUMÁRIO: entradas, saídas, saldo e contagem
app.MapGet("/api/transactions/summary", async (TransactionDbContext db) =>
{
    var entradas = await db.Transacoes
        .Where(t => t.Tipo == TransactionType.Entrada)
        .SumAsync(t => t.Valor);

    var saidas = await db.Transacoes
        .Where(t => t.Tipo == TransactionType.Saida)
        .SumAsync(t => t.Valor);

    var total = await db.Transacoes.CountAsync();

    return Results.Ok(new
    {
        entradas,
        saidas,
        saldo = entradas - saidas,
        totalTransacoes = total
    });
});

// BUSCA FILTRADA: por período, tipo (1(Entrada) / 2(Saida)) e texto na descrição
app.MapGet("/api/transactions/search",
    async (DateTime? from, DateTime? to, TransactionType? tipo, string? q, TransactionDbContext db) =>
    {
        var query = db.Transacoes.AsQueryable();

        if (from.HasValue) query = query.Where(t => t.Data >= from.Value);
        if (to.HasValue) query = query.Where(t => t.Data <= to.Value);
        if (tipo.HasValue) query = query.Where(t => t.Tipo == tipo.Value);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(t => t.Descricao.Contains(q));

        var result = await query
            .OrderByDescending(t => t.Data)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

        return Results.Ok(result);
});

// ESTATÍSTICAS MENSAIS: soma e quantidade por Ano-Mês e Tipo (Entrada/Saída)
app.MapGet("/api/transactions/stats/by-month", async (TransactionDbContext db) =>
{
    var stats = await db.Transacoes
        .GroupBy(t => new { t.Data.Year, t.Data.Month, t.Tipo })
        .Select(g => new
        {
            Year = g.Key.Year,
            Month = g.Key.Month,
            Tipo = g.Key.Tipo,
            Total = g.Sum(x => x.Valor),
            Count = g.Count()
        })
        .OrderBy(x => x.Year)
        .ThenBy(x => x.Month)
        .ThenBy(x => x.Tipo)
        .ToListAsync();

    return Results.Ok(stats);
});

static async Task<(bool ok, decimal rate, string provider, object raw, string? error)>
FetchRateAsync(HttpClient http, string from, string to)
{
    try
    {
        var url1 = $"https://api.exchangerate.host/latest?base={from}&symbols={to}";
        var doc1 = await http.GetFromJsonAsync<JsonElement>(url1);
        if (doc1.TryGetProperty("rates", out var r1) && r1.TryGetProperty(to, out var e1))
            return (true, e1.GetDecimal(), "exchangerate.host", doc1, null);
    }
    catch (Exception ex) {}

    try
    {
        var url2 = $"https://api.frankfurter.app/latest?from={from}&to={to}";
        var doc2 = await http.GetFromJsonAsync<JsonElement>(url2);
        if (doc2.TryGetProperty("rates", out var r2) && r2.TryGetProperty(to, out var e2))
            return (true, e2.GetDecimal(), "frankfurter.app", doc2, null);
    }
    catch (Exception ex) { return (false, 0m, "", new { }, ex.Message); }

    return (false, 0m, "", new { }, "Nenhum provedor retornou taxa válida.");
}



// Uso da API externa, cotação em tempo real (Dolar)
app.MapGet("/api/external/exchange", async (HttpClient http, string @base = "BRL", string symbols = "USD") =>
{
    var (ok, rate, provider, raw, error) = await FetchRateAsync(http, @base, symbols);
    if (!ok) return Results.Problem(error ?? "Falha na consulta de câmbio.", statusCode: 502);

    return Results.Ok(new
    {
        @base,
        symbols,
        rate,
        provider,
        fetchedAt = DateTime.UtcNow,
        raw
    });
})
.WithSummary("Cotação de moedas com fallback (exchangerate.host → frankfurter.app)")
.WithDescription("Ex.: /api/external/exchange?base=BRL&symbols=USD");

// POST
app.MapPost("/api/external/exchange/transaction", async (
    HttpClient http,
    TransactionDbContext db,
    decimal valor,
    string from = "USD",
    string to = "BRL") =>
{
    if (valor <= 0) return Results.BadRequest("Valor deve ser > 0.");

    var (ok, rate, provider, raw, error) = await FetchRateAsync(http, from, to);
    if (!ok) return Results.Problem(error ?? "Falha na consulta de câmbio.", statusCode: 502);

    var convertido = valor * rate;

    var t = new Transaction
    {
        Descricao = $"Conversão {from}->{to} via {provider}",
        Valor = convertido,
        Tipo = TransactionType.Entrada,
        Data = DateTime.Now
    };

    db.Transacoes.Add(t);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        from,
        to,
        rate,
        provider,
        valorOriginal = valor,
        valorConvertido = convertido,
        transacaoId = t.Id
    });
})
.WithSummary("Converte valor entre moedas e grava como transação (Entrada)")
.WithDescription("Ex.: POST /api/external/exchange/transaction?valor=100&from=USD&to=BRL");


app.Run();