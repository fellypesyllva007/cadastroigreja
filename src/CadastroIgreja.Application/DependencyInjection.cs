using Microsoft.Extensions.DependencyInjection;

namespace CadastroIgreja.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddScoped<AuthService>()
        .AddScoped<ChurchService>()
        .AddScoped<IHierarchicalAuthorizationService, HierarchicalAuthorizationService>()
        .AddScoped<UserService>()
        .AddScoped<RoleChangeRequestService>()
        .AddScoped<PreacherRequestService>()
        .AddScoped<PreachingLetterService>()
        .AddScoped<LeaderSignatureService>()
        .AddSingleton<IPreachingLetterPdfGenerator, PlainPdfPreachingLetterGenerator>()
        .AddScoped<AuditLogService>();
}
