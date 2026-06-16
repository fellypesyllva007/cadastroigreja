using CadastroIgreja.Application.DTOs;
using CadastroIgreja.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadastroIgreja.Api.Controllers;

[ApiController]
[Route("api/letters")]
public class CartasController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartaPregacaoResponseDto>>> Get(CancellationToken ct) => Ok(await dbContext.CartasPregacao.AsNoTracking().Select(x => new CartaPregacaoResponseDto(x.Id, x.Numero, x.UsuarioId, x.DataEmissao, x.DataValidade, x.QrCode, x.PdfPath, x.Ativa)).ToListAsync(ct));

    [HttpGet("validate/{numero}")]
    public async Task<IActionResult> Validate(string numero, CancellationToken ct)
    {
        var carta = await dbContext.CartasPregacao.AsNoTracking().FirstOrDefaultAsync(x => x.Numero == numero && x.Ativa, ct);
        return carta is null ? NotFound(new { valid = false }) : Ok(new { valid = carta.DataValidade >= DateTime.UtcNow, letter = ToDto(carta) });
    }

    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        var carta = await dbContext.CartasPregacao.FindAsync([id], ct);
        if (carta is null) return NotFound();
        carta.Ativa = false;
        await dbContext.SaveChangesAsync(ct);
        return NoContent();
    }

    private static CartaPregacaoResponseDto ToDto(CadastroIgreja.Domain.Entities.CartaPregacao c) => new(c.Id, c.Numero, c.UsuarioId, c.DataEmissao, c.DataValidade, c.QrCode, c.PdfPath, c.Ativa);
}
