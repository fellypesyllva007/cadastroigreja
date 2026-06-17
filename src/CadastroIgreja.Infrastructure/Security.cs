using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CadastroIgreja.Application;
using CadastroIgreja.Domain;
using Microsoft.Extensions.Configuration;

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

public sealed class HmacJwtTokenService(IConfiguration configuration) : ITokenService
{
    private const int AccessTokenSeconds = 3600;

    public AuthTokenResponse Create(User user)
    {
        var now = DateTimeOffset.UtcNow;
        var header = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new { alg = "HS256", typ = "JWT" }));
        var payload = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, object>
        {
            ["sub"] = user.Id.ToString(),
            ["email"] = user.Email,
            ["role"] = user.Role.ToString(),
            ["churchId"] = user.ChurchId.ToString(),
            ["iat"] = now.ToUnixTimeSeconds(),
            ["exp"] = now.AddSeconds(AccessTokenSeconds).ToUnixTimeSeconds()
        }));
        var unsigned = $"{header}.{payload}";
        var signature = Base64Url(HMACSHA256.HashData(SigningKey(configuration), Encoding.UTF8.GetBytes(unsigned)));
        return new AuthTokenResponse($"{unsigned}.{signature}", Base64Url(RandomNumberGenerator.GetBytes(32)), AccessTokenSeconds);
    }

    public static bool TryValidate(string token, IConfiguration configuration, out JwtPrincipal principal)
    {
        principal = default;
        var parts = token.Split('.');
        if (parts.Length != 3) return false;
        var unsigned = $"{parts[0]}.{parts[1]}";
        var expected = Base64Url(HMACSHA256.HashData(SigningKey(configuration), Encoding.UTF8.GetBytes(unsigned)));
        if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(parts[2]))) return false;

        using var payload = JsonDocument.Parse(Base64UrlDecode(parts[1]));
        if (!payload.RootElement.TryGetProperty("sub", out var sub) || !Guid.TryParse(sub.GetString(), out var userId)) return false;
        if (!payload.RootElement.TryGetProperty("email", out var email)) return false;
        if (!payload.RootElement.TryGetProperty("exp", out var exp) || DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64()) <= DateTimeOffset.UtcNow) return false;
        principal = new JwtPrincipal(userId, email.GetString() ?? string.Empty);
        return true;
    }

    private static byte[] SigningKey(IConfiguration configuration)
    {
        var secret = configuration["Jwt:Secret"] ?? Environment.GetEnvironmentVariable("CADASTROIGREJA_JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32)
        {
            throw new InvalidOperationException("Configure Jwt:Secret (ou CADASTROIGREJA_JWT_SECRET) com pelo menos 32 caracteres para assinar tokens.");
        }
        return Encoding.UTF8.GetBytes(secret);
    }

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        return Convert.FromBase64String(padded);
    }
}

public readonly record struct JwtPrincipal(Guid UserId, string Email);
