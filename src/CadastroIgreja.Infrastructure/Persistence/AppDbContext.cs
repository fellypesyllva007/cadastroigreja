using CadastroIgreja.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CadastroIgreja.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Igreja> Igrejas => Set<Igreja>();
    public DbSet<Cargo> Cargos => Set<Cargo>();
}