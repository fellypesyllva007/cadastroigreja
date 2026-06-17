import 'package:flutter/material.dart';

class AsyncListSection<T> extends StatelessWidget {
  const AsyncListSection({required this.title, required this.future, required this.itemBuilder, super.key});

  final String title;
  final Future<List<T>> future;
  final Widget Function(BuildContext context, T item) itemBuilder;

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<List<T>>(
      future: future,
      builder: (context, snapshot) {
        return ListView(
          padding: const EdgeInsets.all(24),
          children: [
            Text(title, style: Theme.of(context).textTheme.headlineMedium),
            const SizedBox(height: 16),
            if (snapshot.connectionState == ConnectionState.waiting) const Center(child: CircularProgressIndicator()),
            if (snapshot.hasError) Card(child: ListTile(leading: const Icon(Icons.error_outline), title: const Text('Erro ao carregar'), subtitle: Text('${snapshot.error}'))),
            if (snapshot.hasData && snapshot.data!.isEmpty) const Card(child: ListTile(title: Text('Nenhum registro encontrado.'))),
            if (snapshot.hasData) ...snapshot.data!.map((item) => itemBuilder(context, item)),
          ],
        );
      },
    );
  }
}
