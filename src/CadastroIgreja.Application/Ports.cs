using CadastroIgreja.Domain;

namespace CadastroIgreja.Application;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
}

public interface IChurchRepository
{
    Task AddAsync(Church church, CancellationToken cancellationToken = default);
    Task<Church?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Church>> ListAsync(Guid? parentId, ChurchType? type, CancellationToken cancellationToken = default);
}

public interface IRoleChangeRequestRepository
{
    Task AddAsync(RoleChangeRequest request, CancellationToken cancellationToken = default);
    Task<RoleChangeRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RoleChangeRequest>> ListAsync(Guid? userId, RequestStatus? status, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
}

public interface IPreacherRequestRepository
{
    Task AddAsync(PreacherRequest request, CancellationToken cancellationToken = default);
    Task<PreacherRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PreacherRequest>> ListAsync(Guid? userId, RequestStatus? status, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
}

public interface IPreachingLetterRepository
{
    Task AddAsync(PreachingLetter letter, CancellationToken cancellationToken = default);
    Task<PreachingLetter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PreachingLetter>> ListAsync(Guid? userId, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

public interface ITokenService
{
    AuthTokenResponse Create(User user);
}

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AuditLog>> ListAsync(string? entityName, string? entityId, CancellationToken cancellationToken = default);
}
