namespace CadastroIgreja.Domain.Entities;
public class CartaPregacao
{
 public Guid Id { get; set; }
 public string Numero { get; set; } = string.Empty;
 public Guid UsuarioId { get; set; }
 public DateTime DataEmissao { get; set; }
 public DateTime DataValidade { get; set; }
 public string QrCode { get; set; } = string.Empty;
 public bool Ativa { get; set; } = true;
}