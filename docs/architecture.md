# Arquitetura do Sistema

## Visao Geral

O sistema sera composto por Flutter, ASP.NET Core 9 e PostgreSQL.

## Frontend
- Flutter Web
- Flutter Android
- Flutter iOS
- Flutter Windows
- Flutter macOS

## Backend
- ASP.NET Core 9
- REST API
- JWT Authentication
- Swagger

## Arquitetura

Clean Architecture:
- Presentation
- API
- Application
- Domain
- Infrastructure

## CQRS

Commands:
- RegisterUser
- ApproveMember
- ApprovePreacher
- IssueLetter

Queries:
- GetUser
- GetChurch
- GetLetter

## Estrutura de Projetos

src/
  CadastroIgreja.Api
  CadastroIgreja.Application
  CadastroIgreja.Domain
  CadastroIgreja.Infrastructure

## Seguranca
- JWT
- Refresh Token
- Auditoria
- Controle Hierarquico

## Deploy
- Docker
- PostgreSQL
- Nginx
- HTTPS