using System.Collections.Concurrent;
using CadastroIgreja.Application;
using CadastroIgreja.Domain;

namespace CadastroIgreja.Infrastructure;

public sealed class InMemoryChurchRepository : IChurchRepository
{
    private readonly ConcurrentDictionary<Guid, Church> _churches = new();

    public InMemoryChurchRepository()
    {
        var sede = new Church { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Sede", Type = ChurchType.Sede };
        _churches.TryAdd(sede.Id, sede);
    }

    public Task AddAsync(Church church, CancellationToken cancellationToken = default)
    {
        _churches[church.Id] = church;
        return Task.CompletedTask;
    }

    public Task<Church?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_churches.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<Church>> ListAsync(Guid? parentId, ChurchType? type, CancellationToken cancellationToken = default)
    {
        var query = _churches.Values.AsEnumerable();
        if (parentId.HasValue) query = query.Where(c => c.ParentId == parentId);
        if (type.HasValue) query = query.Where(c => c.Type == type);
        return Task.FromResult<IReadOnlyCollection<Church>>(query.OrderBy(c => c.Name).ToArray());
    }
}

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.GetValueOrDefault(id));

    public Task SaveAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public sealed class InMemoryRoleChangeRequestRepository : IRoleChangeRequestRepository
{
    private readonly ConcurrentDictionary<Guid, RoleChangeRequest> _requests = new();
    public Task AddAsync(RoleChangeRequest request, CancellationToken cancellationToken = default) { _requests[request.Id] = request; return Task.CompletedTask; }
    public Task<RoleChangeRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_requests.GetValueOrDefault(id));
    public Task<IReadOnlyCollection<RoleChangeRequest>> ListAsync(Guid? userId, RequestStatus? status, CancellationToken cancellationToken = default)
    {
        var query = _requests.Values.AsEnumerable();
        if (userId.HasValue) query = query.Where(r => r.UserId == userId);
        if (status.HasValue) query = query.Where(r => r.Status == status);
        return Task.FromResult<IReadOnlyCollection<RoleChangeRequest>>(query.OrderByDescending(r => r.CreatedAt).ToArray());
    }
    public Task SaveAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public sealed class InMemoryPreacherRequestRepository : IPreacherRequestRepository
{
    private readonly ConcurrentDictionary<Guid, PreacherRequest> _requests = new();
    public Task AddAsync(PreacherRequest request, CancellationToken cancellationToken = default) { _requests[request.Id] = request; return Task.CompletedTask; }
    public Task<PreacherRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_requests.GetValueOrDefault(id));
    public Task<IReadOnlyCollection<PreacherRequest>> ListAsync(Guid? userId, RequestStatus? status, CancellationToken cancellationToken = default)
    {
        var query = _requests.Values.AsEnumerable();
        if (userId.HasValue) query = query.Where(r => r.UserId == userId);
        if (status.HasValue) query = query.Where(r => r.Status == status);
        return Task.FromResult<IReadOnlyCollection<PreacherRequest>>(query.OrderByDescending(r => r.CreatedAt).ToArray());
    }
    public Task SaveAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public sealed class InMemoryPreachingLetterRepository : IPreachingLetterRepository
{
    private readonly ConcurrentDictionary<Guid, PreachingLetter> _letters = new();
    public Task AddAsync(PreachingLetter letter, CancellationToken cancellationToken = default) { _letters[letter.Id] = letter; return Task.CompletedTask; }
    public Task<PreachingLetter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_letters.GetValueOrDefault(id));
    public Task<IReadOnlyCollection<PreachingLetter>> ListAsync(Guid? userId, CancellationToken cancellationToken = default)
    {
        var query = _letters.Values.AsEnumerable();
        if (userId.HasValue) query = query.Where(l => l.UserId == userId);
        return Task.FromResult<IReadOnlyCollection<PreachingLetter>>(query.OrderByDescending(l => l.IssuedAt).ToArray());
    }
    public Task SaveAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}


public sealed class InMemoryAuditLogRepository : IAuditLogRepository
{
    private readonly ConcurrentDictionary<long, AuditLog> _logs = new();

    public Task AddAsync(AuditLog log, CancellationToken cancellationToken = default)
    {
        _logs[log.Id] = log;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<AuditLog>> ListAsync(string? entityName, string? entityId, CancellationToken cancellationToken = default)
    {
        var query = _logs.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(entityName)) query = query.Where(l => l.EntityName.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(entityId)) query = query.Where(l => l.EntityId.Equals(entityId, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult<IReadOnlyCollection<AuditLog>>(query.OrderByDescending(l => l.CreatedAt).ToArray());
    }
}
