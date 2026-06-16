namespace CadastroIgreja.Application.DTOs;

public sealed record LoginRequestDto(string Email, string Senha);
public sealed record LoginResponseDto(string Token, UsuarioResponseDto Usuario);
public sealed record RegisterUsuarioDto(string NomeCompleto, string Email, string Senha, Guid IgrejaId, string? Telefone);
