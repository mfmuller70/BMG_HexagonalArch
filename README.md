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
│   └── migrations/               # Scripts de banco (raiz)
├── Seguros.Tests/                # Testes
│   ├── Unit/
│   │   ├── Application/
│   │   │   └── Services/
│   │   └── Domain/
│   │       └── Entities/
│   ├── Integration/
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
