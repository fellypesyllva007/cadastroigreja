using CadastroIgreja.Domain.Entities;
using CadastroIgreja.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CadastroIgreja.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Igreja> Igrejas => Set<Igreja>();
    public DbSet<Cargo> Cargos => Set<Cargo>();
    public DbSet<UsuarioCargo> UsuarioCargos => Set<UsuarioCargo>();
    public DbSet<SolicitacaoPregador> SolicitacoesPregador => Set<SolicitacaoPregador>();
    public DbSet<SolicitacaoCargo> SolicitacoesCargo => Set<SolicitacaoCargo>();
    public DbSet<Aprovacao> Aprovacoes => Set<Aprovacao>();
    public DbSet<CartaPregacao> CartasPregacao => Set<CartaPregacao>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Usuario>().Property(x => x.Email).HasMaxLength(180);
        modelBuilder.Entity<Igreja>().Property(x => x.Tipo).HasConversion<string>();
        modelBuilder.Entity<SolicitacaoPregador>().Property(x => x.Status).HasConversion<string>();
        modelBuilder.Entity<SolicitacaoCargo>().Property(x => x.Status).HasConversion<string>();
        modelBuilder.Entity<Aprovacao>().Property(x => x.StatusGerado).HasConversion<string>();
        modelBuilder.Entity<CartaPregacao>().HasIndex(x => x.Numero).IsUnique();
        modelBuilder.Entity<UsuarioCargo>().HasKey(x => new { x.UsuarioId, x.CargoId });
        modelBuilder.Entity<Igreja>().HasOne(x => x.Parent).WithMany(x => x.Filhas).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cargo>().HasData(
            new Cargo { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Nome = "Membro" },
            new Cargo { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Nome = "Diácono" },
            new Cargo { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Nome = "Presbítero" },
            new Cargo { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Nome = "Pastor" },
            new Cargo { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), Nome = "Dirigente" });
    }
}
