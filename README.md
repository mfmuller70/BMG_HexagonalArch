# 🏗️ Sistema de Seguros BMG - Arquitetura Hexagonal

Sistema de gerenciamento de propostas e contratações de seguros desenvolvido com **Arquitetura Hexagonal** (Ports & Adapters), **DDD** e **SOLID**.

## 📋 Visão Geral

Sistema BMG focado em **Propostas de Seguro** e **Contratações**, com comunicação assíncrona via **RabbitMQ** e persistência em **SQL Server**. O sistema permite o fluxo completo desde a criação de uma proposta até sua contratação, com validações de negócio e comunicação entre serviços.

### 🎯 Funcionalidades Principais

#### **PropostaService**
- ✅ **Criar proposta de seguro** - Cadastro de novas propostas com validações
- ✅ **Listar propostas** - Consulta de todas as propostas cadastradas
- ✅ **Obter proposta por ID** - Consulta específica de uma proposta
- ✅ **Alterar status da proposta** - Transições de status controladas

#### **ContratacaoService**
- ✅ **Contratar uma proposta** - Somente se aprovada, com geração de número de contrato
- ✅ **Verificar status da proposta** - Consulta se proposta existe e está disponível

## 🏛️ Arquitetura

### **Estrutura do Projeto**
```
BMG/
├── API/                          # Camada de Apresentação
│   ├── Controllers/
│   └── Properties/
├── Application/                  # Camada de Aplicação
│   ├── DTOs/
│   ├── Mappers/
│   └── Services/
├── Domain/                       # Camada de Domínio
│   ├── Entities/
│   ├── Ports/
│   ├── Exceptions/
│   └── ValueObjects/
├── Infra.Data/                   # Camada de Infraestrutura
│   ├── Context/
│   ├── Adapters/
│   └── migrations/
├── migrations/                   # Scripts de banco (raiz)
├── Seguros.Tests/                # Testes
│   ├── Unit/
│   │   ├── Application/
│   │   │   └── Services/
│   │   └── Domain/
│   │       └── Entities/
│   ├── Integration/
│   └── TestResults/
├── migrations/                   # Scripts de banco
├── docker-compose.yml            # Docker Compose
├── Dockerfile                    # Container da aplicação
├── HexagonalArch.sln            # Solution file
└── README.md                     # Documentação
```

### **Diagrama da Arquitetura Hexagonal**

```
                    🌐 EXTERNAL ACTORS
                         │
                    ┌────▼────┐
                    │   API   │ ◄─── Presentation Layer
                    │Controllers│      (HTTP/REST)
                    └────┬────┘
                         │
                    ┌────▼────┐
                    │Application│ ◄─── Application Layer
                    │ Services │      (DTOs, Mappers)
                    └────┬────┘
                         │
              ┌──────────▼──────────┐
              │      DOMAIN         │ ◄─── Core Business Logic
              │   ┌─────────────┐   │      (Entities, Ports)
              │   │   Entities  │   │
              │   │   Proposta  │   │
              │   │ Contratacao │   │
              │   └─────────────┘   │
              │   ┌─────────────┐   │
              │   │    Ports    │   │      Interfaces
              │   │(IRepository)│   │      (Contracts)
              │   │ (IService)  │   │
              │   └─────────────┘   │
              └──────────┬──────────┘
                         │
    ┌─────────────────────────────────────────────────────────┐
    │                   INFRASTRUCTURE                        │ ◄─── External Adapters
    │                                                         │
    │ ┌─────────────────┐        ┏━━━━━━━━━━━━━━━━━━━━━━━━━━┓ │
    │ │  SQL Server     │        ┃    Docker Container      ┃ │
    │ │  Repository     │        ┃ ┌────────────────────┐   ┃ │
    │ │  Persistence    │        ┃ │     RabbitMQ       │   ┃ │
    │ └─────────────────┘        ┃ │     Messaging      │   ┃ │
    │    Database                ┃ │  (5672/15672)      │   ┃ │
    │                            ┃ └────────────────────┘   ┃ │
    │                            ┗━━━━━━━━━━━━━━━━━━━━━━━━━━┛ │
    └─────────────────────────────────────────────────────────┘

    ┌─────────────────────────────────────────────────────────┐
    │                    FLOW DIRECTION                       │
    │  External → API → Application → Domain ← Infrastructure │
    │           Request              Business      Data       │
    │                                Logic      & Messages    │
    └─────────────────────────────────────────────────────────┘
```

## 📊 Status das Propostas

O sistema trabalha com **4 status** para propostas, com transições controladas:

