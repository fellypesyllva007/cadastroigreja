import 'package:flutter/material.dart';

import '../models/usuario.dart';
import '../services/catalog_service.dart';
import '../widgets/async_list_section.dart';

class UsuariosScreen extends StatelessWidget {
  const UsuariosScreen({required this.catalogService, super.key});

  final CatalogService catalogService;

  @override
  Widget build(BuildContext context) {
    return AsyncListSection<Usuario>(
      title: 'Membros',
      future: catalogService.listarUsuarios(),
      itemBuilder: (context, usuario) => Card(
        child: ListTile(
          leading: const Icon(Icons.person_outline),
          title: Text(usuario.nomeCompleto.isEmpty ? usuario.email : usuario.nomeCompleto),
          subtitle: Text('${usuario.email} • igreja: ${usuario.igrejaId}'),
          trailing: Chip(label: Text(usuario.ativo ? 'Ativo' : 'Inativo')),
        ),
      ),
    );
  }
}
