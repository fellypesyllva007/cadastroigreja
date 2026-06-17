# Estado real do projeto

Atualizado em 2026-06-17.

## Diagnóstico honesto

O projeto está em estado de **MVP técnico parcial, acima de um esqueleto inicial, mas abaixo de um produto pronto para homologação ou produção**. Ele possui base útil de domínio, contratos, API mínima, autenticação JWT HMAC em runtime, schema PostgreSQL versionado, testes de alguns fluxos e app Flutter iniciado. A afirmação de que o estado geral estaria abaixo de um marco inicial muito baixo não se sustenta pelas evidências do repositório; ainda assim, o projeto continua incompleto para uso real com dados sensíveis.

A documentação anterior usava indicadores numéricos de conclusão. Esses indicadores foram removidos porque davam uma impressão de precisão que o código não sustenta. A avaliação atual passa a ser qualitativa e baseada nas evidências do repositório.

## O que existe de fato

| Área | Estado observado | Evidências |
| --- | --- | --- |
| Backend ASP.NET Core | API minimal com rotas para cadastro/login, igrejas, perfil, aprovações, solicitações de pregador, cartas e auditoria. | Fluxos principais expostos e testados parcialmente. |
| Domínio e aplicação | Regras centrais existem em forma simplificada. | Cadastro valida dados básicos, login exige usuário aprovado, fluxo de pregador avança por etapas e emite carta simples. |
| Banco de dados | Migration PostgreSQL inicial relativamente completa. | Há tabelas para igrejas, usuários, cargos, tokens, solicitações, cartas, arquivos e auditoria, além de validações de hierarquia. |
| Testes | Cobertura de alguns fluxos importantes. | Há testes para hierarquia inválida, cadastro, pregador, carta, cargo e auditoria. |
| CI | Pipeline configurado para backend, banco e Flutter. | Restore/build/test .NET, validação SQL e análise/testes Flutter estão descritos no workflow. |
| Flutter | Cliente iniciado com modelos, API client e telas do fluxo principal. | Ainda precisa revisão de compilação, alinhamento com backend e amadurecimento de UX. |

## Pendências críticas

### Persistência runtime

O schema PostgreSQL está versionado, mas a API ainda usa repositórios em memória. Isso faz os dados desaparecerem quando a aplicação reinicia e impede uso real em produção.

### Autenticação e sessão

O runtime já possui geração e validação de token JWT HMAC para o access token, mas ainda falta endurecimento produtivo: rotação/expiração operacional, refresh token realmente persistido e revogável, configuração segura de chaves, políticas de sessão e tratamento completo de credenciais.

### Autorização por cargo e hierarquia

Os endpoints exigem autenticação e algumas aprovações checam se o aprovador é Pastor ou Dirigente, mas as regras ministeriais descritas na documentação ainda não estão completas. É necessário validar cargo, igreja, escopo hierárquico, etapa esperada, impedimento de autoaprovação quando aplicável e permissões negativas.

### Cartas

O backend cria registros simples de carta. Geração real de PDF, QR Code, assinatura, layout, vínculo com arquivo e storage externo ainda são pendências.

### Flutter

O app Flutter tem estrutura inicial, mas precisa ser validado localmente com `flutter analyze` e `flutter test`, corrigindo inconsistências de modelos e melhorando telas ainda técnicas, como campos manuais de IDs.

## Próximos passos recomendados

1. Corrigir inconsistências do Flutter e garantir que o app compile.
2. Implementar repositórios PostgreSQL reais e trocar o DI para persistência em banco.
3. Endurecer autenticação e sessão com refresh token persistido, revogação, chaves seguras e políticas produtivas.
4. Implementar autorização por cargo, igreja, hierarquia e etapa nos fluxos de aprovação.
5. Alinhar domínio C#, contrato OpenAPI e schema SQL, especialmente status, campos obrigatórios e cartas.
6. Implementar emissão documental real com PDF, QR Code, storage e validação pública.
7. Adicionar testes negativos de autorização e testes de integração com PostgreSQL.
8. Rodar CI completo em ambiente com .NET, Flutter e PostgreSQL disponíveis.
