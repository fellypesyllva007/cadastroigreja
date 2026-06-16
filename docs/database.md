# Modelo de Banco de Dados

## Church
Id
Name
Type
ParentId
Active

Tipos:
- Sede
- Regional
- Setorial
- CongregacaoLocal
- CasaOracao

## User
Id
Name
Email
Phone
PasswordHash
ChurchId
RoleId
Status

## Role
- Membro
- Diacono
- Presbitero
- Pastor
- Dirigente

## RoleChangeRequest
Solicitacoes de alteracao de cargo.

## PreacherRequest
Solicitacoes de pregacao.
Estados:
- Pending
- HouseApproved
- LocalApproved
- SetorialApproved
- Rejected

## Approval
Historico de aprovacoes.

## PreachingLetter
Numero
IssueDate
ExpirationDate
PdfPath
QrCode

## AuditLog
Usuario
Acao
Entidade
Data
IP

## Indices
- Email unico
- Numero da carta unico
- ChurchId indexado
- UserId indexado