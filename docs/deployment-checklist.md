# Checklist de publicação e teste

Este projeto ainda está na fase de fundação técnica. Use este checklist para validar o que já pode ser levado ao servidor e o que ainda depende de implementação.

## Pronto para subir em ambiente de teste

- Provisionar PostgreSQL 16.
- Executar `database/migrations/001_initial_schema.sql` em uma base vazia.
- Conferir se as tabelas, índices e triggers foram criados sem erro.
- Configurar backup e retenção antes de cadastrar dados reais.
- Usar credenciais diferentes das credenciais de desenvolvimento do `docker-compose.yml`.

## Ainda não pronto para uso por usuários finais

- O backend ASP.NET Core ainda não foi implementado.
- O frontend Flutter ainda não foi implementado.
- Autenticação JWT, upload de arquivos, geração de PDF e QR Code ainda são contratos/documentação, não funcionalidades executáveis.
- O arquivo OpenAPI é uma especificação inicial e ainda precisa ser conectado a uma API real.

## Smoke test sugerido no servidor

Após aplicar a migration, valide pelo `psql`:

```sql
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY table_name;
```

Tabelas esperadas:

- `approvals`
- `audit_logs`
- `churches`
- `password_reset_tokens`
- `preacher_requests`
- `preaching_letters`
- `refresh_tokens`
- `role_change_requests`
- `roles`
- `storage_files`
- `users`

Valide também se os cargos padrão foram inseridos:

```sql
SELECT name FROM roles ORDER BY id;
```

Resultado esperado: `Membro`, `Diacono`, `Presbitero`, `Pastor`, `Dirigente`.

## Próximo marco de desenvolvimento

1. Criar a solution ASP.NET Core 9 com camadas `Api`, `Application`, `Domain` e `Infrastructure`.
2. Implementar autenticação, registro e login usando as tabelas de usuários e refresh tokens.
3. Implementar endpoints de igrejas e usuários conforme `openapi/cadastroigreja.v1.yaml`.
4. Adicionar testes automatizados de regras de hierarquia e permissões.
