using CadastroIgreja.Domain;

namespace CadastroIgreja.Application;

public sealed record RegisterUserRequest(string FullName, string Email, string Password, Guid ChurchId, string? Phone);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthTokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);
public sealed record CreateChurchRequest(string Name, ChurchType Type, Guid? ParentId);
public sealed record ChurchResponse(Guid Id, string Name, ChurchType Type, Guid? ParentId);
public sealed record UserProfileResponse(Guid Id, string FullName, string Email, string? Phone, Guid ChurchId, MemberRole Role, UserStatus Status);
