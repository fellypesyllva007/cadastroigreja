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
        var service = new AuthService(users, churches, new Pbkdf2PasswordHasher(), new DemoTokenService(), audit);
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
        await users.AddAsync(user);

        var service = new PreacherRequestService(preacherRequests, letters, users, churches, audit);
        var requestId = await service.CreateAsync(new CreatePreacherRequest(user.Id, "Chamado validado"));

        var first = await service.ApproveAsync(requestId);
        var second = await service.ApproveAsync(requestId);
        var third = await service.ApproveAsync(requestId);

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
        await users.AddAsync(user);

        var service = new RoleChangeRequestService(requests, users, audit);
        var requestId = await service.CreateAsync(new CreateRoleChangeRequest(user.Id, MemberRole.Diacono, "Serviço local"));
        var approved = await service.ApproveAsync(requestId, Guid.NewGuid());

        Assert.True(approved);
        Assert.Equal(MemberRole.Diacono, (await users.GetByIdAsync(user.Id))!.Role);
        Assert.Contains(await audit.ListAsync(nameof(RoleChangeRequest), requestId.ToString()), log => log.Action == "RoleChangeApproved");
    }
}
