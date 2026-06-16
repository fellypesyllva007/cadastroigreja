import 'package:flutter/material.dart';

import '../models/carta_pregacao.dart';
import '../services/catalog_service.dart';
import '../widgets/async_list_section.dart';

class CartasScreen extends StatelessWidget {
  const CartasScreen({required this.catalogService, super.key});

  final CatalogService catalogService;

  @override
  Widget build(BuildContext context) {
    return AsyncListSection<CartaPregacao>(
      title: 'Cartas de Pregação',
      future: catalogService.listarCartas(),
      itemBuilder: (context, carta) => Card(
        child: ListTile(
          leading: const Icon(Icons.assignment_turned_in_outlined),
          title: Text(carta.numero),
          subtitle: Text('Usuário: ${carta.usuarioId}'),
          trailing: Chip(label: Text(carta.ativa ? 'Válida' : 'Suspensa')),
        ),
      ),
    );
  }
}
