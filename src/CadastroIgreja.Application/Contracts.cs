using CadastroIgreja.Domain;

namespace CadastroIgreja.Application;

public sealed record RegisterUserRequest(string FullName, string Email, string Password, Guid ChurchId, string? Phone);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthTokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);
public sealed record CreateChurchRequest(string Name, ChurchType Type, Guid? ParentId);
public sealed record ChurchResponse(Guid Id, string Name, ChurchType Type, Guid? ParentId);
public sealed record UserProfileResponse(Guid Id, string FullName, string Email, string? Phone, Guid ChurchId, MemberRole Role, UserStatus Status);

public sealed record CreateRoleChangeRequest(Guid UserId, MemberRole RequestedRole);
public sealed record RoleChangeRequestResponse(Guid Id, Guid UserId, MemberRole RequestedRole, RequestStatus Status, DateTimeOffset CreatedAt, DateTimeOffset? DecidedAt);

public sealed record CreatePreacherRequest(Guid UserId);
public sealed record PreacherRequestResponse(Guid Id, Guid UserId, Guid ChurchId, RequestStatus Status, PreacherApprovalStep CurrentStep, DateTimeOffset CreatedAt, DateTimeOffset? DecidedAt, Guid? LetterId);

public sealed record PreachingLetterResponse(Guid Id, Guid UserId, Guid ChurchId, string Number, DateOnly IssuedAt, DateOnly ValidUntil, bool Suspended);
