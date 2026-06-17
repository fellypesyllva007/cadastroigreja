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


public sealed class HierarchicalAuthorizationService(IUserRepository users, IChurchRepository churches) : IHierarchicalAuthorizationService
{
    public async Task<bool> CanApproveUserAsync(Guid? approverId, User targetUser, CancellationToken cancellationToken = default) =>
        await HasMinisterialScopeAsync(approverId, targetUser.ChurchId, cancellationToken);

    public async Task<bool> CanApproveRoleChangeAsync(Guid? approverId, User targetUser, MemberRole requestedRole, CancellationToken cancellationToken = default)
    {
        var approver = await GetActiveMinisterAsync(approverId, cancellationToken);
        if (approver is null || !await IsChurchInScopeAsync(approver.ChurchId, targetUser.ChurchId, cancellationToken)) return false;
        if (requestedRole == MemberRole.Pastor || requestedRole == MemberRole.Dirigente) return approver.Role == MemberRole.Dirigente;
        return true;
    }

    public async Task<bool> CanApprovePreacherStepAsync(Guid? approverId, PreacherRequest request, CancellationToken cancellationToken = default) =>
        await HasMinisterialScopeAsync(approverId, request.ChurchId, cancellationToken);

    public async Task<bool> CanIssueLetterAsync(Guid? approverId, PreacherRequest request, CancellationToken cancellationToken = default) =>
        await HasMinisterialScopeAsync(approverId, request.ChurchId, cancellationToken);

    public async Task<bool> CanSuspendLetterAsync(Guid? actorId, PreachingLetter letter, CancellationToken cancellationToken = default) =>
        await HasMinisterialScopeAsync(actorId, letter.ChurchId, cancellationToken);

    public async Task<bool> CanViewAuditLogsAsync(Guid? actorId, CancellationToken cancellationToken = default) =>
        await GetActiveMinisterAsync(actorId, cancellationToken) is { Role: MemberRole.Dirigente };

    private async Task<bool> HasMinisterialScopeAsync(Guid? actorId, Guid targetChurchId, CancellationToken ct)
    {
        var actor = await GetActiveMinisterAsync(actorId, ct);
        return actor is not null && await IsChurchInScopeAsync(actor.ChurchId, targetChurchId, ct);
    }

    private async Task<User?> GetActiveMinisterAsync(Guid? actorId, CancellationToken ct)
    {
        if (actorId is null) return null;
        var actor = await users.GetByIdAsync(actorId.Value, ct);
        return actor is { Status: UserStatus.Approved } && (actor.Role == MemberRole.Dirigente || actor.Role == MemberRole.Pastor) ? actor : null;
    }

    private async Task<bool> IsChurchInScopeAsync(Guid actorChurchId, Guid targetChurchId, CancellationToken ct)
    {
        var currentId = targetChurchId;
        while (true)
        {
            if (currentId == actorChurchId) return true;
            var current = await churches.GetByIdAsync(currentId, ct);
            if (current?.ParentId is not Guid parentId) return false;
            currentId = parentId;
        }
    }
}

internal static class AuthorizationGuards
{
    public static void Ensure(bool allowed, string message = "Usuário sem permissão hierárquica para esta ação.")
    {
        if (!allowed) throw new UnauthorizedAccessException(message);
    }
}


public sealed class UserService(IUserRepository users, IHierarchicalAuthorizationService authorization, IAuditLogRepository audit)
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
        AuthorizationGuards.Ensure(await authorization.CanApproveUserAsync(approverId, user, ct));
        user.Status = UserStatus.Approved;
        await users.SaveAsync(ct);
        await audit.AddAsync(AuditLog.Create(approverId, "UserApproved", nameof(User), id.ToString()), ct);
        return true;
    }

    public async Task<bool> RejectAsync(Guid id, Guid? approverId = null, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(id, ct);
        if (user is null) return false;
        AuthorizationGuards.Ensure(await authorization.CanApproveUserAsync(approverId, user, ct));
        user.Status = UserStatus.Rejected;
        await users.SaveAsync(ct);
        await audit.AddAsync(AuditLog.Create(approverId, "UserRejected", nameof(User), id.ToString()), ct);
        return true;
    }
}

