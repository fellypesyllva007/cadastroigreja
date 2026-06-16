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

public sealed class RoleChangeRequestService(IRoleChangeRequestRepository requests, IUserRepository users)
{
    public async Task<Guid> CreateAsync(CreateRoleChangeRequest request, CancellationToken ct = default)
    {
        if (await users.GetByIdAsync(request.UserId, ct) is null) throw new InvalidOperationException("Usuário não encontrado.");
        var entity = new RoleChangeRequest { UserId = request.UserId, RequestedRole = request.RequestedRole };
        await requests.AddAsync(entity, ct);
        return entity.Id;
    }

    public async Task<IReadOnlyCollection<RoleChangeRequestResponse>> ListAsync(Guid? userId, RequestStatus? status, CancellationToken ct = default) =>
        (await requests.ListAsync(userId, status, ct)).Select(ToResponse).ToArray();

    public async Task<bool> ApproveAsync(Guid id, CancellationToken ct = default)
    {
        var request = await requests.GetByIdAsync(id, ct);
        if (request is null || request.Status != RequestStatus.Pending) return false;
        var user = await users.GetByIdAsync(request.UserId, ct);
        if (user is null) return false;
        user.Role = request.RequestedRole;
        request.Status = RequestStatus.Approved;
        request.DecidedAt = DateTimeOffset.UtcNow;
        await requests.SaveAsync(ct);
        await users.SaveAsync(ct);
        return true;
    }

    private static RoleChangeRequestResponse ToResponse(RoleChangeRequest r) => new(r.Id, r.UserId, r.RequestedRole, r.Status, r.CreatedAt, r.DecidedAt);
}

public sealed class PreacherRequestService(IPreacherRequestRepository requests, IPreachingLetterRepository letters, IUserRepository users, IChurchRepository churches)
{
    public async Task<Guid> CreateAsync(CreatePreacherRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(request.UserId, ct) ?? throw new InvalidOperationException("Usuário não encontrado.");
        var church = await churches.GetByIdAsync(user.ChurchId, ct) ?? throw new InvalidOperationException("Igreja não encontrada.");
        var entity = new PreacherRequest { UserId = user.Id, ChurchId = user.ChurchId, CurrentStep = InitialStep(church.Type) };
        await requests.AddAsync(entity, ct);
        return entity.Id;
    }

    public async Task<IReadOnlyCollection<PreacherRequestResponse>> ListAsync(Guid? userId, RequestStatus? status, CancellationToken ct = default) =>
        (await requests.ListAsync(userId, status, ct)).Select(ToResponse).ToArray();

    public async Task<PreacherRequestResponse?> ApproveAsync(Guid id, CancellationToken ct = default)
    {
        var request = await requests.GetByIdAsync(id, ct);
        if (request is null || request.Status != RequestStatus.Pending) return null;
        if (request.CurrentStep != PreacherApprovalStep.Setorial)
        {
            request.CurrentStep = request.CurrentStep == PreacherApprovalStep.CasaOracao ? PreacherApprovalStep.CongregacaoLocal : PreacherApprovalStep.Setorial;
        }
        else
        {
            var letter = new PreachingLetter { UserId = request.UserId, ChurchId = request.ChurchId, Number = $"CP-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}" };
            await letters.AddAsync(letter, ct);
            request.Status = RequestStatus.Approved;
            request.CurrentStep = PreacherApprovalStep.Completed;
            request.DecidedAt = DateTimeOffset.UtcNow;
            request.LetterId = letter.Id;
        }
        await requests.SaveAsync(ct);
        return ToResponse(request);
    }

    private static PreacherApprovalStep InitialStep(ChurchType type) => type switch
    {
        ChurchType.CasaOracao => PreacherApprovalStep.CasaOracao,
        ChurchType.CongregacaoLocal => PreacherApprovalStep.CongregacaoLocal,
        _ => PreacherApprovalStep.Setorial
    };

    private static PreacherRequestResponse ToResponse(PreacherRequest r) => new(r.Id, r.UserId, r.ChurchId, r.Status, r.CurrentStep, r.CreatedAt, r.DecidedAt, r.LetterId);
}

public sealed class PreachingLetterService(IPreachingLetterRepository letters)
{
    public async Task<IReadOnlyCollection<PreachingLetterResponse>> ListAsync(Guid? userId, CancellationToken ct = default) =>
        (await letters.ListAsync(userId, ct)).Select(ToResponse).ToArray();

    public async Task<PreachingLetterResponse?> ValidateAsync(Guid id, CancellationToken ct = default)
    {
        var letter = await letters.GetByIdAsync(id, ct);
        return letter is null ? null : ToResponse(letter);
    }

    private static PreachingLetterResponse ToResponse(PreachingLetter l) => new(l.Id, l.UserId, l.ChurchId, l.Number, l.IssuedAt, l.ValidUntil, l.Suspended);
}
