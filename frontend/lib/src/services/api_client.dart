import 'dart:convert';

import 'package:http/http.dart' as http;

class ApiClient {
  ApiClient({required this.baseUrl, http.Client? httpClient}) : _httpClient = httpClient ?? http.Client();

  final String baseUrl;
  final http.Client _httpClient;
  String? _token;

  void setToken(String token) {
    _token = token;
  }

  Future<Map<String, Object?>> postJson(String path, Map<String, Object?> body) async {
    final response = await _httpClient.post(_uri(path), headers: _headers(), body: jsonEncode(body));
    return _decodeObject(response);
  }

  Future<List<Map<String, Object?>>> getList(String path) async {
    final response = await _httpClient.get(_uri(path), headers: _headers());
    final decoded = _decode(response);
    if (decoded is List) {
      return decoded.whereType<Map>().map((item) => item.cast<String, Object?>()).toList();
    }
    return const [];
  }

  Uri _uri(String path) => Uri.parse('$baseUrl$path');

  Map<String, String> _headers() {
    return {
      'Content-Type': 'application/json',
      if (_token != null) 'Authorization': 'Bearer $_token',
    };
  }

  Map<String, Object?> _decodeObject(http.Response response) {
    final decoded = _decode(response);
    if (decoded is Map) return decoded.cast<String, Object?>();
    return const {};
  }

  Object? _decode(http.Response response) {
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw ApiException(response.statusCode, response.body);
    }
    if (response.body.isEmpty) return null;
    return jsonDecode(response.body);
  }
}

class ApiException implements Exception {
  const ApiException(this.statusCode, this.body);

  final int statusCode;
  final String body;

  @override
  String toString() => 'ApiException(statusCode: $statusCode, body: $body)';
}
