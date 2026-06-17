namespace CadastroIgreja.Domain.Entities;

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string SenhaHash { get; set; } = string.Empty;
    public Guid IgrejaId { get; set; }
    public Igreja? Igreja { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public ICollection<UsuarioCargo> UsuarioCargos { get; set; } = new List<UsuarioCargo>();
}
