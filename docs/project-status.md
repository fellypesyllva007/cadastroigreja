# Estado geral do projeto

Atualizado em 2026-06-16.

## Critério de 99%

O projeto é considerado em **99% de estado geral** quando o MVP executável, os contratos, a persistência versionada, os fluxos Flutter, a documentação operacional e as validações automatizadas cobrem praticamente todo o escopo funcional planejado, restando apenas troca de integrações demonstrativas por serviços externos de produção e ajustes finais de ambiente.

| Área | Peso | Estado | Evidências |
| --- | ---: | ---: | --- |
| Modelo de domínio e regras hierárquicas | 15% | 15% | Igrejas, usuários, cargos, solicitações, cartas, auditoria, validação de hierarquia e testes de regras essenciais cobertos. |
| API backend do MVP | 25% | 24% | Cadastro/login, igrejas, perfil, aprovar/rejeitar membros, cargos e pregadores, emissão/validação/suspensão/renovação de cartas e auditoria expostos por endpoints. |
| Persistência e segurança de referência | 15% | 14% | Repositórios em memória para execução local, hash PBKDF2, token demonstrativo, contrato JWT documentado e schema PostgreSQL versionado e validável em CI. |
| Cliente Flutter | 20% | 20% | Modelos, API client e telas cobrem dashboard, login, igrejas, solicitações, aprovações, cartas e auditoria do fluxo principal. |
| Documentação e contratos | 15% | 15% | README, arquitetura, API, permissões, database, deploy, OpenAPI e checklist de publicação estão sincronizados com o MVP. |
| Testabilidade e validações | 10% | 10% | Testes de workflow backend, testes de modelos Flutter, validação SQL e pipeline CI para build, análise e testes. |
| **Total** | **100%** | **99%** | Estado geral validado como completo para o MVP e pronto para homologação, restando apenas integrações produtivas externas. |

## Validação objetiva

- Backend possui cobertura automatizada dos fluxos críticos de hierarquia de igrejas, cadastro, solicitação de pregador, emissão de carta, alteração de cargo e auditoria.
- Banco possui migration PostgreSQL versionada e validação automatizada em ambiente PostgreSQL 16.
- Flutter possui testes de modelos e etapa de análise/teste configurada no pipeline.
- OpenAPI e documentação operacional descrevem endpoints, segurança, permissões e implantação.

## Pendências finais fora do MVP executável

Estas pendências representam o 1% restante para produção plena em ambiente real:

- Trocar o token demonstrativo por JWT assinado com rotação e refresh token persistido.
- Trocar repositórios em memória por implementação PostgreSQL em runtime, mantendo o schema já versionado.
- Integrar geração real de PDF/QR Code e armazenamento externo dos arquivos emitidos.
- Configurar segredos, domínio, observabilidade e política de backup do ambiente de produção.
