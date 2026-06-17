using CadastroIgreja.Domain;

namespace CadastroIgreja.Application;

public sealed record RegisterUserRequest(string FullName, string Email, string Password, Guid ChurchId, string? Phone);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthTokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);
public sealed record CreateChurchRequest(string Name, ChurchType Type, Guid? ParentId);
public sealed record ChurchResponse(Guid Id, string Name, ChurchType Type, Guid? ParentId);
public sealed record UserProfileResponse(Guid Id, string FullName, string Email, string? Phone, Guid ChurchId, MemberRole Role, UserStatus Status);

public sealed record CreateRoleChangeRequest(Guid UserId, MemberRole RequestedRole, string? Justification = null);
public sealed record RoleChangeRequestResponse(Guid Id, Guid UserId, MemberRole RequestedRole, RequestStatus Status, DateTimeOffset CreatedAt, DateTimeOffset? DecidedAt, string? Justification);

public sealed record CreatePreacherRequest(Guid UserId, string? Notes = null, Guid? DestinationChurchId = null);
public sealed record PreacherRequestResponse(Guid Id, Guid UserId, Guid ChurchId, RequestStatus Status, PreacherApprovalStep CurrentStep, DateTimeOffset CreatedAt, DateTimeOffset? DecidedAt, Guid? LetterId, string? Notes);

public sealed record PreachingLetterResponse(Guid Id, Guid UserId, Guid ChurchId, Guid PreacherRequestId, string Number, DateOnly IssuedAt, DateOnly ValidUntil, bool Suspended, string ValidationUrl);
public sealed record AuditLogResponse(long Id, Guid? UserId, string Action, string EntityName, string EntityId, string? Metadata, DateTimeOffset CreatedAt);

public sealed record LeaderSignatureRequest(string FileName, string MimeType, byte[] Content);
public sealed record LeaderSignatureResponse(Guid Id, Guid LeaderId, string StoragePath, string MimeType, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, bool Active);
public sealed record PreachingLetterValidationResponse(Guid Id, string LetterNumber, string PreacherName, string IssuingChurch, string DestinationChurch, DateTimeOffset IssuedAt, LetterStatus Status, DateTimeOffset ApprovedAt, string ApprovedByName);
public sealed record PreachingLetterPdfModel(PreachingLetter Letter, User Preacher, Church OriginChurch, Church? DestinationChurch, User Approver, Church? ApproverChurch, LeaderSignature? Signature, byte[]? SignatureBytes, IReadOnlyList<Church> OriginHierarchy);
