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
