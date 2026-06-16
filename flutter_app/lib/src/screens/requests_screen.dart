import 'package:flutter/material.dart';

import '../services/api_client.dart';

class RequestsScreen extends StatefulWidget {
  const RequestsScreen({super.key, required this.apiClient, required this.token});

  final ApiClient apiClient;
  final String token;

  @override
  State<RequestsScreen> createState() => _RequestsScreenState();
}

class _RequestsScreenState extends State<RequestsScreen> {
  late Future<List<dynamic>> _preacherRequests = widget.apiClient.listPreacherRequests(widget.token);
  late Future<List<dynamic>> _letters = widget.apiClient.listLetters(widget.token);

  void _reload() => setState(() {
        _preacherRequests = widget.apiClient.listPreacherRequests(widget.token);
        _letters = widget.apiClient.listLetters(widget.token);
      });

  Future<void> _run(Future<void> Function(String token) action, String success) async {
    try {
      await action(widget.token);
      _reload();
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(success)));
    } catch (error) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Erro: $error')));
    }
  }

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        FilledButton.icon(
          onPressed: () => _run(widget.apiClient.requestRole, 'Solicitação de cargo enviada.'),
          icon: const Icon(Icons.badge),
          label: const Text('Solicitar alteração de cargo'),
        ),
        const SizedBox(height: 12),
        FilledButton.icon(
          onPressed: () => _run(widget.apiClient.requestPreacher, 'Solicitação de pregador enviada.'),
          icon: const Icon(Icons.campaign),
          label: const Text('Solicitar autorização para pregar'),
        ),
        const Divider(height: 32),
        Text('Solicitações de pregador pendentes', style: Theme.of(context).textTheme.titleMedium),
        FutureBuilder<List<dynamic>>(
          future: _preacherRequests,
          builder: (context, snapshot) => ListTile(
            title: Text(snapshot.hasData ? '${snapshot.requireData.length} pendente(s)' : 'Carregando...'),
            subtitle: snapshot.hasError ? Text('${snapshot.error}') : null,
          ),
        ),
        Text('Cartas emitidas', style: Theme.of(context).textTheme.titleMedium),
        FutureBuilder<List<dynamic>>(
          future: _letters,
          builder: (context, snapshot) => ListTile(
            title: Text(snapshot.hasData ? '${snapshot.requireData.length} carta(s)' : 'Carregando...'),
            subtitle: snapshot.hasError ? Text('${snapshot.error}') : null,
          ),
        ),
      ],
    );
  }
}
