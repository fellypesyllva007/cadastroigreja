namespace CadastroIgreja.Domain;

public enum RequestStatus { Pending, Approved, Rejected }
public enum PreacherApprovalStep { CasaOracao, CongregacaoLocal, Setorial, Completed }

public sealed class RoleChangeRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; init; }
    public MemberRole RequestedRole { get; init; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DecidedAt { get; set; }
}

public sealed class PreacherRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; init; }
    public Guid ChurchId { get; init; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public PreacherApprovalStep CurrentStep { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DecidedAt { get; set; }
    public Guid? LetterId { get; set; }
}

public sealed class PreachingLetter
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; init; }
    public Guid ChurchId { get; init; }
    public required string Number { get; init; }
    public DateOnly IssuedAt { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly ValidUntil { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));
    public bool Suspended { get; set; }
}
