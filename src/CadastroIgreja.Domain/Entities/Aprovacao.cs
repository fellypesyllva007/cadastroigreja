using CadastroIgreja.Domain.Enums;

namespace CadastroIgreja.Domain.Entities;

public class Aprovacao
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SolicitacaoPregadorId { get; set; }
    public SolicitacaoPregador? SolicitacaoPregador { get; set; }
    public Guid AprovadorId { get; set; }
    public Usuario? Aprovador { get; set; }
    public StatusSolicitacaoPregador StatusGerado { get; set; }
    public string? Observacao { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
