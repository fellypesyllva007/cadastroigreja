import 'package:flutter/material.dart';

import '../models/usuario.dart';
import '../widgets/info_card.dart';

class DashboardScreen extends StatelessWidget {
  const DashboardScreen({required this.usuario, super.key});

  final Usuario usuario;

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(24),
      children: [
        Text('Painel administrativo', style: Theme.of(context).textTheme.headlineMedium),
        const SizedBox(height: 8),
        Text('Bem-vindo, ${usuario.nomeCompleto.isEmpty ? usuario.email : usuario.nomeCompleto}.'),
        const SizedBox(height: 16),
        const Wrap(
          spacing: 12,
          runSpacing: 12,
          children: [
            InfoCard(icon: Icons.login, title: 'Autenticação', description: 'Login JWT integrado ao backend.'),
            InfoCard(icon: Icons.account_tree, title: 'Hierarquia', description: 'Consulta de Sede, Regionais, Setoriais e Congregações.'),
            InfoCard(icon: Icons.verified, title: 'Cartas', description: 'Listagem e validação de cartas de pregação.'),
          ],
        ),
      ],
    );
  }
}
