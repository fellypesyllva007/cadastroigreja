import 'package:flutter/material.dart';

import '../models/usuario.dart';
import '../services/api_client.dart';
import '../services/catalog_service.dart';
import 'cartas_screen.dart';
import 'dashboard_screen.dart';
import 'igrejas_screen.dart';
import 'usuarios_screen.dart';

class ShellScreen extends StatefulWidget {
  const ShellScreen({required this.apiClient, required this.usuario, super.key});

  final ApiClient apiClient;
  final Usuario usuario;

  @override
  State<ShellScreen> createState() => _ShellScreenState();
}

class _ShellScreenState extends State<ShellScreen> {
  int selectedIndex = 0;

  @override
  Widget build(BuildContext context) {
    final catalogService = CatalogService(widget.apiClient);
    final sections = [
      DashboardScreen(usuario: widget.usuario),
      IgrejasScreen(catalogService: catalogService),
      UsuariosScreen(catalogService: catalogService),
      CartasScreen(catalogService: catalogService),
    ];

    return Scaffold(
      appBar: AppBar(
        title: const Text('CadastroIgreja'),
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16),
            child: Center(child: Text(widget.usuario.nomeCompleto.isEmpty ? widget.usuario.email : widget.usuario.nomeCompleto)),
          ),
        ],
      ),
      body: Row(
        children: [
          NavigationRail(
            selectedIndex: selectedIndex,
            onDestinationSelected: (index) => setState(() => selectedIndex = index),
            labelType: NavigationRailLabelType.all,
            destinations: const [
              NavigationRailDestination(icon: Icon(Icons.dashboard_outlined), selectedIcon: Icon(Icons.dashboard), label: Text('Início')),
              NavigationRailDestination(icon: Icon(Icons.account_tree_outlined), selectedIcon: Icon(Icons.account_tree), label: Text('Igrejas')),
              NavigationRailDestination(icon: Icon(Icons.people_outline), selectedIcon: Icon(Icons.people), label: Text('Membros')),
              NavigationRailDestination(icon: Icon(Icons.assignment_outlined), selectedIcon: Icon(Icons.assignment), label: Text('Cartas')),
            ],
          ),
          const VerticalDivider(width: 1),
          Expanded(child: sections[selectedIndex]),
        ],
      ),
    );
  }
}
