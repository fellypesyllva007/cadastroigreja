namespace CadastroIgreja.Domain;

public enum ChurchType { Sede, Regional, Setorial, CongregacaoLocal, CasaOracao }

public sealed class Church
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required ChurchType Type { get; set; }
    public Guid? ParentId { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Phone { get; set; }
    public string? Cnpj { get; set; }
    public string? InstitutionalInfo { get; set; }
}
