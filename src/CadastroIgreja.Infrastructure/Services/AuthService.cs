using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CadastroIgreja.Application.DTOs;
using CadastroIgreja.Application.Interfaces;
using CadastroIgreja.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CadastroIgreja.Infrastructure.Services;

public class AuthService(IConfiguration configuration, AppDbContext dbContext) : IAuthService
{
    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var usuario = await dbContext.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Email == request.Email && x.Ativo, cancellationToken);
        if (usuario is null || !VerificarSenha(request.Senha, usuario.SenhaHash)) return null;
        var dto = new UsuarioResponseDto(usuario.Id, usuario.NomeCompleto, usuario.Email, usuario.Telefone, usuario.IgrejaId, usuario.Ativo, usuario.CriadoEm);
        return new LoginResponseDto(GerarToken(usuario.Id, usuario.Email, usuario.NomeCompleto), dto);
    }

    public string GerarToken(Guid usuarioId, string email, string nomeCompleto)
    {
        var jwt = configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada.")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Name, nomeCompleto)
        };
        var token = new JwtSecurityToken(jwt["Issuer"], jwt["Audience"], claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string HashSenha(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerificarSenha(string senha, string senhaHash)
    {
        var parts = senhaHash.Split('.', 2);
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(senha, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
