using CadastroIgreja.Domain.Enums;

namespace CadastroIgreja.Domain.Entities;

public class SolicitacaoCargo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public Guid CargoSolicitadoId { get; set; }
    public Cargo? CargoSolicitado { get; set; }
    public StatusSolicitacaoCargo Status { get; set; } = StatusSolicitacaoCargo.Pendente;
    public string? Justificativa { get; set; }
    public string? ObservacaoAprovacao { get; set; }
    public Guid? AprovadorId { get; set; }
    public Usuario? Aprovador { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AvaliadoEm { get; set; }
}
