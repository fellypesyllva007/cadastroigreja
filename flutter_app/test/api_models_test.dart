import 'package:flutter_test/flutter_test.dart';
import 'package:cadastroigreja/src/models/api_models.dart';

void main() {
  test('ChurchType maps backend enum values', () {
    expect(ChurchTypeApi.fromApi('CongregacaoLocal'), ChurchType.congregacaoLocal);
    expect(ChurchType.casaOracao.apiValue, 'CasaOracao');
  });

  test('request and letter models parse backend payloads', () {
    final roleRequest = RoleChangeRequest.fromJson({
      'id': 'role-id',
      'userId': 'user-id',
      'requestedRole': 'Presbitero',
      'status': 'Pending',
      'createdAt': '2026-06-16T10:00:00Z',
      'decidedAt': null,
    });
    expect(roleRequest.requestedRole, MemberRole.presbitero);
    expect(roleRequest.status.label, 'Pendente');

    final preacherRequest = PreacherRequest.fromJson({
      'id': 'preacher-id',
      'userId': 'user-id',
      'churchId': 'church-id',
      'status': 'Approved',
      'currentStep': 'Completed',
      'createdAt': '2026-06-16T10:00:00Z',
      'decidedAt': '2026-06-16T11:00:00Z',
      'letterId': 'letter-id',
    });
    expect(preacherRequest.currentStep, PreacherApprovalStep.completed);
    expect(preacherRequest.status, RequestStatus.approved);

    final letter = PreachingLetter.fromJson({
      'id': 'letter-id',
      'userId': 'user-id',
      'churchId': 'church-id',
      'number': 'CP-20260616-1234',
      'issuedAt': '2026-06-16',
      'validUntil': '2027-06-16',
      'suspended': false,
    });
    expect(letter.number, 'CP-20260616-1234');
    expect(letter.suspended, isFalse);
  });
}
