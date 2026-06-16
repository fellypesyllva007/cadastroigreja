import 'usuario.dart';

class LoginRequest {
  const LoginRequest({required this.email, required this.senha});

  final String email;
  final String senha;

  Map<String, Object?> toJson() => {'email': email, 'senha': senha};
}

class LoginResponse {
  const LoginResponse({required this.token, required this.usuario});

  final String token;
  final Usuario usuario;

  factory LoginResponse.fromJson(Map<String, Object?> json) {
    return LoginResponse(
      token: json['token'] as String? ?? '',
      usuario: Usuario.fromJson(json['usuario'] as Map<String, Object?>? ?? const {}),
    );
  }
}
