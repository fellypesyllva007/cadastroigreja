import 'package:flutter/material.dart';

import '../services/api_client.dart';
import '../services/session_store.dart';
import 'dashboard_screen.dart';
import 'login_screen.dart';

class AppShell extends StatefulWidget {
  const AppShell({super.key, required this.apiClient, required this.sessionStore});

  final ApiClient apiClient;
  final SessionStore sessionStore;

  @override
  State<AppShell> createState() => _AppShellState();
}

class _AppShellState extends State<AppShell> {
  String? _token;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _restore();
  }

  Future<void> _restore() async {
    final token = await widget.sessionStore.readAccessToken();
    if (mounted) setState(() { _token = token; _loading = false; });
  }

  Future<void> _onAuthenticated(Map<String, dynamic> response) async {
    final accessToken = response['accessToken'] as String;
    await widget.sessionStore.save(
      accessToken: accessToken,
      refreshToken: response['refreshToken'] as String,
    );
    if (mounted) setState(() => _token = accessToken);
  }

  Future<void> _logout() async {
    await widget.sessionStore.clear();
    if (mounted) setState(() => _token = null);
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) return const Scaffold(body: Center(child: CircularProgressIndicator()));
    final token = _token;
    if (token == null) {
      return LoginScreen(apiClient: widget.apiClient, onAuthenticated: _onAuthenticated);
    }
    return DashboardScreen(apiClient: widget.apiClient, token: token, onLogout: _logout);
  }
}
