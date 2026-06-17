import '../models/auth_models.dart';
import 'api_client.dart';

class AuthService {
  const AuthService(this._apiClient);

  final ApiClient _apiClient;

  Future<LoginResponse> login(LoginRequest request) async {
    final json = await _apiClient.postJson('/api/auth/login', request.toJson());
    final response = LoginResponse.fromJson(json);
    _apiClient.setToken(response.token);
    return response;
  }
}
