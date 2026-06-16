namespace CadastroIgreja.Domain.Entities;

public class Igreja
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
}