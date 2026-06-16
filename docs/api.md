# API REST

A especificação OpenAPI versionada fica em `openapi/cadastroigreja.v1.yaml`.

## Grupos de endpoints

- `/api/auth`: cadastro, login e renovação de sessão.
- `/api/churches`: consulta e manutenção da estrutura hierárquica.
- `/api/users`: perfil, aprovação de membros e administração de usuários.
- `/api/role-requests`: solicitações e aprovações de alteração de cargo.
- `/api/preacher-requests`: fluxo de autorização para pregação.
- `/api/letters`: cartas emitidas, suspensão, renovação e validação pública por QR Code.

## Autenticação

A API usa JWT Bearer Token em todos os endpoints privados. Endpoints de cadastro, login e validação pública de carta não exigem autenticação.

## Regras transversais

- Toda ação de escrita deve gerar registro em `audit_logs`.
- Toda consulta deve respeitar o escopo hierárquico do usuário autenticado.
- Aprovações finais de pregador e emissão de carta são exclusivas do nível Setorial ou superior.
