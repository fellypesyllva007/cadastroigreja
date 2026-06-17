using CadastroIgreja.Application.DTOs;
using CadastroIgreja.Domain.Entities;
using CadastroIgreja.Domain.Enums;
using CadastroIgreja.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadastroIgreja.Api.Controllers;

[ApiController]
[Route("api/role-requests")]
public class SolicitacoesCargoController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SolicitacaoCargoResponseDto>>> Get(CancellationToken ct)
    {
        return Ok(await dbContext.SolicitacoesCargo.AsNoTracking()
            .OrderByDescending(x => x.CriadoEm)
            .Select(x => new SolicitacaoCargoResponseDto(x.Id, x.UsuarioId, x.CargoSolicitadoId, x.Status.ToString(), x.Justificativa, x.ObservacaoAprovacao, x.AprovadorId, x.CriadoEm, x.AvaliadoEm))
            .ToListAsync(ct));
    }

    [HttpPost("usuarios/{usuarioId:guid}")]
    public async Task<ActionResult<SolicitacaoCargoResponseDto>> Create(Guid usuarioId, SolicitacaoCargoCreateDto request, CancellationToken ct)
    {
        if (!await dbContext.Usuarios.AnyAsync(x => x.Id == usuarioId, ct)) return NotFound(new { message = "Usuário não encontrado." });
        if (!await dbContext.Cargos.AnyAsync(x => x.Id == request.CargoSolicitadoId && x.Ativo, ct)) return BadRequest(new { message = "Cargo inválido." });

        var solicitacao = new SolicitacaoCargo { UsuarioId = usuarioId, CargoSolicitadoId = request.CargoSolicitadoId, Justificativa = request.Justificativa };
        dbContext.SolicitacoesCargo.Add(solicitacao);
        await dbContext.SaveChangesAsync(ct);
        return Created($"/api/role-requests/{solicitacao.Id}", ToDto(solicitacao));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, AvaliarSolicitacaoCargoDto request, CancellationToken ct)
    {
        var solicitacao = await dbContext.SolicitacoesCargo.FindAsync([id], ct);
        if (solicitacao is null) return NotFound();
        solicitacao.Status = StatusSolicitacaoCargo.Aprovado;
        solicitacao.AprovadorId = request.AprovadorId;
        solicitacao.ObservacaoAprovacao = request.Observacao;
        solicitacao.AvaliadoEm = DateTime.UtcNow;
        if (!await dbContext.UsuarioCargos.AnyAsync(x => x.UsuarioId == solicitacao.UsuarioId && x.CargoId == solicitacao.CargoSolicitadoId, ct))
        {
            dbContext.UsuarioCargos.Add(new UsuarioCargo { UsuarioId = solicitacao.UsuarioId, CargoId = solicitacao.CargoSolicitadoId });
        }
        await dbContext.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, AvaliarSolicitacaoCargoDto request, CancellationToken ct)
    {
        var solicitacao = await dbContext.SolicitacoesCargo.FindAsync([id], ct);
        if (solicitacao is null) return NotFound();
        solicitacao.Status = StatusSolicitacaoCargo.Rejeitado;
        solicitacao.AprovadorId = request.AprovadorId;
        solicitacao.ObservacaoAprovacao = request.Observacao;
        solicitacao.AvaliadoEm = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);
        return NoContent();
    }

    private static SolicitacaoCargoResponseDto ToDto(SolicitacaoCargo s) => new(s.Id, s.UsuarioId, s.CargoSolicitadoId, s.Status.ToString(), s.Justificativa, s.ObservacaoAprovacao, s.AprovadorId, s.CriadoEm, s.AvaliadoEm);
}
