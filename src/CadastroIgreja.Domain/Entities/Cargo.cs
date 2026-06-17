namespace CadastroIgreja.Domain.Entities;

public class Cargo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}
