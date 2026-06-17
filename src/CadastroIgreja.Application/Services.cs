using CadastroIgreja.Domain;

namespace CadastroIgreja.Application;

public sealed class AuthService(IUserRepository users, IChurchRepository churches, IPasswordHasher hasher, ITokenService tokens, IAuditLogRepository audit)
{
    public async Task<Guid> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName)) throw new InvalidOperationException("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(request.Email)) throw new InvalidOperationException("E-mail é obrigatório.");
        if (request.Password.Length < 8) throw new InvalidOperationException("Senha deve conter no mínimo 8 caracteres.");
        if (await churches.GetByIdAsync(request.ChurchId, ct) is null) throw new InvalidOperationException("Igreja não encontrada.");
        if (await users.GetByEmailAsync(request.Email, ct) is not null) throw new InvalidOperationException("E-mail já cadastrado.");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            ChurchId = request.ChurchId,
            PasswordHash = hasher.Hash(request.Password)
        };

        await users.AddAsync(user, ct);
        await audit.AddAsync(AuditLog.Create(null, "UserRegistered", nameof(User), user.Id.ToString(), $"churchId={user.ChurchId}"), ct);
        return user.Id;
    }

    public async Task<AuthTokenResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || user.Status != UserStatus.Approved || !hasher.Verify(request.Password, user.PasswordHash)) return null;
        await audit.AddAsync(AuditLog.Create(user.Id, "UserLoggedIn", nameof(User), user.Id.ToString()), ct);
        return tokens.Create(user);
    }
}

public sealed class ChurchService(IChurchRepository churches, IAuditLogRepository audit)
{
    public async Task<Guid> CreateAsync(CreateChurchRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("Nome da igreja é obrigatório.");
        if (request.Type != ChurchType.Sede && request.ParentId is null) throw new InvalidOperationException("Igrejas subordinadas exigem parentId.");
        if (request.Type == ChurchType.Sede && request.ParentId is not null) throw new InvalidOperationException("Sede não pode possuir parentId.");
        if (request.ParentId is not null)
        {
            var parent = await churches.GetByIdAsync(request.ParentId.Value, ct) ?? throw new InvalidOperationException("Igreja superior não encontrada.");
            if (parent.Type != ExpectedParentType(request.Type)) throw new InvalidOperationException($"{request.Type} exige igreja superior do tipo {ExpectedParentType(request.Type)}.");
        }
        var church = new Church { Name = request.Name.Trim(), Type = request.Type, ParentId = request.ParentId };
        await churches.AddAsync(church, ct);
        await audit.AddAsync(AuditLog.Create(null, "ChurchCreated", nameof(Church), church.Id.ToString(), $"type={church.Type};parentId={church.ParentId}"), ct);
        return church.Id;
    }

    public async Task<IReadOnlyCollection<ChurchResponse>> ListAsync(Guid? parentId, ChurchType? type, CancellationToken ct = default) =>
        (await churches.ListAsync(parentId, type, ct)).Select(c => new ChurchResponse(c.Id, c.Name, c.Type, c.ParentId)).ToArray();

    private static ChurchType? ExpectedParentType(ChurchType type) => type switch
    {
        ChurchType.Regional => ChurchType.Sede,
        ChurchType.Setorial => ChurchType.Regional,
        ChurchType.CongregacaoLocal => ChurchType.Setorial,
        ChurchType.CasaOracao => ChurchType.CongregacaoLocal,
        _ => null
    };
}

public sealed class UserService(IUserRepository users, IAuditLogRepository audit)
{
    public async Task<UserProfileResponse?> GetProfileAsync(Guid id, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(id, ct);
        return user is null ? null : new UserProfileResponse(user.Id, user.FullName, user.Email, user.Phone, user.ChurchId, user.Role, user.Status);
    }

    public async Task<bool> ApproveAsync(Guid id, Guid? approverId = null, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(id, ct);
        if (user is null) return false;
        user.Status = UserStatus.Approved;
        await users.SaveAsync(ct);
        await audit.AddAsync(AuditLog.Create(approverId, "UserApproved", nameof(User), id.ToString()), ct);
        return true;
    }

