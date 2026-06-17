Cadastro Igreja

Ambiente

Este repositório utiliza:

- .NET 9 SDK
- PostgreSQL 16
- Flutter

Backend

Restaurar dependências:

dotnet restore CadastroIgreja.sln

Compilar:

dotnet build CadastroIgreja.sln --configuration Release

Executar testes:

dotnet test tests/CadastroIgreja.Tests/CadastroIgreja.Tests.csproj --configuration Release

Flutter

Projeto Flutter principal:

flutter_app/

Instalar dependências:

cd flutter_app
flutter pub get

Analisar:

flutter analyze

Executar testes:

flutter test

Gerar build web:

flutter build web --release
