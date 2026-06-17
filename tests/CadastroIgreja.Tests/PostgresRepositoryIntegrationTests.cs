using CadastroIgreja.Application;
using CadastroIgreja.Domain;
using CadastroIgreja.Infrastructure;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CadastroIgreja.Tests;

public sealed class PostgresRepositoryIntegrationTests
{
    [Fact]
    public async Task Postgres_repositories_persist_church_user_request_and_audit_when_database_is_configured()
    {
        var connectionString = Environment.GetEnvironmentVariable("CADASTROIGREJA_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(connectionString)) return;

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using (var cleanup = new NpgsqlCommand("TRUNCATE audit_logs, preaching_letters, preacher_requests, role_change_requests, leader_signatures, users, churches RESTART IDENTITY CASCADE", connection))
        {
            await cleanup.ExecuteNonQueryAsync();
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Postgres"] = connectionString }).Build();
        var factory = new PostgresConnectionFactory(configuration);
        var churches = new PostgresChurchRepository(factory);
        var users = new PostgresUserRepository(factory);
        var audit = new PostgresAuditLogRepository(factory);
        var churchService = new ChurchService(churches, audit);
        var auth = new AuthService(users, churches, new Pbkdf2PasswordHasher(), new TestTokenService(), audit);

        var sedeId = await churchService.CreateAsync(new CreateChurchRequest("Sede Teste", ChurchType.Sede, null));
        var userId = await auth.RegisterAsync(new RegisterUserRequest("Persistente", "persistente@example.com", "SenhaForte123", sedeId, null));
        var user = await users.GetByIdAsync(userId);
        user!.Status = UserStatus.Approved;
        await users.SaveAsync();

        Assert.Equal(UserStatus.Approved, (await users.GetByEmailAsync("persistente@example.com"))!.Status);
        Assert.Contains(await audit.ListAsync(nameof(User), userId.ToString()), log => log.Action == "UserRegistered");
    }
}