    public async Task<bool> RejectAsync(Guid id, Guid? approverId = null, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(id, ct);
        if (user is null) return false;
        user.Status = UserStatus.Rejected;
        await users.SaveAsync(ct);
        await audit.AddAsync(AuditLog.Create(approverId, "UserRejected", nameof(User), id.ToString()), ct);
        return true;
    }
}

public sealed class RoleChangeRequestService(IRoleChangeRequestRepository requests, IUserRepository users, IAuditLogRepository audit)
{
    public async Task<Guid> CreateAsync(CreateRoleChangeRequest request, CancellationToken ct = default)
    {
        if (await users.GetByIdAsync(request.UserId, ct) is null) throw new InvalidOperationException("Usuário não encontrado.");
        var entity = new RoleChangeRequest { UserId = request.UserId, RequestedRole = request.RequestedRole, Justification = request.Justification?.Trim() };
        await requests.AddAsync(entity, ct);
        await audit.AddAsync(AuditLog.Create(request.UserId, "RoleChangeRequested", nameof(RoleChangeRequest), entity.Id.ToString(), $"requestedRole={entity.RequestedRole}"), ct);
        return entity.Id;
    }

    public async Task<IReadOnlyCollection<RoleChangeRequestResponse>> ListAsync(Guid? userId, RequestStatus? status, CancellationToken ct = default) =>
        (await requests.ListAsync(userId, status, ct)).Select(ToResponse).ToArray();

    public async Task<bool> ApproveAsync(Guid id, Guid? approverId = null, CancellationToken ct = default)
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
        await audit.AddAsync(AuditLog.Create(approverId, "RoleChangeApproved", nameof(RoleChangeRequest), id.ToString()), ct);
        return true;
    }

    public async Task<bool> RejectAsync(Guid id, Guid? approverId = null, CancellationToken ct = default)
    {
        var request = await requests.GetByIdAsync(id, ct);
        if (request is null || request.Status != RequestStatus.Pending) return false;
        request.Status = RequestStatus.Rejected;
        request.DecidedAt = DateTimeOffset.UtcNow;
        await requests.SaveAsync(ct);
        await audit.AddAsync(AuditLog.Create(approverId, "RoleChangeRejected", nameof(RoleChangeRequest), id.ToString()), ct);
        return true;
    }

    private static RoleChangeRequestResponse ToResponse(RoleChangeRequest r) => new(r.Id, r.UserId, r.RequestedRole, r.Status, r.CreatedAt, r.DecidedAt, r.Justification);
}

public sealed class PreacherRequestService(IPreacherRequestRepository requests, IPreachingLetterRepository letters, IUserRepository users, IChurchRepository churches, IAuditLogRepository audit)
{
    public async Task<Guid> CreateAsync(CreatePreacherRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(request.UserId, ct) ?? throw new InvalidOperationException("Usuário não encontrado.");
        if (user.Status != UserStatus.Approved) throw new InvalidOperationException("Usuário precisa estar aprovado.");
        var church = await churches.GetByIdAsync(user.ChurchId, ct) ?? throw new InvalidOperationException("Igreja não encontrada.");
        var entity = new PreacherRequest { UserId = user.Id, ChurchId = user.ChurchId, CurrentStep = InitialStep(church.Type), Notes = request.Notes?.Trim() };
        await requests.AddAsync(entity, ct);
        await audit.AddAsync(AuditLog.Create(user.Id, "PreacherRequested", nameof(PreacherRequest), entity.Id.ToString(), $"step={entity.CurrentStep}"), ct);
        return entity.Id;
    }

    public async Task<IReadOnlyCollection<PreacherRequestResponse>> ListAsync(Guid? userId, RequestStatus? status, CancellationToken ct = default) =>
        (await requests.ListAsync(userId, status, ct)).Select(ToResponse).ToArray();

