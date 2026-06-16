class CartaPregacao {
  const CartaPregacao({required this.id, required this.numero, required this.usuarioId, required this.ativa});

  final String id;
  final String numero;
  final String usuarioId;
  final bool ativa;

  factory CartaPregacao.fromJson(Map<String, Object?> json) {
    return CartaPregacao(
      id: json['id'] as String? ?? '',
      numero: json['numero'] as String? ?? '',
      usuarioId: json['usuarioId'] as String? ?? '',
      ativa: json['ativa'] as bool? ?? false,
    );
  }
}
