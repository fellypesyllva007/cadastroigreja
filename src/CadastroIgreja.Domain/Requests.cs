namespace CadastroIgreja.Domain;

public enum RequestStatus { Pending, Approved, Rejected }
public enum LetterStatus { Active, Suspended, Expired, Cancelled }
public enum PreacherApprovalStep { CasaOracao, CongregacaoLocal, Setorial, Completed }

public sealed class RoleChangeRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; init; }
    public MemberRole RequestedRole { get; init; }
    public string? Justification { get; init; }
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
    public string? Notes { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DecidedAt { get; set; }
    public Guid? LetterId { get; set; }
    public Guid? DestinationChurchId { get; set; }
}

public sealed class PreachingLetter
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string LetterNumber { get; init; }
    public Guid PreacherRequestId { get; init; }
    public Guid UserId { get; init; }
    public Guid ChurchId { get; init; }
    public Guid? DestinationChurchId { get; init; }
    public Guid ApprovedByUserId { get; init; }
    public DateTimeOffset ApprovedAt { get; init; }
    public required string PdfStoragePath { get; set; }
    public required string QrCodeValue { get; set; }
    public LetterStatus Status { get; set; } = LetterStatus.Active;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string Number => LetterNumber;
    public DateOnly IssuedAt => DateOnly.FromDateTime(ApprovedAt.DateTime);
    public DateOnly ValidUntil { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));
    public bool Suspended
    {
        get => Status == LetterStatus.Suspended;
        set => Status = value ? LetterStatus.Suspended : LetterStatus.Active;
    }
}

public sealed class LeaderSignature
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid LeaderId { get; init; }
    public required string StoragePath { get; set; }
    public required string MimeType { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool Active { get; set; } = true;
}

public sealed class AuditLog
{
    private static long _nextId;

    public long Id { get; init; } = Interlocked.Increment(ref _nextId);
    public Guid? UserId { get; init; }
    public required string Action { get; init; }
    public required string EntityName { get; init; }
    public required string EntityId { get; init; }
    public string? Metadata { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public static AuditLog Create(Guid? userId, string action, string entityName, string entityId, string? metadata = null) =>
        new() { UserId = userId, Action = action, EntityName = entityName, EntityId = entityId, Metadata = metadata };
}
