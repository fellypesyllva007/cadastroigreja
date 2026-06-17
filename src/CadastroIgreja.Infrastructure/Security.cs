using System.Security.Cryptography;
using System.Text;
using CadastroIgreja.Application;
using CadastroIgreja.Domain;

namespace CadastroIgreja.Infrastructure;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"pbkdf2${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split('$');
        if (parts.Length != 3 || parts[0] != "pbkdf2") return false;
        var salt = Convert.FromBase64String(parts[1]);
        var expected = Convert.FromBase64String(parts[2]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}

public sealed class DemoTokenService : ITokenService
{
    public AuthTokenResponse Create(User user)
    {
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.Id}|{user.Email}|{DateTimeOffset.UtcNow:O}"));
        return new AuthTokenResponse($"demo.{payload}.signature", Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)), 3600);
    }
}