public sealed class RoleChangeRequestService(IRoleChangeRequestRepository requests, IUserRepository users, IHierarchicalAuthorizationService authorization, IAuditLogRepository audit)
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
        AuthorizationGuards.Ensure(await authorization.CanApproveRoleChangeAsync(approverId, user, request.RequestedRole, ct));
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
        var targetUser = await users.GetByIdAsync(request.UserId, ct);
        if (targetUser is null) return false;
        AuthorizationGuards.Ensure(await authorization.CanApproveRoleChangeAsync(approverId, targetUser, request.RequestedRole, ct));
        request.Status = RequestStatus.Rejected;
        request.DecidedAt = DateTimeOffset.UtcNow;
        await requests.SaveAsync(ct);
        await audit.AddAsync(AuditLog.Create(approverId, "RoleChangeRejected", nameof(RoleChangeRequest), id.ToString()), ct);
        return true;
    }

    private static RoleChangeRequestResponse ToResponse(RoleChangeRequest r) => new(r.Id, r.UserId, r.RequestedRole, r.Status, r.CreatedAt, r.DecidedAt, r.Justification);
}

public sealed class PreacherRequestService(IPreacherRequestRepository requests, IPreachingLetterRepository letters, IUserRepository users, IChurchRepository churches, IHierarchicalAuthorizationService authorization, ILeaderSignatureRepository signatures, IFileStorage storage, IPreachingLetterPdfGenerator pdfGenerator, IAuditLogRepository audit)
{
    public async Task<Guid> CreateAsync(CreatePreacherRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(request.UserId, ct) ?? throw new InvalidOperationException("Usuário não encontrado.");
        if (user.Status != UserStatus.Approved) throw new InvalidOperationException("Usuário precisa estar aprovado.");
        var church = await churches.GetByIdAsync(user.ChurchId, ct) ?? throw new InvalidOperationException("Igreja não encontrada.");
        var entity = new PreacherRequest { UserId = user.Id, ChurchId = user.ChurchId, DestinationChurchId = request.DestinationChurchId, CurrentStep = InitialStep(church.Type), Notes = request.Notes?.Trim() };
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
        AuthorizationGuards.Ensure(await authorization.CanApprovePreacherStepAsync(approverId, request, ct));
        if (request.CurrentStep != PreacherApprovalStep.Setorial)
        {
            request.CurrentStep = request.CurrentStep == PreacherApprovalStep.CasaOracao ? PreacherApprovalStep.CongregacaoLocal : PreacherApprovalStep.Setorial;
        }
        else
        {
            AuthorizationGuards.Ensure(await authorization.CanIssueLetterAsync(approverId, request, ct));
            var approvedAt = DateTimeOffset.UtcNow;
            request.Status = RequestStatus.Approved;
            request.CurrentStep = PreacherApprovalStep.Completed;
            request.DecidedAt = approvedAt;
            var validationUrl = $"/api/letters/{{0}}/validate";
            var letter = new PreachingLetter
            {
                UserId = request.UserId,
                ChurchId = request.ChurchId,
                DestinationChurchId = request.DestinationChurchId,
                PreacherRequestId = request.Id,
                ApprovedByUserId = approverId!.Value,
                ApprovedAt = approvedAt,
                LetterNumber = $"CP-{approvedAt:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
                PdfStoragePath = string.Empty,
                QrCodeValue = validationUrl
            };
            letter.QrCodeValue = $"/api/letters/{letter.Id}/validate";
            var preacher = await users.GetByIdAsync(request.UserId, ct) ?? throw new InvalidOperationException("Pregador não encontrado.");
            var originChurch = await churches.GetByIdAsync(request.ChurchId, ct) ?? throw new InvalidOperationException("Igreja de origem não encontrada.");
            var destinationChurch = request.DestinationChurchId is null ? null : await churches.GetByIdAsync(request.DestinationChurchId.Value, ct);
            var approver = await users.GetByIdAsync(approverId.Value, ct) ?? throw new InvalidOperationException("Aprovador não encontrado.");
            var approverChurch = await churches.GetByIdAsync(approver.ChurchId, ct);
            var signature = await signatures.GetActiveByLeaderIdAsync(approver.Id, ct);
            var signatureBytes = signature is null ? null : await storage.ReadAsync(signature.StoragePath, ct);
            if (signature is null) await audit.AddAsync(AuditLog.Create(approver.Id, "LeaderSignatureMissing", nameof(LeaderSignature), approver.Id.ToString(), $"preacherRequestId={request.Id}"), ct);
            var pdf = await pdfGenerator.GenerateAsync(new PreachingLetterPdfModel(letter, preacher, originChurch, destinationChurch, approver, approverChurch, signature, signatureBytes, await LoadHierarchyAsync(originChurch, ct)), ct);
            letter.PdfStoragePath = await storage.SaveAsync($"storage/letters/{letter.Id}/carta-recomendacao.pdf", pdf, "application/pdf", ct);
            await letters.AddAsync(letter, ct);
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
        AuthorizationGuards.Ensure(await authorization.CanApprovePreacherStepAsync(approverId, request, ct));
        request.Status = RequestStatus.Rejected;
        request.DecidedAt = DateTimeOffset.UtcNow;
        await requests.SaveAsync(ct);
        await audit.AddAsync(AuditLog.Create(approverId, "PreacherRejected", nameof(PreacherRequest), id.ToString()), ct);
        return true;
    }

    private async Task<IReadOnlyList<Church>> LoadHierarchyAsync(Church church, CancellationToken ct)
    {
        var result = new List<Church> { church };
        var current = church;
        while (current.ParentId is Guid parentId && await churches.GetByIdAsync(parentId, ct) is { } parent)
        {
            result.Add(parent);
            current = parent;
        }
        return result;
    }

    private static PreacherApprovalStep InitialStep(ChurchType type) => type switch
    {
        ChurchType.CasaOracao => PreacherApprovalStep.CasaOracao,
        ChurchType.CongregacaoLocal => PreacherApprovalStep.CongregacaoLocal,
        _ => PreacherApprovalStep.Setorial
    };

    private static PreacherRequestResponse ToResponse(PreacherRequest r) => new(r.Id, r.UserId, r.ChurchId, r.Status, r.CurrentStep, r.CreatedAt, r.DecidedAt, r.LetterId, r.Notes);
}

public sealed class PreachingLetterService(IPreachingLetterRepository letters, IUserRepository users, IChurchRepository churches, IHierarchicalAuthorizationService authorization, IFileStorage storage, IAuditLogRepository audit)
{
    public async Task<IReadOnlyCollection<PreachingLetterResponse>> ListAsync(Guid? userId, CancellationToken ct = default) =>
        (await letters.ListAsync(userId, ct)).Select(ToResponse).ToArray();

