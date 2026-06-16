namespace CadastroIgreja.Domain;

public enum ChurchType { Sede, Regional, Setorial, CongregacaoLocal, CasaOracao }

public sealed class Church
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required ChurchType Type { get; set; }
    public Guid? ParentId { get; set; }
}
