class Usuario {
  const Usuario({
    required this.id,
    required this.nomeCompleto,
    required this.email,
    required this.igrejaId,
    required this.ativo,
    this.telefone,
  });

  final String id;
  final String nomeCompleto;
  final String email;
  final String? telefone;
  final String igrejaId;
  final bool ativo;

  factory Usuario.fromJson(Map<String, Object?> json) {
    return Usuario(
      id: json['id'] as String? ?? '',
      nomeCompleto: json['nomeCompleto'] as String? ?? '',
      email: json['email'] as String? ?? '',
      telefone: json['telefone'] as String?,
      igrejaId: json['igrejaId'] as String? ?? '',
      ativo: json['ativo'] as bool? ?? false,
    );
  }
}
