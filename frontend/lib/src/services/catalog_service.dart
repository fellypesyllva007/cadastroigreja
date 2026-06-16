import '../models/carta_pregacao.dart';
import '../models/igreja.dart';
import '../models/usuario.dart';
import 'api_client.dart';

class CatalogService {
  const CatalogService(this._apiClient);

  final ApiClient _apiClient;

  Future<List<Igreja>> listarIgrejas() async {
    final items = await _apiClient.getList('/api/churches');
    return items.map(Igreja.fromJson).toList();
  }

  Future<List<Usuario>> listarUsuarios() async {
    final items = await _apiClient.getList('/api/users');
    return items.map(Usuario.fromJson).toList();
  }

  Future<List<CartaPregacao>> listarCartas() async {
    final items = await _apiClient.getList('/api/letters');
    return items.map(CartaPregacao.fromJson).toList();
  }
}