### **Status Disponíveis**
1. **EmAnalise** (1) - Status inicial quando proposta é criada
2. **Aprovada** (2) - Proposta aprovada para contratação
3. **Rejeitada** (3) - Proposta rejeitada
4. **Contratada** (4) - Proposta contratada (status final)

### **Regras de Transição**
- ✅ **EmAnalise** → **Aprovada** ou **Rejeitada**
- ✅ **Aprovada** → **Contratada** (apenas via contratação)
- ❌ **Contratada** → Nenhum outro status (status final)
- ❌ **Rejeitada** → Nenhum outro status

## 🗄️ Banco de Dados SQL Server

### **Configuração**
- **Servidor**: MULLER
- **Banco**: DBSeguros_BMG
- **Autenticação**: Windows Authentication
- **Trust Server Certificate**: Habilitado

### **Tabelas**
- **Banco**: DBSeguros_BMG
- **Propostas**
- **Contratacoes**

### **Migrations**
- **InitialCreate**: Criação inicial das tabelas
- **Comando**: `dotnet ef database update --project Infra.Data --startup-project API`

## 🐰 RabbitMQ Message Broker

### **Configuração**
- **Host**: localhost
- **Porta AMQP**: 5672
- **Porta Management**: 15672
- **Usuário**: guest
- **Senha**: guest
- **Virtual Host**: /

### **Fila**

- Fila única: `status.queue` (durável)
- Publicação via exchange padrão (amq.default) usando routingKey = `status.queue`

### **Mensagens Publicadas**
- Evento de mudança de status contendo: PropostaId, StatusAnterior, NovoStatus, Timestamp, Evento

### **Estrutura das Mensagens**
```json
{
  "PropostaId": "guid",
  "StatusAnterior": "EmAnalise",
  "NovoStatus": "Aprovada", 
  "Timestamp": "2025-01-03T10:30:00Z",
  "Evento": "MudancaStatus"
}
```

### Observações
- Não há consumidores automáticos na API; as mensagens permanecem na fila para visibilidade no RabbitMQ Management.

## 🚀 Como Executar

### **Pré-requisitos**
- .NET 8 SDK
- SQL Server (Local ou Docker)
- Docker Desktop (para RabbitMQ)

### **1. Executar Migrations**
```bash
# Na pasta do projeto
dotnet ef database update --project Infra.Data --startup-project API
```

### **2. Executar RabbitMQ (Docker)**
```bash
# Iniciar RabbitMQ
docker-compose up -d

# Verificar se está rodando
docker ps
```

### **3. Executar a Aplicação**
```bash
# Executar API
dotnet run --project API

# Acessar Swagger
http://localhost:5000/swagger
```

## 🔧 Configuração

### **Docker Compose**
```yaml
version: '3.8'
services:
  rabbitmq:
    image: rabbitmq:3-management
    container_name: seguros-rabbitmq
    ports:
      - "5672:5672"   # AMQP port
      - "15672:15672" # Management UI
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
      RABBITMQ_DEFAULT_VHOST: /
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - seguros-network
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 30s
      timeout: 10s
      retries: 5
```

## 📡 Endpoints da API

### **Propostas**
- `GET /api/propostas` - Listar todas as propostas
- `POST /api/propostas` - Criar nova proposta
- `GET /api/propostas/{id}` - Obter proposta por ID
- `PUT /api/propostas/{id}/status` - Alterar status da proposta

### **Contratações**
- `POST /api/contratacoes` - Contratar proposta (apenas se aprovada)
- `GET /api/contratacoes/verificar-status/{propostaId}` - Verificar se proposta existe

## 🧪 Testes

### **Estrutura de Testes**
- **Unit Tests**: Testes unitários para entidades e serviços
- **Integration Tests**: Testes de integração com banco e RabbitMQ

### **Executar Testes**
```bash
# Todos os testes
dotnet test

# Testes unitários
dotnet test --filter "Category=Unit"

# Testes de integração
dotnet test --filter "Category=Integration"
```

