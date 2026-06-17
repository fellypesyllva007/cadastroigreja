import 'package:flutter/material.dart';

import '../models/api_models.dart';
import '../services/api_client.dart';

class ChurchesScreen extends StatefulWidget {
  const ChurchesScreen({super.key, required this.apiClient, required this.token});

  final ApiClient apiClient;
  final String token;

  @override
  State<ChurchesScreen> createState() => _ChurchesScreenState();
}

class _ChurchesScreenState extends State<ChurchesScreen> {
  late Future<List<Church>> _future = widget.apiClient.listChurches(token: widget.token);

  void _reload() => setState(() => _future = widget.apiClient.listChurches(token: widget.token));

  Future<void> _create() async {
    final name = TextEditingController();
    ChurchType type = ChurchType.sede;
    final parentId = TextEditingController();
    final created = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Nova igreja'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(controller: name, decoration: const InputDecoration(labelText: 'Nome')),
            DropdownButtonFormField(
              value: type,
              items: ChurchType.values.map((item) => DropdownMenuItem(value: item, child: Text(item.label))).toList(),
              onChanged: (value) => type = value ?? ChurchType.sede,
              decoration: const InputDecoration(labelText: 'Tipo'),
            ),
            TextField(controller: parentId, decoration: const InputDecoration(labelText: 'Parent ID (opcional para Sede)')),
          ],
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Cancelar')),
          FilledButton(onPressed: () => Navigator.pop(context, true), child: const Text('Salvar')),
        ],
      ),
    );
    if (created != true) return;
    try {
      await widget.apiClient.createChurch(token: widget.token, name: name.text, type: type, parentId: parentId.text);
      _reload();
    } catch (error) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Erro ao criar igreja: $error')));
    }
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<List<Church>>(
      future: _future,
      builder: (context, snapshot) {
        final body = switch (snapshot.connectionState) {
          ConnectionState.done when snapshot.hasData => ListView(
              padding: const EdgeInsets.all(16),
              children: snapshot.requireData
                  .map((church) => Card(
                        child: ListTile(
                          leading: const Icon(Icons.church),
                          title: Text(church.name),
                          subtitle: Text('${church.type.label}${church.parentId == null ? '' : ' • Pai: ${church.parentId}'}'),
                        ),
                      ))
                  .toList(),
            ),
          ConnectionState.done => Center(child: Text('Erro ao listar igrejas: ${snapshot.error}')),
          _ => const Center(child: CircularProgressIndicator()),
        };
        return Scaffold(body: body, floatingActionButton: FloatingActionButton(onPressed: _create, child: const Icon(Icons.add)));
      },
    );
  }
}
