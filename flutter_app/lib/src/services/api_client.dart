import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/api_models.dart';

class ApiException implements Exception {
  ApiException(this.message, this.statusCode);

  final String message;
  final int statusCode;

  @override
  String toString() => message;
}

class ApiClient {
  ApiClient({required this.baseUri, http.Client? httpClient}) : _httpClient = httpClient ?? http.Client();

  factory ApiClient.fromEnvironment() {
    const apiBaseUrl = String.fromEnvironment('API_BASE_URL', defaultValue: 'http://localhost:5000');
    return ApiClient(baseUri: Uri.parse(apiBaseUrl));
  }

  final Uri baseUri;
  final http.Client _httpClient;

  Future<Map<String, dynamic>> login({required String email, required String password}) async {
    final response = await _post('/api/auth/login', body: {'email': email, 'password': password});
    return jsonDecode(response.body) as Map<String, dynamic>;
  }

  Future<void> register({
    required String fullName,
    required String email,
    required String password,
    required String churchId,
    String? phone,
  }) async {
    await _post('/api/auth/register', body: {
      'fullName': fullName,
      'email': email,
      'password': password,
      'churchId': churchId,
      if (phone != null && phone.isNotEmpty) 'phone': phone,
    });
  }

  Future<List<Church>> listChurches({String? token, String? parentId, ChurchType? type}) async {
    final query = <String, String>{
      if (parentId != null && parentId.isNotEmpty) 'parentId': parentId,
      if (type != null) 'type': type.apiValue,
    };
    final response = await _get('/api/churches', token: token, query: query);
    final items = jsonDecode(response.body) as List<dynamic>;
    return items.cast<Map<String, dynamic>>().map(Church.fromJson).toList();
  }

  Future<void> createChurch({required String token, required String name, required ChurchType type, String? parentId}) async {
    await _post('/api/churches', token: token, body: {
      'name': name,
      'type': type.apiValue,
      if (parentId != null && parentId.isNotEmpty) 'parentId': parentId,
    });
  }

  Future<UserProfile> me(String token) async {
    final response = await _get('/api/users/me', token: token);
    return UserProfile.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<void> requestRole(String token) => _post('/api/role-requests', token: token, body: {}).then((_) {});
  Future<void> requestPreacher(String token) => _post('/api/preacher-requests', token: token, body: {}).then((_) {});

  Future<List<dynamic>> listPreacherRequests(String token) async {
    final response = await _get('/api/preacher-requests', token: token);
    return jsonDecode(response.body) as List<dynamic>;
  }

  Future<List<dynamic>> listLetters(String token) async {
    final response = await _get('/api/letters', token: token);
    return jsonDecode(response.body) as List<dynamic>;
  }

  Future<http.Response> _get(String path, {String? token, Map<String, String>? query}) async {
    final uri = _uri(path, query);
    final response = await _httpClient.get(uri, headers: _headers(token));
    return _ensureSuccess(response);
  }

  Future<http.Response> _post(String path, {String? token, required Map<String, dynamic> body}) async {
    final response = await _httpClient.post(_uri(path), headers: _headers(token), body: jsonEncode(body));
    return _ensureSuccess(response);
  }

  Uri _uri(String path, [Map<String, String>? query]) => baseUri.replace(
        path: path,
        queryParameters: query == null || query.isEmpty ? null : query,
      );

  Map<String, String> _headers(String? token) => {
        'content-type': 'application/json',
        if (token != null) 'authorization': 'Bearer $token',
      };

  http.Response _ensureSuccess(http.Response response) {
    if (response.statusCode >= 200 && response.statusCode < 300) return response;
    throw ApiException(response.body.isEmpty ? 'Erro HTTP ${response.statusCode}' : response.body, response.statusCode);
  }
}
