using CadastroIgreja.Domain;

namespace CadastroIgreja.Application;

public sealed class AuthService(IUserRepository users, IChurchRepository churches, IPasswordHasher hasher, ITokenService tokens)
{
    public async Task<Guid> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        if (await churches.GetByIdAsync(request.ChurchId, ct) is null) throw new InvalidOperationException("Igreja não encontrada.");
        if (await users.GetByEmailAsync(request.Email, ct) is not null) throw new InvalidOperationException("E-mail já cadastrado.");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone,
            ChurchId = request.ChurchId,
            PasswordHash = hasher.Hash(request.Password)
        };

        await users.AddAsync(user, ct);
        return user.Id;
    }

    public async Task<AuthTokenResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || user.Status != UserStatus.Approved || !hasher.Verify(request.Password, user.PasswordHash)) return null;
        return tokens.Create(user);
    }
}

public sealed class ChurchService(IChurchRepository churches)
{
    public async Task<Guid> CreateAsync(CreateChurchRequest request, CancellationToken ct = default)
    {
        if (request.Type != ChurchType.Sede && request.ParentId is null) throw new InvalidOperationException("Igrejas subordinadas exigem parentId.");
        var church = new Church { Name = request.Name.Trim(), Type = request.Type, ParentId = request.ParentId };
        await churches.AddAsync(church, ct);
        return church.Id;
    }

    public async Task<IReadOnlyCollection<ChurchResponse>> ListAsync(Guid? parentId, ChurchType? type, CancellationToken ct = default) =>
        (await churches.ListAsync(parentId, type, ct)).Select(c => new ChurchResponse(c.Id, c.Name, c.Type, c.ParentId)).ToArray();
}

public sealed class UserService(IUserRepository users)
{
    public async Task<UserProfileResponse?> GetProfileAsync(Guid id, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(id, ct);
        return user is null ? null : new UserProfileResponse(user.Id, user.FullName, user.Email, user.Phone, user.ChurchId, user.Role, user.Status);
    }

    public async Task<bool> ApproveAsync(Guid id, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(id, ct);
        if (user is null) return false;
        user.Status = UserStatus.Approved;
        await users.SaveAsync(ct);
        return true;
    }
}
