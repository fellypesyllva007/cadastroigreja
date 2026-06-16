namespace CadastroIgreja.Domain;

public enum MemberRole { Membro, Diacono, Presbitero, Pastor, Dirigente }
public enum UserStatus { Pending, Approved }

public sealed class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string PasswordHash { get; set; }
    public Guid ChurchId { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Membro;
    public UserStatus Status { get; set; } = UserStatus.Pending;
}
