import 'package:flutter/material.dart';

import '../models/igreja.dart';
import '../services/catalog_service.dart';
import '../widgets/async_list_section.dart';

class IgrejasScreen extends StatelessWidget {
  const IgrejasScreen({required this.catalogService, super.key});

  final CatalogService catalogService;

  @override
  Widget build(BuildContext context) {
    return AsyncListSection<Igreja>(
      title: 'Igrejas',
      future: catalogService.listarIgrejas(),
      itemBuilder: (context, igreja) => Card(
        child: ListTile(
          leading: const Icon(Icons.account_tree_outlined),
          title: Text(igreja.nome),
          subtitle: Text('${igreja.tipo}${igreja.parentId == null ? '' : ' • superior: ${igreja.parentId}'}'),
          trailing: Chip(label: Text(igreja.ativa ? 'Ativa' : 'Inativa')),
        ),
      ),
    );
  }
}
