# CadastroIgreja

Sistema de gestão e cadastro de membros, cargos, dirigentes e autorização de pregadores para igrejas organizadas em estrutura hierárquica.

## Objetivo

Centralizar o cadastro de membros, controle ministerial, fluxo de aprovações e emissão de cartas de pregação através de uma plataforma moderna, segura e multiplataforma.

---

# Tecnologias

## Frontend

Flutter

Plataformas suportadas:
- Web
- Android
- iOS
- Windows
- macOS
- Linux

## Backend

ASP.NET Core 9

## Banco de Dados

PostgreSQL

## Autenticação

JWT Bearer Token

## Armazenamento

- Cartas PDF
- Fotos de perfil
- Documentos

---

# Estrutura Hierárquica

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

---

# Cadastro

O usuário informa o nível ao qual pertence.

- Sede → seleciona apenas a Sede.
- Regional → seleciona Sede e Regional.
- Setorial → seleciona Sede, Regional e Setorial.
- Congregação Local → seleciona Sede, Regional, Setorial e Congregação.
- Casa de Oração → seleciona toda a hierarquia até a Casa.

Todos os usuários iniciam como Membro.

---

# Cargos

- Membro
- Diácono
- Presbítero
- Pastor
- Dirigente

Uma igreja pode possuir vários dirigentes.

O cargo não concede automaticamente autorização para pregar.

---

# Controle de Pregadores

A autorização de pregação é independente do cargo.

## Fluxo Casa de Oração

1. Solicitação do usuário.
2. Aprovação do dirigente da Casa de Oração.
3. Aprovação do dirigente da Congregação Local.
4. Aprovação final do dirigente Setorial.
5. Emissão da Carta de Pregação.

## Fluxo Congregação Local

1. Solicitação do usuário.
2. Aprovação do dirigente Local.
3. Aprovação final do dirigente Setorial.
4. Emissão da Carta.

## Fluxo Setorial

1. Solicitação do usuário.
2. Aprovação final do dirigente Setorial.
3. Emissão da Carta.

---

# Funcionalidades

## Usuário

- Cadastro
- Login
- Recuperação de senha
- Consulta de perfil
- Solicitação de alteração de cargo
- Solicitação de autorização para pregar
- Consulta de cartas emitidas

## Dirigente

- Aprovação de membros
- Aprovação de cargos
- Aprovação de pregadores
- Consulta de relatórios
- Gerenciamento de igrejas subordinadas

## Administração

- Gerenciamento de Sedes
- Regionais
- Setoriais
- Congregações Locais
- Casas de Oração
- Controle geral do sistema

---

# Matriz de Permissões

## Casa de Oração

Pode:
- Aprovar membros da própria Casa.
- Aprovar alterações de cargo locais.
- Pré-aprovar pregadores.
- Consultar membros locais.

Não pode:
- Emitir cartas.
- Aprovar pregadores em definitivo.

## Congregação Local

Pode:
- Aprovar membros da Congregação e Casas subordinadas.
- Aprovar alterações de cargo.
- Pré-aprovar pregadores.
- Consultar relatórios locais.

Não pode:
- Emitir cartas.
- Aprovar pregadores em definitivo.

## Setorial

Pode:
- Aprovar membros.
- Aprovar cargos.
- Aprovar pregadores.
- Emitir cartas de pregação.
- Suspender cartas.
- Renovar cartas.
- Gerenciar Congregações Locais e Casas de Oração.

É o aprovador final das cartas de pregação.

## Regional

Pode:
- Consultar Setoriais subordinadas.
- Consultar membros.
- Consultar pregadores.
- Consultar cartas emitidas.
- Emitir relatórios regionais.

## Sede

Controle total do sistema:
- Gerenciar toda a estrutura.
- Gerenciar usuários.
- Gerenciar permissões.
- Auditar aprovações.
- Configurar regras globais.

---

# Carta de Pregação

A carta conterá:
- Nome completo
- Cargo
- Igreja de origem
- Número da carta
- Data de emissão
- Data de validade
- QR Code de validação

---

# Segurança

- JWT Authentication
- Controle de permissões por cargo
- Controle de permissões por nível hierárquico
- Auditoria de ações
- Histórico de aprovações

---

# Roadmap

## Fase 1
- Cadastro de igrejas
- Cadastro de membros
- Login
- Controle de cargos

## Fase 2
- Aprovação de membros
- Aprovação de cargos
- Solicitação de pregadores

## Fase 3
- Emissão de cartas
- QR Code
- Validação pública

## Fase 4
- Aplicativos móveis
- Relatórios avançados
- Notificações
- Assinatura digital

---

# Licença

Projeto privado.
