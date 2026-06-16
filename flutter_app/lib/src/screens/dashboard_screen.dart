import 'package:flutter/material.dart';

import '../models/api_models.dart';
import '../services/api_client.dart';
import 'churches_screen.dart';
import 'requests_screen.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key, required this.apiClient, required this.token, required this.onLogout});

  final ApiClient apiClient;
  final String token;
  final VoidCallback onLogout;

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  int _index = 0;

  @override
  Widget build(BuildContext context) {
    final pages = [
      _Home(apiClient: widget.apiClient, token: widget.token),
      ChurchesScreen(apiClient: widget.apiClient, token: widget.token),
      RequestsScreen(apiClient: widget.apiClient, token: widget.token),
    ];
    return Scaffold(
      appBar: AppBar(
        title: const Text('CadastroIgreja'),
        actions: [IconButton(onPressed: widget.onLogout, icon: const Icon(Icons.logout), tooltip: 'Sair')],
      ),
      body: pages[_index],
      bottomNavigationBar: NavigationBar(
        selectedIndex: _index,
        onDestinationSelected: (value) => setState(() => _index = value),
        destinations: const [
          NavigationDestination(icon: Icon(Icons.person), label: 'Perfil'),
          NavigationDestination(icon: Icon(Icons.church), label: 'Igrejas'),
          NavigationDestination(icon: Icon(Icons.assignment), label: 'Fluxos'),
        ],
      ),
    );
  }
}

class _Home extends StatelessWidget {
  const _Home({required this.apiClient, required this.token});

  final ApiClient apiClient;
  final String token;

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<UserProfile>(
      future: apiClient.me(token),
      builder: (context, snapshot) {
        if (snapshot.connectionState != ConnectionState.done) return const Center(child: CircularProgressIndicator());
        if (snapshot.hasError) return Center(child: Text('Erro ao carregar perfil: ${snapshot.error}'));
        final profile = snapshot.requireData;
        return ListView(
          padding: const EdgeInsets.all(16),
          children: [
            ListTile(title: Text(profile.fullName), subtitle: Text(profile.email), leading: const CircleAvatar(child: Icon(Icons.person))),
            ListTile(title: const Text('Cargo'), subtitle: Text(profile.role.label)),
            ListTile(title: const Text('Status'), subtitle: Text(profile.status)),
            ListTile(title: const Text('Igreja'), subtitle: Text(profile.churchId)),
          ],
        );
      },
    );
  }
}
