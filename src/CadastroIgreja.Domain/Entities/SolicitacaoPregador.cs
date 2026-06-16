using CadastroIgreja.Domain.Enums;

namespace CadastroIgreja.Domain.Entities;

public class SolicitacaoPregador
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public StatusSolicitacaoPregador Status { get; set; } = StatusSolicitacaoPregador.Pendente;
    public string? Observacao { get; set; }
    public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;
    public ICollection<Aprovacao> Aprovacoes { get; set; } = new List<Aprovacao>();
}
