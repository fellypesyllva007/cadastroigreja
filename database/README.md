# Banco de Dados

Este diretório contém os artefatos versionados do PostgreSQL.

## Executar localmente

```bash
docker compose up -d db
psql "$DATABASE_URL" -f database/migrations/001_initial_schema.sql
```

## Convenções

- Migrações devem ser numeradas sequencialmente (`NNN_descricao.sql`).
- Campos de auditoria usam `timestamptz` em UTC.
- Identificadores públicos usam `uuid` gerado por `gen_random_uuid()`.
- Estados são restringidos por `CHECK` até que haja necessidade de tabelas de domínio administráveis.