    public async Task<PreachingLetterValidationResponse?> ValidateAsync(Guid id, CancellationToken ct = default)
    {
        var letter = await letters.GetByIdAsync(id, ct);
        if (letter is null) return null;
        var preacher = await users.GetByIdAsync(letter.UserId, ct);
        var origin = await churches.GetByIdAsync(letter.ChurchId, ct);
        var destination = letter.DestinationChurchId is null ? origin : await churches.GetByIdAsync(letter.DestinationChurchId.Value, ct);
        var approver = await users.GetByIdAsync(letter.ApprovedByUserId, ct);
        return new PreachingLetterValidationResponse(letter.Id, letter.LetterNumber, preacher?.FullName ?? string.Empty, origin?.Name ?? string.Empty, destination?.Name ?? string.Empty, letter.ApprovedAt, letter.Status, letter.ApprovedAt, approver?.FullName ?? string.Empty);
    }

    public async Task<byte[]?> GetPdfAsync(Guid id, CancellationToken ct = default)
    {
        var letter = await letters.GetByIdAsync(id, ct);
        return letter is null ? null : await storage.ReadAsync(letter.PdfStoragePath, ct);
    }

    public async Task<PreachingLetterResponse?> SuspendAsync(Guid id, Guid? actorId = null, CancellationToken ct = default)
    {
        var letter = await letters.GetByIdAsync(id, ct);
        if (letter is null) return null;
        AuthorizationGuards.Ensure(await authorization.CanSuspendLetterAsync(actorId, letter, ct));
        letter.Suspended = true;
        await letters.SaveAsync(ct);
        await audit.AddAsync(AuditLog.Create(actorId, "LetterSuspended", nameof(PreachingLetter), id.ToString()), ct);
        return ToResponse(letter);
    }

