using CadastroIgreja.Application.DTOs;
using CadastroIgreja.Domain.Entities;
using CadastroIgreja.Domain.Enums;
using CadastroIgreja.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadastroIgreja.Api.Controllers;

[ApiController]
[Route("api/preacher-requests")]
public class SolicitacoesPregadorController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SolicitacaoPregadorResponseDto>>> Get(CancellationToken ct) => Ok(await dbContext.SolicitacoesPregador.AsNoTracking().Select(x => new SolicitacaoPregadorResponseDto(x.Id, x.UsuarioId, x.Status.ToString(), x.Observacao, x.DataSolicitacao)).ToListAsync(ct));

    [HttpPost("usuarios/{usuarioId:guid}")]
    public async Task<ActionResult<SolicitacaoPregadorResponseDto>> Create(Guid usuarioId, SolicitacaoPregadorCreateDto request, CancellationToken ct)
    {
        if (!await dbContext.Usuarios.AnyAsync(x => x.Id == usuarioId, ct)) return NotFound(new { message = "Usuário não encontrado." });
        var solicitacao = new SolicitacaoPregador { UsuarioId = usuarioId, Observacao = request.Observacao };
        dbContext.SolicitacoesPregador.Add(solicitacao);
        await dbContext.SaveChangesAsync(ct);
        return Created($"/api/preacher-requests/{solicitacao.Id}", ToDto(solicitacao));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, AprovarSolicitacaoDto request, CancellationToken ct)
    {
        var solicitacao = await dbContext.SolicitacoesPregador.FindAsync([id], ct);
        if (solicitacao is null) return NotFound();
        solicitacao.Status = solicitacao.Status switch
        {
            StatusSolicitacaoPregador.Pendente => StatusSolicitacaoPregador.AprovadoCasaOracao,
            StatusSolicitacaoPregador.AprovadoCasaOracao => StatusSolicitacaoPregador.AprovadoCongregacao,
            _ => StatusSolicitacaoPregador.AprovadoSetorial
        };
        dbContext.Aprovacoes.Add(new Aprovacao { SolicitacaoPregadorId = id, AprovadorId = request.AprovadorId, StatusGerado = solicitacao.Status, Observacao = request.Observacao });
        if (solicitacao.Status == StatusSolicitacaoPregador.AprovadoSetorial) EmitirCarta(solicitacao);
        await dbContext.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, RejeitarSolicitacaoDto request, CancellationToken ct)
    {
        var solicitacao = await dbContext.SolicitacoesPregador.FindAsync([id], ct);
        if (solicitacao is null) return NotFound();
        solicitacao.Status = StatusSolicitacaoPregador.Rejeitado;
        dbContext.Aprovacoes.Add(new Aprovacao { SolicitacaoPregadorId = id, AprovadorId = request.AprovadorId, StatusGerado = solicitacao.Status, Observacao = request.Motivo });
        await dbContext.SaveChangesAsync(ct);
        return NoContent();
    }

    private void EmitirCarta(SolicitacaoPregador solicitacao)
    {
        if (dbContext.CartasPregacao.Any(x => x.SolicitacaoPregadorId == solicitacao.Id)) return;
        var numero = $"CP-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        dbContext.CartasPregacao.Add(new CartaPregacao { UsuarioId = solicitacao.UsuarioId, SolicitacaoPregadorId = solicitacao.Id, Numero = numero, QrCode = $"/api/letters/validate/{numero}" });
    }

    private static SolicitacaoPregadorResponseDto ToDto(SolicitacaoPregador s) => new(s.Id, s.UsuarioId, s.Status.ToString(), s.Observacao, s.DataSolicitacao);
}
