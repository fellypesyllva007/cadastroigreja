import 'package:flutter/material.dart';

import '../services/api_client.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key, required this.apiClient, required this.onAuthenticated});

  final ApiClient apiClient;
  final ValueChanged<Map<String, dynamic>> onAuthenticated;

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _email = TextEditingController(text: 'admin@cadastroigreja.local');
  final _password = TextEditingController();
  bool _busy = false;

  Future<void> _login() async {
    setState(() => _busy = true);
    try {
      final response = await widget.apiClient.login(email: _email.text, password: _password.text);
      widget.onAuthenticated(response);
    } catch (error) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Falha no login: $error')));
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 420),
          child: Card(
            margin: const EdgeInsets.all(24),
            child: Padding(
              padding: const EdgeInsets.all(24),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Text('CadastroIgreja', style: Theme.of(context).textTheme.headlineMedium),
                  const SizedBox(height: 24),
                  TextField(controller: _email, decoration: const InputDecoration(labelText: 'E-mail')),
                  const SizedBox(height: 12),
                  TextField(controller: _password, obscureText: true, decoration: const InputDecoration(labelText: 'Senha')),
                  const SizedBox(height: 24),
                  FilledButton.icon(
                    onPressed: _busy ? null : _login,
                    icon: _busy ? const SizedBox.square(dimension: 18, child: CircularProgressIndicator(strokeWidth: 2)) : const Icon(Icons.login),
                    label: const Text('Entrar'),
                  ),
                  const SizedBox(height: 8),
                  const Text('Cadastros novos ficam pendentes até aprovação pelo backend.'),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
