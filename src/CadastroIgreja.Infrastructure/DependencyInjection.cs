using CadastroIgreja.Application;
using Microsoft.Extensions.DependencyInjection;

namespace CadastroIgreja.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services) => services
        .AddSingleton<IChurchRepository, InMemoryChurchRepository>()
        .AddSingleton<IUserRepository, InMemoryUserRepository>()
        .AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>()
        .AddSingleton<ITokenService, DemoTokenService>();
}
