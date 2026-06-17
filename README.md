# CadastroIgreja

Sistema de gestão e cadastro de membros, cargos, dirigentes e autorização de pregadores para igrejas organizadas em estrutura hierárquica.

## Objetivo

Centralizar o cadastro de membros, controle ministerial, fluxo de aprovações e emissão de cartas de pregação através de uma plataforma moderna, segura e multiplataforma.

## Tecnologias

- Frontend: Flutter, unificado em `frontend/`.
- Backend: ASP.NET Core 9.
- Banco de dados: PostgreSQL.
- Autenticação: JWT Bearer HMAC no runtime atual.
- Armazenamento planejado: cartas PDF, fotos de perfil e documentos.

## Estrutura hierárquica

1. Sede
2. Regional
3. Setorial
4. Congregação Local
5. Casa de Oração

```text
Sede
 └── Regional
      └── Setorial
           └── Congregação Local
                └── Casa de Oração
```

## Cadastro

O usuário informa o nível ao qual pertence:

- Sede: seleciona apenas a Sede.
- Regional: seleciona Sede e Regional.
- Setorial: seleciona Sede, Regional e Setorial.
- Congregação Local: seleciona Sede, Regional, Setorial e Congregação.
- Casa de Oração: seleciona toda a hierarquia até a Casa.

Todos os usuários iniciam como Membro.

## Cargos

- Membro
- Diácono
- Presbítero
- Pastor
- Dirigente

Uma igreja pode possuir vários dirigentes. O cargo não concede automaticamente autorização para pregar.

## Controle de pregadores

A autorização de pregação é independente do cargo.

### Fluxo Casa de Oração

1. Solicitação do usuário.
2. Aprovação do dirigente da Casa de Oração.
3. Aprovação do dirigente da Congregação Local.
4. Aprovação final do dirigente Setorial.
5. Emissão da carta de pregação.

### Fluxo Congregação Local

1. Solicitação do usuário.
2. Aprovação do dirigente Local.
3. Aprovação final do dirigente Setorial.
4. Emissão da carta.

### Fluxo Setorial

1. Solicitação do usuário.
2. Aprovação final do dirigente Setorial.
3. Emissão da carta.

## Funcionalidades

### Usuário

Implementado no MVP técnico:

- Cadastro
- Login
- Consulta de perfil
- Solicitação de alteração de cargo
- Solicitação de autorização para pregar
- Consulta de cartas emitidas

Pendente:

- Recuperação de senha em fluxo produtivo

### Dirigente

- Aprovação de membros
- Aprovação de cargos
- Aprovação de pregadores
- Consulta de relatórios
- Gerenciamento de igrejas subordinadas

### Administração

- Gerenciamento de Sedes, Regionais, Setoriais, Congregações Locais e Casas de Oração
- Controle geral do sistema
- Auditoria de aprovações
- Configuração de regras globais

## Matriz de permissões

### Casa de Oração

Pode aprovar membros da própria Casa, aprovar alterações de cargo locais, pré-aprovar pregadores e consultar membros locais.

Não pode emitir cartas nem aprovar pregadores em definitivo.

### Congregação Local

Pode aprovar membros da Congregação e Casas subordinadas, aprovar alterações de cargo, pré-aprovar pregadores e consultar relatórios locais.

Não pode emitir cartas nem aprovar pregadores em definitivo.

### Setorial

Pode aprovar membros, aprovar cargos, aprovar pregadores, emitir cartas de pregação, suspender cartas, renovar cartas e gerenciar Congregações Locais e Casas de Oração.

É o aprovador final das cartas de pregação.

### Regional

Pode consultar Setoriais subordinadas, membros, pregadores, cartas emitidas e relatórios regionais.

### Sede

Controle total do sistema.

## Carta de pregação

A carta planejada conterá nome completo, cargo, igreja de origem, número da carta, data de emissão, data de validade e QR Code de validação.

No runtime atual, a carta é um registro com número, validade, status e URL de validação. A geração real de PDF, QR Code, assinatura, layout e armazenamento ainda precisa ser concluída.

## Como executar a infraestrutura local

```bash
docker compose up -d db
```

O banco é inicializado com as migrações em `database/migrations`. A especificação da API está em `openapi/cadastroigreja.v1.yaml`.

## Estado atual do projeto

O projeto possui backend ASP.NET Core, cliente Flutter, contrato OpenAPI, migration PostgreSQL e pipeline de CI. Antes de liberar uso por usuários finais, ainda é necessário concluir as regras produtivas de sessão, persistência, autorização hierárquica, emissão real de documentos e validação completa do frontend.

Consulte `docs/project-status.md` e `docs/deployment-checklist.md` antes de publicar em servidor.
