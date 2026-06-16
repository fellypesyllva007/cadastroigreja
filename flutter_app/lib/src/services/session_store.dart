import 'package:shared_preferences/shared_preferences.dart';

class SessionStore {
  static const _tokenKey = 'access_token';
  static const _refreshTokenKey = 'refresh_token';

  Future<String?> readAccessToken() async {
    final preferences = await SharedPreferences.getInstance();
    return preferences.getString(_tokenKey);
  }

  Future<void> save({required String accessToken, required String refreshToken}) async {
    final preferences = await SharedPreferences.getInstance();
    await preferences.setString(_tokenKey, accessToken);
    await preferences.setString(_refreshTokenKey, refreshToken);
  }

  Future<void> clear() async {
    final preferences = await SharedPreferences.getInstance();
    await preferences.remove(_tokenKey);
    await preferences.remove(_refreshTokenKey);
  }
}
