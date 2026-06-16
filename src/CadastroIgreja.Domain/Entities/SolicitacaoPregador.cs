namespace CadastroIgreja.Domain.Entities;
public class SolicitacaoPregador
{
 public Guid Id { get; set; }
 public Guid UsuarioId { get; set; }
 public string Status { get; set; } = "Pendente";
 public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;
}