    public async Task<PreachingLetterResponse?> RenewAsync(Guid id, Guid? actorId = null, CancellationToken ct = default)
    {
        var letter = await letters.GetByIdAsync(id, ct);
        if (letter is null) return null;
        AuthorizationGuards.Ensure(await authorization.CanSuspendLetterAsync(actorId, letter, ct));
        letter.ValidUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));
        letter.Suspended = false;
        await letters.SaveAsync(ct);
        await audit.AddAsync(AuditLog.Create(actorId, "LetterRenewed", nameof(PreachingLetter), id.ToString(), $"validUntil={letter.ValidUntil:O}"), ct);
        return ToResponse(letter);
    }

    private static PreachingLetterResponse ToResponse(PreachingLetter l) => new(l.Id, l.UserId, l.ChurchId, l.PreacherRequestId, l.LetterNumber, l.IssuedAt, l.ValidUntil, l.Suspended, l.QrCodeValue);
}

public sealed class AuditLogService(IAuditLogRepository audit, IHierarchicalAuthorizationService authorization)
{
    public async Task<IReadOnlyCollection<AuditLogResponse>> ListAsync(string? entityName, string? entityId, CancellationToken ct = default) =>
        (await audit.ListAsync(entityName, entityId, ct)).Select(a => new AuditLogResponse(a.Id, a.UserId, a.Action, a.EntityName, a.EntityId, a.Metadata, a.CreatedAt)).ToArray();

    public async Task<IReadOnlyCollection<AuditLogResponse>> ListAuthorizedAsync(Guid? actorId, string? entityName, string? entityId, CancellationToken ct = default)
    {
        AuthorizationGuards.Ensure(await authorization.CanViewAuditLogsAsync(actorId, ct));
        return await ListAsync(entityName, entityId, ct);
    }
}

public sealed class LeaderSignatureService(ILeaderSignatureRepository signatures, IFileStorage storage, IAuditLogRepository audit)
{
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase) { "image/png", "image/jpeg", "image/jpg" };

    public async Task<LeaderSignatureResponse?> GetAsync(Guid leaderId, CancellationToken ct = default) =>
        (await signatures.GetActiveByLeaderIdAsync(leaderId, ct)) is { } s ? ToResponse(s) : null;

    public async Task<LeaderSignatureResponse> SaveAsync(Guid leaderId, LeaderSignatureRequest request, CancellationToken ct = default)
    {
        if (!AllowedMimeTypes.Contains(request.MimeType)) throw new InvalidOperationException("Formato de assinatura inválido. Use PNG, JPG ou JPEG.");
        if (request.Content.Length > 5 * 1024 * 1024) throw new InvalidOperationException("Assinatura deve possuir no máximo 5MB.");
        foreach (var existing in await signatures.ListByLeaderIdAsync(leaderId, ct)) existing.Active = false;
        var extension = request.MimeType.Equals("image/png", StringComparison.OrdinalIgnoreCase) ? ".png" : ".jpg";
        var path = await storage.SaveAsync($"storage/signatures/{leaderId}/assinatura-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}{extension}", request.Content, request.MimeType, ct);
        var signature = new LeaderSignature { LeaderId = leaderId, StoragePath = path, MimeType = request.MimeType, Active = true };
        await signatures.AddAsync(signature, ct);
        await signatures.SaveAsync(ct);
        await audit.AddAsync(AuditLog.Create(leaderId, "LeaderSignatureSaved", nameof(LeaderSignature), signature.Id.ToString(), $"path={path}"), ct);
        return ToResponse(signature);
    }

    private static LeaderSignatureResponse ToResponse(LeaderSignature s) => new(s.Id, s.LeaderId, s.StoragePath, s.MimeType, s.CreatedAt, s.UpdatedAt, s.Active);
}

