using Microsoft.EntityFrameworkCore;
using Nexora.Finance.API.Data;
using Nexora.Finance.CLI.Domain;

var builder = WebApplication.CreateBuilder(args);

// Usa a connstring do appsettings.json; se n�o existir, cai no default local
var cs = builder.Configuration.GetConnectionString("Sqlite")
         ?? "Data Source=DataStore/nexora.db";

builder.Services.AddDbContext<TransactionDbContext>(o => o.UseSqlite(cs));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// garante a pasta e cria o banco/tabela se n�o existir
Directory.CreateDirectory("DataStore");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
    db.Database.EnsureCreated();
}

// ---- ENDPOINTS M�NIMOS (parte 1 do CRUD) ----

// GET: lista todas as transa��es (ordenado)
app.MapGet("/api/transactions", async (TransactionDbContext db) =>
    await db.Transacoes
            .OrderByDescending(x => x.Data)
            .ThenByDescending(x => x.Id)
            .ToListAsync());

// POST: cria transa��o (valida��es simples)
app.MapPost("/api/transactions", async (Transaction input, TransactionDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.Descricao))
        return Results.BadRequest("Descri��o obrigat�ria.");
    if (input.Valor <= 0)
        return Results.BadRequest("Valor deve ser > 0.");

    if (input.Data == default) input.Data = DateTime.Now;

    db.Transacoes.Add(input);
    await db.SaveChangesAsync();
    return Results.Created($"/api/transactions/{input.Id}", input);
});

app.Run();
