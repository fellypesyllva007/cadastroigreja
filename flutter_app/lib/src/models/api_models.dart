enum ChurchType { sede, regional, setorial, congregacaoLocal, casaOracao }

enum MemberRole { membro, diacono, presbitero, pastor, dirigente }

enum RequestStatus { pending, approved, rejected }

enum PreacherApprovalStep { casaOracao, congregacaoLocal, setorial, completed }

extension ChurchTypeApi on ChurchType {
  String get label => switch (this) {
        ChurchType.sede => 'Sede',
        ChurchType.regional => 'Regional',
        ChurchType.setorial => 'Setorial',
        ChurchType.congregacaoLocal => 'Congregação Local',
        ChurchType.casaOracao => 'Casa de Oração',
      };

  String get apiValue => switch (this) {
        ChurchType.sede => 'Sede',
        ChurchType.regional => 'Regional',
        ChurchType.setorial => 'Setorial',
        ChurchType.congregacaoLocal => 'CongregacaoLocal',
        ChurchType.casaOracao => 'CasaOracao',
      };

  static ChurchType fromApi(String value) => ChurchType.values.firstWhere(
        (type) => type.apiValue == value,
        orElse: () => ChurchType.sede,
      );
}

extension MemberRoleApi on MemberRole {
  String get label => switch (this) {
        MemberRole.membro => 'Membro',
        MemberRole.diacono => 'Diácono',
        MemberRole.presbitero => 'Presbítero',
        MemberRole.pastor => 'Pastor',
        MemberRole.dirigente => 'Dirigente',
      };

  String get apiValue => switch (this) {
        MemberRole.membro => 'Membro',
        MemberRole.diacono => 'Diacono',
        MemberRole.presbitero => 'Presbitero',
        MemberRole.pastor => 'Pastor',
        MemberRole.dirigente => 'Dirigente',
      };

  static MemberRole fromApi(String value) => MemberRole.values.firstWhere(
        (role) => role.apiValue == value,
        orElse: () => MemberRole.membro,
      );
}

extension RequestStatusApi on RequestStatus {
  String get label => switch (this) {
        RequestStatus.pending => 'Pendente',
        RequestStatus.approved => 'Aprovada',
        RequestStatus.rejected => 'Rejeitada',
      };

  String get apiValue => switch (this) {
        RequestStatus.pending => 'Pending',
        RequestStatus.approved => 'Approved',
        RequestStatus.rejected => 'Rejected',
      };

  static RequestStatus fromApi(String value) => RequestStatus.values.firstWhere(
        (status) => status.apiValue == value,
        orElse: () => RequestStatus.pending,
      );
}

extension PreacherApprovalStepApi on PreacherApprovalStep {
  String get label => switch (this) {
        PreacherApprovalStep.casaOracao => 'Casa de Oração',
        PreacherApprovalStep.congregacaoLocal => 'Congregação Local',
        PreacherApprovalStep.setorial => 'Setorial',
        PreacherApprovalStep.completed => 'Concluído',
      };

  String get apiValue => switch (this) {
        PreacherApprovalStep.casaOracao => 'CasaOracao',
        PreacherApprovalStep.congregacaoLocal => 'CongregacaoLocal',
        PreacherApprovalStep.setorial => 'Setorial',
        PreacherApprovalStep.completed => 'Completed',
      };

  static PreacherApprovalStep fromApi(String value) => PreacherApprovalStep.values.firstWhere(
        (step) => step.apiValue == value,
        orElse: () => PreacherApprovalStep.setorial,
      );
}

class Church {
  Church({required this.id, required this.name, required this.type, this.parentId});

  final String id;
  final String name;
  final ChurchType type;
  final String? parentId;

  factory Church.fromJson(Map<String, dynamic> json) => Church(
        id: json['id'] as String,
        name: json['name'] as String,
        type: ChurchTypeApi.fromApi(json['type'] as String),
        parentId: json['parentId'] as String?,
      );
}

class UserProfile {
  UserProfile({
    required this.id,
    required this.fullName,
    required this.email,
    required this.churchId,
    required this.role,
    required this.status,
    this.phone,
  });

