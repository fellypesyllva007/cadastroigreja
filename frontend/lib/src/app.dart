import 'package:flutter/material.dart';

import 'config/app_config.dart';
import 'screens/login_screen.dart';
import 'services/api_client.dart';
import 'services/auth_service.dart';

class CadastroIgrejaApp extends StatelessWidget {
  const CadastroIgrejaApp({super.key});

  @override
  Widget build(BuildContext context) {
    final apiClient = ApiClient(baseUrl: AppConfig.apiBaseUrl);
    final authService = AuthService(apiClient);

    return MaterialApp(
      title: 'Cadastro Igreja',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF3156A3)),
        useMaterial3: true,
      ),
      home: LoginScreen(authService: authService, apiClient: apiClient),
    );
  }
}
