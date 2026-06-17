import 'package:flutter/material.dart';

import 'src/screens/app_shell.dart';
import 'src/services/api_client.dart';
import 'src/services/session_store.dart';

void main() {
  runApp(CadastroIgrejaApp(
    apiClient: ApiClient.fromEnvironment(),
    sessionStore: SessionStore(),
  ));
}

class CadastroIgrejaApp extends StatelessWidget {
  const CadastroIgrejaApp({
    super.key,
    required this.apiClient,
    required this.sessionStore,
  });

  final ApiClient apiClient;
  final SessionStore sessionStore;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'CadastroIgreja',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF1F6FEB)),
        useMaterial3: true,
      ),
      home: AppShell(apiClient: apiClient, sessionStore: sessionStore),
    );
  }
}