public sealed class PlainPdfPreachingLetterGenerator : IPreachingLetterPdfGenerator
{
    public Task<byte[]> GenerateAsync(PreachingLetterPdfModel model, CancellationToken cancellationToken = default)
    {
        var culture = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
        var date = model.Letter.ApprovedAt.ToOffset(TimeSpan.Zero).DateTime;
        var longDate = date.ToString("'Dia' dd 'de' MMMM 'de' yyyy", culture);
        var city = model.OriginChurch.City ?? model.ApproverChurch?.City ?? "Cidade configurável";
        var uf = model.OriginChurch.State ?? model.ApproverChurch?.State ?? "UF";
        var signature = model.SignatureBytes is null ? "________________________________\nDirigente" : "[imagem assinatura digital cadastrada]\n" + model.Approver.FullName + "\nDirigente";
        var via = $"CARTA DE RECOMENDAÇÃO PARA UM DIA\\n" +
            $"{model.OriginHierarchy.FirstOrDefault(c => c.Type == ChurchType.Sede)?.Name ?? model.OriginChurch.Name}\\n" +
            $"Regional: {model.OriginHierarchy.FirstOrDefault(c => c.Type == ChurchType.Regional)?.Name ?? ""}  Setorial: {model.OriginHierarchy.FirstOrDefault(c => c.Type == ChurchType.Setorial)?.Name ?? ""}\\n" +
            $"Carta nº {model.Letter.LetterNumber}  ID {model.Letter.Id}  Status {model.Letter.Status}\\n" +
            $"Recomendamos {model.Preacher.FullName}, cargo ministerial {model.Preacher.Role}, membro desde {model.Preacher.ChurchJoinedAt?.ToString("d", culture) ?? "data não informada"}, que congrega em {model.OriginChurch.Name} ({model.OriginChurch.Type}), {model.OriginChurch.City}/{model.OriginChurch.State}.\\n" +
            $"Para pregar em {model.DestinationChurch?.Name ?? model.OriginChurch.Name}, endereço {model.DestinationChurch?.Address ?? model.OriginChurch.Address ?? "endereço configurável"}, {model.DestinationChurch?.City ?? model.OriginChurch.City}/{model.DestinationChurch?.State ?? model.OriginChurch.State}.\\n" +
            $"{city}: {city} UF {uf}\\n{longDate}\\n" +
            $"QR CODE: {model.Letter.QrCodeValue}\\n\\n{signature}\\n" +
            $"Rodapé: {model.OriginChurch.Address} Telefone {model.OriginChurch.Phone} CNPJ {model.OriginChurch.Cnpj} {model.OriginChurch.InstitutionalInfo}\\n" +
            "Observações do verso: válida somente para um dia, mediante validação pública do QR Code.";
        var text = via + "\\n- - - - - - - - - - - - - separação vertical pontilhada - - - - - - - - - - - - -\\n" + via;
        return Task.FromResult(BuildPdf(text));
    }

    private static byte[] BuildPdf(string text)
    {
        static string Esc(string value) => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)").Replace("\r", "").Replace("\n", ") Tj T* (");
        var stream = $"BT /F1 9 Tf 40 800 Td ({Esc(text)}) Tj ET";
        var objects = new[] { "<< /Type /Catalog /Pages 2 0 R >>", "<< /Type /Pages /Kids [3 0 R] /Count 1 >>", $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>", "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>", $"<< /Length {System.Text.Encoding.UTF8.GetByteCount(stream)} >>\nstream\n{stream}\nendstream" };
        var sb = new System.Text.StringBuilder("%PDF-1.4\n"); var offsets = new List<int> { 0 };
        for (var i=0;i<objects.Length;i++){ offsets.Add(System.Text.Encoding.UTF8.GetByteCount(sb.ToString())); sb.Append(i+1).Append(" 0 obj\n").Append(objects[i]).Append("\nendobj\n"); }
        var xref = System.Text.Encoding.UTF8.GetByteCount(sb.ToString()); sb.Append("xref\n0 6\n0000000000 65535 f \n"); foreach(var o in offsets.Skip(1)) sb.Append(o.ToString("0000000000")).Append(" 00000 n \n"); sb.Append("trailer << /Size 6 /Root 1 0 R >>\nstartxref\n").Append(xref).Append("\n%%EOF");
        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }
}
