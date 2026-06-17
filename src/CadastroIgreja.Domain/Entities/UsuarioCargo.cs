namespace CadastroIgreja.Domain.Entities;

public class UsuarioCargo
{
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public Guid CargoId { get; set; }
    public Cargo? Cargo { get; set; }
    public DateTime AtribuidoEm { get; set; } = DateTime.UtcNow;
}
