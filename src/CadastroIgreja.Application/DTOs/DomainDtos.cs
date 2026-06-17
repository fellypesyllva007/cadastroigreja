using CadastroIgreja.Domain.Enums;

namespace CadastroIgreja.Application.DTOs;

public sealed record IgrejaCreateDto(string Nome, NivelHierarquico Tipo, Guid? ParentId);
public sealed record IgrejaResponseDto(Guid Id, string Nome, NivelHierarquico Tipo, Guid? ParentId, bool Ativa);

public sealed record CargoCreateDto(string Nome);
public sealed record CargoResponseDto(Guid Id, string Nome, bool Ativo);

public sealed record UsuarioResponseDto(Guid Id, string NomeCompleto, string Email, string? Telefone, Guid IgrejaId, bool Ativo, DateTime CriadoEm);
public sealed record AtualizarCargoDto(Guid CargoId);

public sealed record SolicitacaoCargoCreateDto(Guid CargoSolicitadoId, string? Justificativa);
public sealed record SolicitacaoCargoResponseDto(Guid Id, Guid UsuarioId, Guid CargoSolicitadoId, string Status, string? Justificativa, string? ObservacaoAprovacao, Guid? AprovadorId, DateTime CriadoEm, DateTime? AvaliadoEm);
public sealed record AvaliarSolicitacaoCargoDto(Guid AprovadorId, string? Observacao);

public sealed record SolicitacaoPregadorCreateDto(string? Observacao);
public sealed record SolicitacaoPregadorResponseDto(Guid Id, Guid UsuarioId, string Status, string? Observacao, DateTime DataSolicitacao);
public sealed record AprovarSolicitacaoDto(Guid AprovadorId, string? Observacao);
public sealed record RejeitarSolicitacaoDto(Guid AprovadorId, string Motivo);

public sealed record CartaPregacaoResponseDto(Guid Id, string Numero, Guid UsuarioId, DateTime DataEmissao, DateTime DataValidade, string QrCode, string? PdfPath, bool Ativa);
