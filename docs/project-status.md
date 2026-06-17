# Estado real do projeto

Atualizado em 2026-06-17.

## Diagnóstico honesto

O projeto está em estado de **MVP técnico parcial, acima de um esqueleto inicial, mas abaixo de um produto pronto para homologação ou produção**. Ele possui base útil de domínio, contratos, API mínima, autenticação JWT HMAC em runtime, schema PostgreSQL versionado, DI configurado para repositórios PostgreSQL, testes de alguns fluxos e app Flutter iniciado. A afirmação de que o estado geral estaria abaixo de um marco inicial muito baixo não se sustenta pelas evidências do repositório; ainda assim, o projeto continua incompleto para uso real com dados sensíveis.

A documentação anterior usava indicadores numéricos de conclusão. Esses indicadores foram removidos porque davam uma impressão de precisão que o código não sustenta. A avaliação atual passa a ser qualitativa e baseada nas evidências do repositório.

## O que existe de fato

| Área | Estado observado | Evidências |
| --- | --- | --- |
| Backend ASP.NET Core | API minimal com rotas para cadastro/login, igrejas, perfil, aprovações, solicitações de pregador, cartas e auditoria. | Fluxos principais expostos e testados parcialmente. |
| Domínio e aplicação | Regras centrais existem em forma simplificada. | Cadastro valida dados básicos, login exige usuário aprovado, fluxo de pregador avança por etapas e emite carta simples. |
| Banco de dados | Migration PostgreSQL inicial relativamente completa e DI apontando para repositórios PostgreSQL no runtime. | Há tabelas para igrejas, usuários, cargos, tokens, solicitações, cartas, arquivos e auditoria; a infraestrutura registra repositórios PostgreSQL por padrão. |
| Testes | Cobertura de alguns fluxos importantes. | Há testes para hierarquia inválida, cadastro, pregador, carta, cargo e auditoria. |
| CI | Pipeline configurado para backend, banco e Flutter. | Restore/build/test .NET, validação SQL e análise/testes Flutter estão descritos no workflow. |
| Flutter | Cliente iniciado com modelos, API client e telas do fluxo principal. | Ainda precisa revisão de compilação, alinhamento com backend e amadurecimento de UX. |

## Pendências críticas

### Persistência runtime

O schema PostgreSQL está versionado e o DI atual registra repositórios PostgreSQL por padrão. Ainda há pontos de endurecimento antes de produção, especialmente transações explícitas para fluxos críticos que combinam banco, arquivo/storage e auditoria, além de validação operacional das migrations em ambiente real.

### Autenticação e sessão

O runtime já possui geração e validação de token JWT HMAC para o access token, mas ainda falta endurecimento produtivo: rotação/expiração operacional, refresh token realmente persistido e revogável, configuração segura de chaves, políticas de sessão e tratamento completo de credenciais.

### Autorização por cargo e hierarquia

Há agora um serviço explícito de autorização hierárquica para aprovar usuários, aprovar mudanças de cargo, aprovar etapas de pregador, emitir/suspender cartas e visualizar auditoria. Ele centraliza a regra que antes aparecia como checagens diretas de Pastor/Dirigente. Ainda é necessário amadurecer regras ministeriais finas, como etapa esperada por nível, impedimento de autoaprovação quando aplicável e políticas por igreja/cargo mais granulares.

### Cartas

O backend cria registros simples de carta. Geração real de PDF, QR Code, assinatura, layout, vínculo com arquivo e storage externo ainda são pendências.

### Flutter

O app Flutter tem estrutura inicial, mas precisa ser validado localmente com `flutter analyze` e `flutter test`, corrigindo inconsistências de modelos e melhorando telas ainda técnicas, como campos manuais de IDs.

## Próximos passos recomendados

1. Corrigir inconsistências do Flutter e garantir que o app compile.
2. Validar repositórios PostgreSQL em ambiente real e adicionar transações explícitas nos fluxos críticos.
3. Endurecer autenticação e sessão com refresh token persistido, revogação, chaves seguras e políticas produtivas.
4. Refinar autorização por etapa, autoaprovação e políticas ministeriais granulares sobre o serviço hierárquico já centralizado.
5. Alinhar domínio C#, contrato OpenAPI e schema SQL, especialmente status, campos obrigatórios e cartas.
6. Implementar emissão documental real com PDF, QR Code, storage e validação pública.
7. Ampliar testes negativos de autorização e testes de integração com PostgreSQL.
8. Rodar CI completo em ambiente com .NET, Flutter e PostgreSQL disponíveis.
