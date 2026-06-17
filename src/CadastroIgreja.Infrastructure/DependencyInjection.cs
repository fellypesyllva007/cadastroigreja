using CadastroIgreja.Application;
using Microsoft.Extensions.DependencyInjection;

namespace CadastroIgreja.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services) => services
        .AddSingleton<PostgresConnectionFactory>()
        .AddScoped<IChurchRepository, PostgresChurchRepository>()
        .AddScoped<IUserRepository, PostgresUserRepository>()
        .AddScoped<IRoleChangeRequestRepository, PostgresRoleChangeRequestRepository>()
        .AddScoped<IPreacherRequestRepository, PostgresPreacherRequestRepository>()
        .AddScoped<IPreachingLetterRepository, PostgresPreachingLetterRepository>()
        .AddScoped<ILeaderSignatureRepository, PostgresLeaderSignatureRepository>()
        .AddSingleton<IFileStorage, LocalFileStorage>()
        .AddScoped<IAuditLogRepository, PostgresAuditLogRepository>()
        .AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>()
        .AddSingleton<ITokenService, HmacJwtTokenService>();
}
