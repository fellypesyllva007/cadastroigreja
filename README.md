# CadastroIgreja

Sistema de gestão e cadastro de membros, cargos e autorização de pregadores para igrejas organizadas em estrutura hierárquica.

## Objetivo

Centralizar o cadastro de membros, controle de cargos e emissão de cartas de pregação através de um sistema moderno, seguro e multiplataforma.

## Tecnologias

### Frontend
- Flutter
- Web
- Android
- iOS
- Windows
- macOS

### Backend
- ASP.NET Core 9

### Banco de Dados
- PostgreSQL

### Autenticação
- JWT Bearer Token

## Estrutura Hierárquica

1. Sede
2. Regional
3. Setorial
4. Congregação Local
5. Casa de Oração

Sede > Regional > Setorial > Congregação Local > Casa de Oração

## Cargos
- Membro
- Diácono
- Presbítero
- Pastor
- Dirigente

O cargo não concede automaticamente permissão para pregar.

## Controle de Pregadores

Fluxo Casa de Oração:
1. Aprovação Casa de Oração
2. Aprovação Congregação Local
3. Aprovação final Setorial
4. Emissão da carta

## Matriz de Permissões

### Casa de Oração
- Aprovar membros locais
- Pré-aprovar pregadores

### Congregação Local
- Aprovar membros subordinados
- Pré-aprovar pregadores

### Setorial
- Aprovação final de pregadores
- Emitir cartas
- Gerenciar locais e casas de oração

### Regional
- Consultas e relatórios

### Sede
- Controle total do sistema

## Segurança
- JWT
- Auditoria
- Histórico de aprovações

## Roadmap
- Cadastro de igrejas
- Cadastro de membros
- Controle de cargos
- Aprovação de pregadores
- Emissão de cartas
- QR Code

## Licença
Projeto privado.