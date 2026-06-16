using CadastroIgreja.Application.DTOs;
using CadastroIgreja.Application.Interfaces;
using CadastroIgreja.Domain.Entities;
using CadastroIgreja.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadastroIgreja.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService, AppDbContext dbContext) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        return response is null ? Unauthorized(new { message = "Credenciais inválidas." }) : Ok(response);
    }

    [HttpPost("register")]
    public async Task<ActionResult<UsuarioResponseDto>> Register(RegisterUsuarioDto request, CancellationToken cancellationToken)
    {
        if (await dbContext.Usuarios.AnyAsync(x => x.Email == request.Email, cancellationToken)) return Conflict(new { message = "E-mail já cadastrado." });
        if (!await dbContext.Igrejas.AnyAsync(x => x.Id == request.IgrejaId, cancellationToken)) return BadRequest(new { message = "Igreja inválida." });

        var usuario = new Usuario { NomeCompleto = request.NomeCompleto, Email = request.Email, Telefone = request.Telefone, IgrejaId = request.IgrejaId, SenhaHash = authService.HashSenha(request.Senha) };
        dbContext.Usuarios.Add(usuario);
        var membro = await dbContext.Cargos.FirstOrDefaultAsync(x => x.Nome == "Membro", cancellationToken);
        if (membro is not null) dbContext.UsuarioCargos.Add(new UsuarioCargo { UsuarioId = usuario.Id, CargoId = membro.Id });
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(UsuariosController.GetById), "Usuarios", new { id = usuario.Id }, ToDto(usuario));
    }

    private static UsuarioResponseDto ToDto(Usuario usuario) => new(usuario.Id, usuario.NomeCompleto, usuario.Email, usuario.Telefone, usuario.IgrejaId, usuario.Ativo, usuario.CriadoEm);
}
