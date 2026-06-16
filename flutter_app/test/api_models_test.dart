import 'package:flutter_test/flutter_test.dart';
import 'package:cadastroigreja/src/models/api_models.dart';

void main() {
  test('ChurchType maps backend enum values', () {
    expect(ChurchTypeApi.fromApi('CongregacaoLocal'), ChurchType.congregacaoLocal);
    expect(ChurchType.casaOracao.apiValue, 'CasaOracao');
  });
}
