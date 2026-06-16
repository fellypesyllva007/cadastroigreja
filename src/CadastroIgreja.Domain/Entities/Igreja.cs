using CadastroIgreja.Domain.Enums;

namespace CadastroIgreja.Domain.Entities;

public class Igreja
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public NivelHierarquico Tipo { get; set; }
    public Guid? ParentId { get; set; }
    public Igreja? Parent { get; set; }
    public ICollection<Igreja> Filhas { get; set; } = new List<Igreja>();
    public bool Ativa { get; set; } = true;
}
