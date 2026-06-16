using CadastroIgreja.Application.DTOs;
using CadastroIgreja.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadastroIgreja.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Route("api/users")]
public class UsuariosController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> Get(CancellationToken ct) => Ok(await dbContext.Usuarios.AsNoTracking().OrderBy(x => x.NomeCompleto).Select(x => new UsuarioResponseDto(x.Id, x.NomeCompleto, x.Email, x.Telefone, x.IgrejaId, x.Ativo, x.CriadoEm)).ToListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UsuarioResponseDto>> GetById(Guid id, CancellationToken ct)
    {
        var usuario = await dbContext.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return usuario is null ? NotFound() : Ok(new UsuarioResponseDto(usuario.Id, usuario.NomeCompleto, usuario.Email, usuario.Telefone, usuario.IgrejaId, usuario.Ativo, usuario.CriadoEm));
    }

    [HttpPost("{id:guid}/cargos")]
    public async Task<IActionResult> AtribuirCargo(Guid id, AtualizarCargoDto request, CancellationToken ct)
    {
        if (!await dbContext.Usuarios.AnyAsync(x => x.Id == id, ct) || !await dbContext.Cargos.AnyAsync(x => x.Id == request.CargoId, ct)) return NotFound();
        if (!await dbContext.UsuarioCargos.AnyAsync(x => x.UsuarioId == id && x.CargoId == request.CargoId, ct)) dbContext.UsuarioCargos.Add(new() { UsuarioId = id, CargoId = request.CargoId });
        await dbContext.SaveChangesAsync(ct);
        return NoContent();
    }
}
