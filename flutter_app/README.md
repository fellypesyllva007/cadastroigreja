# CadastroIgreja Flutter

MVP Flutter do CadastroIgreja com login, perfil, listagem/criação de igrejas e telas de solicitações/cartas.

## Executar

```bash
flutter pub get
flutter run -d chrome --dart-define=API_BASE_URL=http://localhost:5000
```

Para Android Emulator, use o host do emulador:

```bash
flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5000
```

## Testar

```bash
flutter analyze
flutter test
```
