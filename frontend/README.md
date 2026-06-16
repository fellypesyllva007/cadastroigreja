# CadastroIgreja Frontend

Aplicação Flutter inicial configurada para evoluir como cliente multiplataforma do backend ASP.NET Core. O web bootstrap foi incluído e os diretórios Android, iOS, Linux, macOS e Windows trazem instruções para materializar os projetos nativos quando o Flutter SDK estiver disponível.

## Plataformas alvo

- Web
- Android
- iOS
- Windows
- macOS
- Linux

## Como executar

```bash
flutter pub get
flutter run -d chrome
```

Quando o Flutter SDK estiver instalado, os diretórios nativos podem ser materializados/atualizados com:

```bash
flutter create --platforms=web,android,ios,windows,macos,linux .
```

## Configuração da API

O app usa `http://localhost:5000` por padrão. Para apontar para outro backend:

```bash
flutter run -d chrome --dart-define=API_BASE_URL=http://localhost:5000
```

## Estrutura

- `lib/src/config`: configuração por ambiente.
- `lib/src/models`: modelos de resposta da API.
- `lib/src/services`: cliente HTTP e serviços de autenticação/catálogo.
- `lib/src/screens`: login, shell administrativo e telas de listagem.
- `lib/src/widgets`: componentes reutilizáveis.