    public async Task<PreacherRequestResponse?> ApproveAsync(Guid id, Guid? approverId = null, CancellationToken ct = default)
    {
        var request = await requests.GetByIdAsync(id, ct);
        if (request is null || request.Status != RequestStatus.Pending) return null;
        if (request.CurrentStep != PreacherApprovalStep.Setorial)
        {
            request.CurrentStep = request.CurrentStep == PreacherApprovalStep.CasaOracao ? PreacherApprovalStep.CongregacaoLocal : PreacherApprovalStep.Setorial;
        }
        else
        {
            var letter = new PreachingLetter { UserId = request.UserId, ChurchId = request.ChurchId, PreacherRequestId = request.Id, Number = $"CP-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}" };
            await letters.AddAsync(letter, ct);
            request.Status = RequestStatus.Approved;
            request.CurrentStep = PreacherApprovalStep.Completed;
            request.DecidedAt = DateTimeOffset.UtcNow;
            request.LetterId = letter.Id;
        }
        await requests.SaveAsync(ct);
        await audit.AddAsync(AuditLog.Create(approverId, "PreacherStepApproved", nameof(PreacherRequest), id.ToString(), $"step={request.CurrentStep};letterId={request.LetterId}"), ct);
        return ToResponse(request);
    }

    public async Task<bool> RejectAsync(Guid id, Guid? approverId = null, CancellationToken ct = default)
    {
        var request = await requests.GetByIdAsync(id, ct);
        if (request is null || request.Status != RequestStatus.Pending) return false;
        request.Status = RequestStatus.Rejected;
        request.DecidedAt = DateTimeOffset.UtcNow;
        await requests.SaveAsync(ct);
        await audit.AddAsync(AuditLog.Create(approverId, "PreacherRejected", nameof(PreacherRequest), id.ToString()), ct);
        return true;
    }

    private static PreacherApprovalStep InitialStep(ChurchType type) => type switch
    {
        ChurchType.CasaOracao => PreacherApprovalStep.CasaOracao,
        ChurchType.CongregacaoLocal => PreacherApprovalStep.CongregacaoLocal,
        _ => PreacherApprovalStep.Setorial
    };

    private static PreacherRequestResponse ToResponse(PreacherRequest r) => new(r.Id, r.UserId, r.ChurchId, r.Status, r.CurrentStep, r.CreatedAt, r.DecidedAt, r.LetterId, r.Notes);
}

public sealed class PreachingLetterService(IPreachingLetterRepository letters, IAuditLogRepository audit)
{
    public async Task<IReadOnlyCollection<PreachingLetterResponse>> ListAsync(Guid? userId, CancellationToken ct = default) =>
        (await letters.ListAsync(userId, ct)).Select(ToResponse).ToArray();

    public async Task<PreachingLetterResponse?> ValidateAsync(Guid id, CancellationToken ct = default)
    {
        var letter = await letters.GetByIdAsync(id, ct);
        return letter is null ? null : ToResponse(letter);
    }

    public async Task<PreachingLetterResponse?> SuspendAsync(Guid id, Guid? actorId = null, CancellationToken ct = default)
    {
        var letter = await letters.GetByIdAsync(id, ct);
        if (letter is null) return null;
        letter.Suspended = true;
        await letters.SaveAsync(ct);
        await audit.AddAsync(AuditLog.Create(actorId, "LetterSuspended", nameof(PreachingLetter), id.ToString()), ct);
        return ToResponse(letter);
    }

    public async Task<PreachingLetterResponse?> RenewAsync(Guid id, Guid? actorId = null, CancellationToken ct = default)
    {
        var letter = await letters.GetByIdAsync(id, ct);
        if (letter is null) return null;
        letter.ValidUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));
        letter.Suspended = false;
        await letters.SaveAsync(ct);
        await audit.AddAsync(AuditLog.Create(actorId, "LetterRenewed", nameof(PreachingLetter), id.ToString(), $"validUntil={letter.ValidUntil:O}"), ct);
        return ToResponse(letter);
    }

    private static PreachingLetterResponse ToResponse(PreachingLetter l) => new(l.Id, l.UserId, l.ChurchId, l.PreacherRequestId, l.Number, l.IssuedAt, l.ValidUntil, l.Suspended, $"/api/letters/{l.Id}/validate");
}

public sealed class AuditLogService(IAuditLogRepository audit)
{
    public async Task<IReadOnlyCollection<AuditLogResponse>> ListAsync(string? entityName, string? entityId, CancellationToken ct = default) =>
        (await audit.ListAsync(entityName, entityId, ct)).Select(a => new AuditLogResponse(a.Id, a.UserId, a.Action, a.EntityName, a.EntityId, a.Metadata, a.CreatedAt)).ToArray();
}
