namespace CadastroIgreja.Domain.Entities;

public class CartaPregacao
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Numero { get; set; } = string.Empty;
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public Guid SolicitacaoPregadorId { get; set; }
    public SolicitacaoPregador? SolicitacaoPregador { get; set; }
    public DateTime DataEmissao { get; set; } = DateTime.UtcNow;
    public DateTime DataValidade { get; set; } = DateTime.UtcNow.AddYears(1);
    public string QrCode { get; set; } = string.Empty;
    public string? PdfPath { get; set; }
    public bool Ativa { get; set; } = true;
}
