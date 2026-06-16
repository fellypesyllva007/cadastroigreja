enum ChurchType { sede, regional, setorial, congregacaoLocal, casaOracao }

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
  final String role;
  final String status;

  factory UserProfile.fromJson(Map<String, dynamic> json) => UserProfile(
        id: json['id'] as String,
        fullName: json['fullName'] as String,
        email: json['email'] as String,
        phone: json['phone'] as String?,
        churchId: json['churchId'] as String,
        role: json['role'] as String,
        status: json['status'] as String,
      );
}
