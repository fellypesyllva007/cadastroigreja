using CadastroIgreja.Application.DTOs;
using CadastroIgreja.Domain.Entities;
using CadastroIgreja.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadastroIgreja.Api.Controllers;

[ApiController]
[Route("api/cargos")]
public class CargosController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CargoResponseDto>>> Get(CancellationToken ct) => Ok(await dbContext.Cargos.AsNoTracking().OrderBy(x => x.Nome).Select(x => new CargoResponseDto(x.Id, x.Nome, x.Ativo)).ToListAsync(ct));

    [HttpPost]
    public async Task<ActionResult<CargoResponseDto>> Create(CargoCreateDto request, CancellationToken ct)
    {
        var cargo = new Cargo { Nome = request.Nome };
        dbContext.Cargos.Add(cargo);
        await dbContext.SaveChangesAsync(ct);
        return Created($"/api/cargos/{cargo.Id}", new CargoResponseDto(cargo.Id, cargo.Nome, cargo.Ativo));
    }
}
