using Microsoft.EntityFrameworkCore;
using Nexora.Finance.CLI.Domain;

namespace Nexora.Finance.API.Data;

public class TransactionDbContext : DbContext
{
    public TransactionDbContext(DbContextOptions<TransactionDbContext> opt) : base(opt) { }

    public DbSet<Transaction> Transacoes => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        var t = b.Entity<Transaction>();
        t.ToTable("Transacoes");     // mesma tabela que você já usa no console
        t.HasKey(x => x.Id);
        t.Property(x => x.Descricao).IsRequired();
        t.Property(x => x.Valor).HasColumnType("NUMERIC").IsRequired();
        t.Property(x => x.Tipo).IsRequired();
        t.Property(x => x.Data).IsRequired();
    }
}
