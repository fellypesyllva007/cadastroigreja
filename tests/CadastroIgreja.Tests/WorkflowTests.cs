using CadastroIgreja.Application;
using CadastroIgreja.Domain;
using CadastroIgreja.Infrastructure;

namespace CadastroIgreja.Tests;

public sealed class WorkflowTests
{
    [Fact]
    public async Task Church_hierarchy_rejects_invalid_parent_type()
    {
        var churches = new InMemoryChurchRepository();
        var audit = new InMemoryAuditLogRepository();
        var service = new ChurchService(churches, audit);

        var sedeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var regionalId = await service.CreateAsync(new CreateChurchRequest("Regional Norte", ChurchType.Regional, sedeId));

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateChurchRequest("Casa sem local", ChurchType.CasaOracao, regionalId)));

        Assert.Contains("CongregacaoLocal", error.Message);
    }

    [Fact]
    public async Task Registration_requires_existing_church_and_unique_email()
    {
        var churches = new InMemoryChurchRepository();
        var users = new InMemoryUserRepository();
        var audit = new InMemoryAuditLogRepository();
        var service = new AuthService(users, churches, new Pbkdf2PasswordHasher(), new TestTokenService(), audit);
        var churchId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var userId = await service.RegisterAsync(new RegisterUserRequest("Maria Silva", "maria@example.com", "SenhaForte123", churchId, null));

        Assert.NotEqual(Guid.Empty, userId);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(new RegisterUserRequest("Maria Silva", "MARIA@example.com", "SenhaForte123", churchId, null)));
    }

    [Fact]
    public async Task Preacher_request_for_casa_oracao_requires_three_approvals_and_issues_letter()
    {
        var churches = new InMemoryChurchRepository();
        var users = new InMemoryUserRepository();
        var roleRequests = new InMemoryRoleChangeRequestRepository();
        var preacherRequests = new InMemoryPreacherRequestRepository();
        var letters = new InMemoryPreachingLetterRepository();
        var audit = new InMemoryAuditLogRepository();

        var churchService = new ChurchService(churches, audit);
        var sedeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var regionalId = await churchService.CreateAsync(new CreateChurchRequest("Regional", ChurchType.Regional, sedeId));
        var setorialId = await churchService.CreateAsync(new CreateChurchRequest("Setorial", ChurchType.Setorial, regionalId));
        var localId = await churchService.CreateAsync(new CreateChurchRequest("Local", ChurchType.CongregacaoLocal, setorialId));
        var casaId = await churchService.CreateAsync(new CreateChurchRequest("Casa", ChurchType.CasaOracao, localId));

        var user = new User { FullName = "João", Email = "joao@example.com", PasswordHash = "hash", ChurchId = casaId, Status = UserStatus.Approved };
        var approver = new User { FullName = "Pastor", Email = "pastor@example.com", PasswordHash = "hash", ChurchId = localId, Status = UserStatus.Approved, Role = MemberRole.Pastor };
        await users.AddAsync(user);
        await users.AddAsync(approver);

        var service = new PreacherRequestService(preacherRequests, letters, users, churches, new InMemoryLeaderSignatureRepository(), new LocalFileStorage(), new PlainPdfPreachingLetterGenerator(), audit);
        var requestId = await service.CreateAsync(new CreatePreacherRequest(user.Id, "Chamado validado"));

        var first = await service.ApproveAsync(requestId, approver.Id);
        var second = await service.ApproveAsync(requestId, approver.Id);
        var third = await service.ApproveAsync(requestId, approver.Id);

        Assert.Equal(PreacherApprovalStep.CongregacaoLocal, first!.CurrentStep);
        Assert.Equal(PreacherApprovalStep.Setorial, second!.CurrentStep);
        Assert.Equal(RequestStatus.Approved, third!.Status);
        Assert.Equal(PreacherApprovalStep.Completed, third.CurrentStep);
        Assert.NotNull(third.LetterId);
        Assert.Single(await letters.ListAsync(user.Id));
    }

    [Fact]
    public async Task Role_change_approval_updates_member_role_and_audit_log()
    {
        var users = new InMemoryUserRepository();
        var requests = new InMemoryRoleChangeRequestRepository();
        var audit = new InMemoryAuditLogRepository();
        var user = new User { FullName = "Ana", Email = "ana@example.com", PasswordHash = "hash", ChurchId = Guid.NewGuid(), Status = UserStatus.Approved };
        var approver = new User { FullName = "Dirigente", Email = "dirigente@example.com", PasswordHash = "hash", ChurchId = user.ChurchId, Status = UserStatus.Approved, Role = MemberRole.Dirigente };
        await users.AddAsync(user);
        await users.AddAsync(approver);

        var service = new RoleChangeRequestService(requests, users, audit);
        var requestId = await service.CreateAsync(new CreateRoleChangeRequest(user.Id, MemberRole.Diacono, "Serviço local"));
        var approved = await service.ApproveAsync(requestId, approver.Id);

        Assert.True(approved);
        Assert.Equal(MemberRole.Diacono, (await users.GetByIdAsync(user.Id))!.Role);
        Assert.Contains(await audit.ListAsync(nameof(RoleChangeRequest), requestId.ToString()), log => log.Action == "RoleChangeApproved");
    }
    [Fact]
    public async Task Role_change_approval_rejects_member_without_ministerial_authority()
    {
        var users = new InMemoryUserRepository();
        var requests = new InMemoryRoleChangeRequestRepository();
        var audit = new InMemoryAuditLogRepository();
        var user = new User { FullName = "Bruno", Email = "bruno@example.com", PasswordHash = "hash", ChurchId = Guid.NewGuid(), Status = UserStatus.Approved };
        var memberApprover = new User { FullName = "Membro", Email = "membro@example.com", PasswordHash = "hash", ChurchId = user.ChurchId, Status = UserStatus.Approved, Role = MemberRole.Membro };
        await users.AddAsync(user);
        await users.AddAsync(memberApprover);

        var service = new RoleChangeRequestService(requests, users, audit);
        var requestId = await service.CreateAsync(new CreateRoleChangeRequest(user.Id, MemberRole.Presbitero, "Teste negativo"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ApproveAsync(requestId, memberApprover.Id));
        Assert.Equal(MemberRole.Membro, (await users.GetByIdAsync(user.Id))!.Role);
    }

    [Fact]
    public async Task Final_approval_generates_pdf_with_approved_at_qr_code_and_missing_signature_audit()
    {
        var churches = new InMemoryChurchRepository();
        var users = new InMemoryUserRepository();
        var preacherRequests = new InMemoryPreacherRequestRepository();
        var letters = new InMemoryPreachingLetterRepository();
        var signatures = new InMemoryLeaderSignatureRepository();
        var storage = new LocalFileStorage();
        var audit = new InMemoryAuditLogRepository();
        var churchId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var preacher = new User { FullName = "Pregador Dinâmico", Email = "pregador@example.com", PasswordHash = "hash", ChurchId = churchId, Status = UserStatus.Approved, ChurchJoinedAt = new DateOnly(2020, 2, 14) };
        var approver = new User { FullName = "Dirigente Dinâmico", Email = "dirigente2@example.com", PasswordHash = "hash", ChurchId = churchId, Status = UserStatus.Approved, Role = MemberRole.Dirigente };
        await users.AddAsync(preacher);
        await users.AddAsync(approver);
        var service = new PreacherRequestService(preacherRequests, letters, users, churches, signatures, storage, new PlainPdfPreachingLetterGenerator(), audit);

        var requestId = await service.CreateAsync(new CreatePreacherRequest(preacher.Id));
        var approved = await service.ApproveAsync(requestId, approver.Id);

        var letter = Assert.Single(await letters.ListAsync(preacher.Id));
        var pdf = await storage.ReadAsync(letter.PdfStoragePath);
        Assert.Equal(approved!.DecidedAt, letter.ApprovedAt);
        Assert.StartsWith("%PDF", System.Text.Encoding.UTF8.GetString(pdf!));
        Assert.Contains(letter.Id.ToString(), letter.QrCodeValue);
        Assert.Contains(await audit.ListAsync(nameof(LeaderSignature), approver.Id.ToString()), log => log.Action == "LeaderSignatureMissing");
    }

    [Fact]
    public async Task Leader_signature_replacement_keeps_only_one_active_signature()
    {
        var repo = new InMemoryLeaderSignatureRepository();
        var service = new LeaderSignatureService(repo, new LocalFileStorage(), new InMemoryAuditLogRepository());
        var leaderId = Guid.NewGuid();

        await service.SaveAsync(leaderId, new LeaderSignatureRequest("assinatura.png", "image/png", new byte[] { 1, 2, 3 }));
        var active = await service.SaveAsync(leaderId, new LeaderSignatureRequest("assinatura.jpg", "image/jpeg", new byte[] { 4, 5, 6 }));

        Assert.True(active.Active);
        Assert.Single((await repo.ListByLeaderIdAsync(leaderId)).Where(s => s.Active));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveAsync(leaderId, new LeaderSignatureRequest("assinatura.gif", "image/gif", new byte[] { 1 })));
    }

}


internal sealed class TestTokenService : ITokenService
{
    public AuthTokenResponse Create(User user) => new("test-access-token", "test-refresh-token", 3600);
}
