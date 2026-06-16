using CadastroIgreja.Application.DTOs;

namespace CadastroIgreja.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    string GerarToken(Guid usuarioId, string email, string nomeCompleto);
    string HashSenha(string senha);
    bool VerificarSenha(string senha, string senhaHash);
}
