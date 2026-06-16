namespace CadastroIgreja.Domain.Entities;

public class Usuario
{
    public Guid Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public Guid IgrejaId { get; set; }
    public bool Ativo { get; set; } = true;
}