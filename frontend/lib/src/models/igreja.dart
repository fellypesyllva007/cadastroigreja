class Igreja {
  const Igreja({required this.id, required this.nome, required this.tipo, required this.ativa, this.parentId});

  final String id;
  final String nome;
  final String tipo;
  final String? parentId;
  final bool ativa;

  factory Igreja.fromJson(Map<String, Object?> json) {
    return Igreja(
      id: json['id'] as String? ?? '',
      nome: json['nome'] as String? ?? '',
      tipo: json['tipo'] as String? ?? '',
      parentId: json['parentId'] as String?,
      ativa: json['ativa'] as bool? ?? false,
    );
  }
}