  final String id;
  final String fullName;
  final String email;
  final String? phone;
  final String churchId;
  final String preacherRequestId;
  final MemberRole role;
  final String status;

  factory UserProfile.fromJson(Map<String, dynamic> json) => UserProfile(
        id: json['id'] as String,
        fullName: json['fullName'] as String,
        email: json['email'] as String,
        phone: json['phone'] as String?,
        churchId: json['churchId'] as String,
        role: MemberRoleApi.fromApi(json['role'] as String),
        status: json['status'] as String,
      );
}

class RoleChangeRequest {
  RoleChangeRequest({required this.id, required this.userId, required this.requestedRole, required this.status, required this.createdAt, this.decidedAt, this.justification});

  final String id;
  final String userId;
  final MemberRole requestedRole;
  final RequestStatus status;
  final DateTime createdAt;
  final DateTime? decidedAt;
  final String? justification;

  factory RoleChangeRequest.fromJson(Map<String, dynamic> json) => RoleChangeRequest(
        id: json['id'] as String,
        userId: json['userId'] as String,
        requestedRole: MemberRoleApi.fromApi(json['requestedRole'] as String),
        status: RequestStatusApi.fromApi(json['status'] as String),
        createdAt: DateTime.parse(json['createdAt'] as String),
        decidedAt: json['decidedAt'] == null ? null : DateTime.parse(json['decidedAt'] as String),
        justification: json['justification'] as String?,
      );
}

class PreacherRequest {
  PreacherRequest({required this.id, required this.userId, required this.churchId, required this.status, required this.currentStep, required this.createdAt, this.decidedAt, this.letterId, this.notes});

  final String id;
  final String userId;
  final String churchId;
  final RequestStatus status;
  final PreacherApprovalStep currentStep;
  final DateTime createdAt;
  final DateTime? decidedAt;
  final String? letterId;
  final String? notes;

  factory PreacherRequest.fromJson(Map<String, dynamic> json) => PreacherRequest(
        id: json['id'] as String,
        userId: json['userId'] as String,
        churchId: json['churchId'] as String,
        status: RequestStatusApi.fromApi(json['status'] as String),
        currentStep: PreacherApprovalStepApi.fromApi(json['currentStep'] as String),
        createdAt: DateTime.parse(json['createdAt'] as String),
        decidedAt: json['decidedAt'] == null ? null : DateTime.parse(json['decidedAt'] as String),
        letterId: json['letterId'] as String?,
        notes: json['notes'] as String?,
      );
}

class PreachingLetter {
  PreachingLetter({required this.id, required this.userId, required this.churchId, required this.preacherRequestId, required this.number, required this.issuedAt, required this.validUntil, required this.suspended, required this.validationUrl});

  final String id;
  final String userId;
  final String churchId;
  final String preacherRequestId;
  final String number;
  final DateTime issuedAt;
  final DateTime validUntil;
  final bool suspended;
  final String validationUrl;

  factory PreachingLetter.fromJson(Map<String, dynamic> json) => PreachingLetter(
        id: json['id'] as String,
        userId: json['userId'] as String,
        churchId: json['churchId'] as String,
        preacherRequestId: json['preacherRequestId'] as String,
        number: json['number'] as String,
        issuedAt: DateTime.parse(json['issuedAt'] as String),
        validUntil: DateTime.parse(json['validUntil'] as String),
        suspended: json['suspended'] as bool,
        validationUrl: json['validationUrl'] as String,
      );
}


class AuditLogEntry {
  AuditLogEntry({required this.id, required this.action, required this.entityName, required this.entityId, required this.createdAt, this.userId, this.metadata});

  final int id;
  final String? userId;
  final String action;
  final String entityName;
  final String entityId;
  final String? metadata;
  final DateTime createdAt;

  factory AuditLogEntry.fromJson(Map<String, dynamic> json) => AuditLogEntry(
        id: json['id'] as int,
        userId: json['userId'] as String?,
        action: json['action'] as String,
        entityName: json['entityName'] as String,
        entityId: json['entityId'] as String,
        metadata: json['metadata'] as String?,
        createdAt: DateTime.parse(json['createdAt'] as String),
      );
}
