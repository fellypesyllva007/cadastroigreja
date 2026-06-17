import 'package:flutter/material.dart';

class InfoCard extends StatelessWidget {
  const InfoCard({required this.icon, required this.title, required this.description, super.key});

  final IconData icon;
  final String title;
  final String description;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: 320,
      child: Card(
        child: ListTile(
          leading: Icon(icon),
          title: Text(title),
          subtitle: Text(description),
        ),
      ),
    );
  }
}
