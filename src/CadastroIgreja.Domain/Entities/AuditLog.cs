namespace CadastroIgreja.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UsuarioId { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string Entidade { get; set; } = string.Empty;
    public string? EntidadeId { get; set; }
    public string? Ip { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
