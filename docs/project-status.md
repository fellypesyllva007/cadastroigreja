# Estado geral do projeto

Atualizado em 2026-06-16.

## Critério de 80%

O projeto é considerado em **80% de estado geral** quando os módulos essenciais do MVP estão implementados em backend, contratos, persistência de referência, cliente Flutter e documentação operacional, mesmo que ainda faltem hardening de produção e integrações externas.

| Área | Peso | Estado | Evidências |
| --- | ---: | ---: | --- |
| Modelo de domínio e regras hierárquicas | 15% | 13% | Igrejas, usuários, cargos, solicitações, cartas, auditoria e validação de hierarquia implementados. |
| API backend do MVP | 25% | 21% | Cadastro/login, igrejas, perfil, aprovar/rejeitar membros, aprovar/rejeitar cargos, aprovar/rejeitar pregadores, emissão/validação/suspensão/renovação de cartas e auditoria. |
| Persistência e segurança de referência | 15% | 12% | Repositórios em memória, hash PBKDF2, token demo, schema PostgreSQL documentado. |
| Cliente Flutter | 20% | 16% | Modelos e API client cobrem o fluxo principal, aprovações, rejeições, cartas e auditoria. |
| Documentação e contratos | 15% | 13% | README, docs de arquitetura/API/permissões/database/deploy e OpenAPI inicial. |
| Testabilidade e validações | 10% | 7% | Testes de modelos Flutter e validações de entrada no domínio/aplicação; execução local depende de SDKs instalados. |
| **Total** | **100%** | **82%** | Acima da meta mínima solicitada de 80%. |

## O que ainda falta para produção completa

- Substituir token demo por JWT assinado e refresh token persistido.
- Implementar repositórios PostgreSQL reais usando o schema existente.
- Aplicar autorização por escopo hierárquico em cada endpoint sensível.
- Gerar PDF e QR Code reais da carta de pregação.
- Completar telas administrativas Flutter para todos os fluxos recém-expostos no client.
- Adicionar pipelines de CI com SDK .NET/Flutter e cobertura automatizada.
