# Estado real do projeto

Atualizado em 2026-06-17.

## Diagnóstico honesto

O projeto está em estado de **protótipo/MVP técnico parcial**. Ele possui uma base útil de domínio, contratos, API mínima, schema PostgreSQL versionado, testes de alguns fluxos e um app Flutter iniciado, mas ainda não deve ser tratado como produto pronto para produção ou homologação com dados reais.

A documentação anterior usava percentuais de conclusão. Esses indicadores foram removidos porque davam uma impressão de avanço maior do que o código realmente sustenta. A avaliação atual passa a ser qualitativa e baseada nas evidências do repositório.

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

O runtime usa autenticação demonstrativa e tokens `demo.*`/`dev-admin`. JWT assinado, validação criptográfica, expiração curta e refresh token persistido ainda precisam ser implementados.

### Autorização por cargo e hierarquia

Os endpoints exigem autenticação, mas as regras ministeriais descritas na documentação ainda não estão aplicadas de forma segura nos serviços de aprovação. É necessário validar cargo, igreja, escopo hierárquico e permissões negativas.

### Cartas

O backend cria registros simples de carta. Geração real de PDF, QR Code, assinatura, layout, vínculo com arquivo e storage externo ainda são pendências.

### Flutter

O app Flutter tem estrutura inicial, mas precisa ser validado localmente com `flutter analyze` e `flutter test`, corrigindo inconsistências de modelos e melhorando telas ainda técnicas, como campos manuais de IDs.

## Próximos passos recomendados

1. Corrigir inconsistências do Flutter e garantir que o app compile.
2. Implementar repositórios PostgreSQL reais e trocar o DI para persistência em banco.
3. Substituir autenticação demonstrativa por JWT real com refresh token persistido.
4. Implementar autorização por cargo e hierarquia nos fluxos de aprovação.
5. Alinhar domínio C# e schema SQL, especialmente status, campos obrigatórios e cartas.
6. Implementar emissão documental real com PDF, QR Code, storage e validação pública.
7. Adicionar testes negativos de autorização e testes de integração com PostgreSQL.
8. Rodar CI completo em ambiente com .NET, Flutter e PostgreSQL disponíveis.
