import 'package:flutter/material.dart';

import '../models/api_models.dart';
import '../services/api_client.dart';

class RequestsScreen extends StatefulWidget {
  const RequestsScreen({super.key, required this.apiClient, required this.token});

  final ApiClient apiClient;
  final String token;

  @override
  State<RequestsScreen> createState() => _RequestsScreenState();
}

class _RequestsScreenState extends State<RequestsScreen> {
  late Future<UserProfile> _profile = widget.apiClient.me(widget.token);
  late Future<List<RoleChangeRequest>> _roleRequests = widget.apiClient.listRoleRequests(widget.token);
  late Future<List<PreacherRequest>> _preacherRequests = widget.apiClient.listPreacherRequests(widget.token);
  late Future<List<PreachingLetter>> _letters = widget.apiClient.listLetters(widget.token);
  MemberRole _requestedRole = MemberRole.diacono;

  void _reload() => setState(() {
        _profile = widget.apiClient.me(widget.token);
        _roleRequests = widget.apiClient.listRoleRequests(widget.token);
        _preacherRequests = widget.apiClient.listPreacherRequests(widget.token);
        _letters = widget.apiClient.listLetters(widget.token);
      });

  Future<void> _run(Future<void> Function(UserProfile profile) action, String success) async {
    try {
      final profile = await _profile;
      await action(profile);
      _reload();
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(success)));
    } catch (error) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Erro: $error')));
    }
  }

  Future<void> _approvePreacher(String id) async {
    try {
      await widget.apiClient.approvePreacherRequest(widget.token, id);
      _reload();
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Solicitação avançada/aprovada.')));
    } catch (error) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Erro: $error')));
    }
  }

  @override
  Widget build(BuildContext context) {
    return RefreshIndicator(
      onRefresh: () async => _reload(),
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Text('Solicitações', style: Theme.of(context).textTheme.titleLarge),
          const SizedBox(height: 12),
          DropdownButtonFormField<MemberRole>(
            value: _requestedRole,
            decoration: const InputDecoration(labelText: 'Cargo desejado', border: OutlineInputBorder()),
            items: MemberRole.values.where((role) => role != MemberRole.membro).map((role) {
              return DropdownMenuItem(value: role, child: Text(role.label));
            }).toList(),
            onChanged: (value) => setState(() => _requestedRole = value ?? _requestedRole),
          ),
          const SizedBox(height: 12),
          FilledButton.icon(
            onPressed: () => _run(
              (profile) => widget.apiClient.requestRole(widget.token, userId: profile.id, requestedRole: _requestedRole),
              'Solicitação de cargo enviada.',
            ),
            icon: const Icon(Icons.badge),
            label: const Text('Solicitar alteração de cargo'),
          ),
          const SizedBox(height: 12),
          FilledButton.icon(
            onPressed: () => _run(
              (profile) => widget.apiClient.requestPreacher(widget.token, userId: profile.id),
              'Solicitação de pregador enviada.',
            ),
            icon: const Icon(Icons.campaign),
            label: const Text('Solicitar autorização para pregar'),
          ),
          const Divider(height: 32),
          Text('Alterações de cargo', style: Theme.of(context).textTheme.titleMedium),
          FutureBuilder<List<RoleChangeRequest>>(
            future: _roleRequests,
            builder: (context, snapshot) => _RequestList(
              loadingText: 'Carregando cargos...',
              emptyText: 'Nenhuma alteração de cargo encontrada.',
              loading: snapshot.connectionState != ConnectionState.done,
              error: snapshot.error,
              children: snapshot.data
                      ?.map((request) => ListTile(
                            leading: const Icon(Icons.badge_outlined),
                            title: Text(request.requestedRole.label),
                            subtitle: Text('${request.status.label} • ${_formatDate(request.createdAt)}'),
                          ))
                      .toList() ??
                  const [],
            ),
          ),
          const SizedBox(height: 16),
          Text('Autorizações de pregador', style: Theme.of(context).textTheme.titleMedium),
          FutureBuilder<List<PreacherRequest>>(
            future: _preacherRequests,
            builder: (context, snapshot) => _RequestList(
              loadingText: 'Carregando pregadores...',
              emptyText: 'Nenhuma autorização encontrada.',
              loading: snapshot.connectionState != ConnectionState.done,
              error: snapshot.error,
              children: snapshot.data
                      ?.map((request) => ListTile(
                            leading: const Icon(Icons.campaign_outlined),
                            title: Text(request.currentStep.label),
                            subtitle: Text('${request.status.label} • ${_formatDate(request.createdAt)}'),
                            trailing: request.status == RequestStatus.pending
                                ? TextButton(onPressed: () => _approvePreacher(request.id), child: const Text('Aprovar'))
                                : null,
                          ))
                      .toList() ??
                  const [],
            ),
          ),
          const SizedBox(height: 16),
          Text('Cartas emitidas', style: Theme.of(context).textTheme.titleMedium),
          FutureBuilder<List<PreachingLetter>>(
            future: _letters,
            builder: (context, snapshot) => _RequestList(
              loadingText: 'Carregando cartas...',
              emptyText: 'Nenhuma carta emitida.',
              loading: snapshot.connectionState != ConnectionState.done,
              error: snapshot.error,
              children: snapshot.data
                      ?.map((letter) => ListTile(
                            leading: const Icon(Icons.description_outlined),
                            title: Text(letter.number),
                            subtitle: Text('Emitida em ${_formatDate(letter.issuedAt)} • válida até ${_formatDate(letter.validUntil)}'),
                          ))
                      .toList() ??
                  const [],
            ),
          ),
        ],
      ),
    );
  }

  String _formatDate(DateTime date) => '${date.day.toString().padLeft(2, '0')}/${date.month.toString().padLeft(2, '0')}/${date.year}';
}

class _RequestList extends StatelessWidget {
  const _RequestList({required this.loadingText, required this.emptyText, required this.loading, required this.children, this.error});

  final String loadingText;
  final String emptyText;
  final bool loading;
  final Object? error;
  final List<Widget> children;

  @override
  Widget build(BuildContext context) {
    if (error != null) return ListTile(title: const Text('Não foi possível carregar.'), subtitle: Text('$error'));
    if (loading) return ListTile(title: Text(loadingText));
    if (children.isEmpty) return ListTile(title: Text(emptyText));
    return Column(children: children);
  }
}
