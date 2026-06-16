using CadastroIgreja.Application.DTOs;
using CadastroIgreja.Domain.Entities;
using CadastroIgreja.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadastroIgreja.Api.Controllers;

[ApiController]
[Route("api/igrejas")]
[Route("api/churches")]
public class IgrejasController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IgrejaResponseDto>>> Get(CancellationToken ct) => Ok(await dbContext.Igrejas.AsNoTracking().OrderBy(x => x.Tipo).ThenBy(x => x.Nome).Select(x => new IgrejaResponseDto(x.Id, x.Nome, x.Tipo, x.ParentId, x.Ativa)).ToListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<IgrejaResponseDto>> GetById(Guid id, CancellationToken ct)
    {
        var igreja = await dbContext.Igrejas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return igreja is null ? NotFound() : Ok(ToDto(igreja));
    }

    [HttpPost]
    public async Task<ActionResult<IgrejaResponseDto>> Create(IgrejaCreateDto request, CancellationToken ct)
    {
        if (request.ParentId.HasValue && !await dbContext.Igrejas.AnyAsync(x => x.Id == request.ParentId, ct)) return BadRequest(new { message = "Igreja superior inválida." });
        var igreja = new Igreja { Nome = request.Nome, Tipo = request.Tipo, ParentId = request.ParentId };
        dbContext.Igrejas.Add(igreja);
        await dbContext.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = igreja.Id }, ToDto(igreja));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, IgrejaCreateDto request, CancellationToken ct)
    {
        var igreja = await dbContext.Igrejas.FindAsync([id], ct);
        if (igreja is null) return NotFound();
        igreja.Nome = request.Nome; igreja.Tipo = request.Tipo; igreja.ParentId = request.ParentId;
        await dbContext.SaveChangesAsync(ct);
        return NoContent();
    }

    private static IgrejaResponseDto ToDto(Igreja igreja) => new(igreja.Id, igreja.Nome, igreja.Tipo, igreja.ParentId, igreja.Ativa);
}