### **Cobertura de Testes**
```bash
# Gerar relatório de cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## 🔄 Fluxo de Dados

### **Criação de Proposta**
1. **POST** `/api/propostas` → Cria proposta com status "EmAnalise"
2. **Validação** → Nome do cliente (min. 3 caracteres) e valor > 0
3. **RabbitMQ** → Publica evento `proposta.criada`
4. **Banco** → Persiste proposta no SQL Server

### **Alteração de Status**
1. **PUT** `/api/propostas/{id}/status` → Altera status (Aprovada/Rejeitada)
2. **Validação** → Verifica se proposta existe e pode ser alterada
3. **RabbitMQ** → Publica evento `proposta.status.alterado`
4. **Banco** → Atualiza status e data de atualização

### **Contratação**
1. **POST** `/api/contratacoes` → Contrata proposta (apenas se aprovada)
2. **Validação** → Verifica se proposta existe e está aprovada
3. **PropostaService** → Atualiza status para "Contratada"
4. **Geração** → Cria número de contrato único (CTR + data + GUID)
5. **RabbitMQ** → Publica evento `contratacao.realizada`
6. **Banco** → Persiste contratação e atualiza proposta

### **Verificação de Status**
1. **GET** `/api/contratacoes/verificar-status/{propostaId}` → Verifica se proposta existe
2. **PropostaService** → Consulta status da proposta
3. **Resposta** → Retorna se proposta existe e está disponível

## 🐳 Docker

### **Executar com Docker**
```bash
# Apenas RabbitMQ
docker-compose up -d

# Ou executar a aplicação localmente
dotnet run --project API
```

### **Acessar RabbitMQ Management**
- **URL**: http://localhost:15672
- **Usuário**: guest
- **Senha**: guest

### **Testar Eventos RabbitMQ**
```bash
# 1. Criar uma proposta (publica evento EmAnalise)
POST /api/propostas
{
  "clienteNome": "João Silva",
  "valorCobertura": 50000
}

# 2. Aprovar proposta (publica evento Aprovada)
PUT /api/propostas/{id}/status
{
  "status": "Aprovada"
}

# 3. Contratar proposta (publica evento Contratada)
POST /api/contratacoes
{
  "propostaId": "guid-da-proposta"
}
```

## 📊 Monitoramento

### **Logs**
- **RabbitMQ**: Conectado automaticamente na inicialização
- **SQL Server**: Queries executadas são logadas
- **Aplicação**: Logs estruturados com diferentes níveis

### **Health Checks**
- **API**: http://localhost:5000/swagger
- **RabbitMQ**: http://localhost:15672
- **SQL Server**: Conexão testada na inicialização

## 🏗️ Arquitetura Hexagonal

### **Ports (Interfaces)**
- `IPropostaService` - Lógica de negócio de propostas
- `IContratacaoService` - Lógica de negócio de contratações
- `IMessageService` - Comunicação assíncrona
- `IPropostaRepository` - Persistência de propostas
- `IContratacaoRepository` - Persistência de contratações

### **Adapters (Implementações)**
- `PropostaServiceManager` - Implementação da lógica de propostas
- `ContratacaoServiceManager` - Implementação da lógica de contratações
- `RabbitMQService` - Implementação do RabbitMQ
- `PropostaRepository` - Implementação do repositório de propostas
- `ContratacaoRepository` - Implementação do repositório de contratações

### **Entidades**
- `Proposta` - Entidade principal com status e validações
- `Contratacao` - Entidade simplificada com número de contrato
- `StatusProposta` - Enum com 4 status possíveis

## 🔧 Tecnologias

- **.NET 8** - Framework principal
- **Entity Framework Core** - ORM
- **SQL Server** - Banco de dados
- **RabbitMQ** - Message Broker
- **Docker** - Containerização
- **xUnit** - Testes unitários
- **Moq** - Mocking
- **FluentAssertions** - Assertions

## 📝 Notas

- ✅ **Arquitetura limpa** com separação clara de responsabilidades
- ✅ **Sistema robusto** com validações de negócio
- ✅ **Status centralizado** apenas para propostas
- ✅ **Comunicação assíncrona** via RabbitMQ
- ✅ **Configuração flexível** para diferentes ambientes
- ✅ **Documentação completa** com Swagger
- ✅ **Testes abrangentes** unitários e de integração
- ✅ **Tratamento de exceções** específicas do domínio

---

## 📄 Requisitos do Sistema

Para informações detalhadas sobre os requisitos funcionais e não funcionais desta solução, consulte:

**📋 [Requisitos BMG_INDT - Arquitetura Hexagonal.pdf](./Requisitos%20BMG_INDT%20-%20Arquitetura%20Hexagonal.pdf)**

Este documento contém todas as especificações técnicas, regras de negócio e critérios de aceitação utilizados na construção desta solução.

> **Nota:** O arquivo PDF está atualmente nos Solution Items do Visual Studio, mas precisa ser copiado para a raiz do repositório para que o link funcione no GitHub.


## 📄 Copyright
© 2025 **Marcos Muller**. Todos os direitos reservados.
