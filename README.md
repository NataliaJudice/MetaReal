# MetaReal

Sistema web para gestão de vendas e metas de vendedores.

## Tecnologias

- **Backend:** C# / .NET 8 / ASP.NET Core Web API
- **Arquitetura:** Clean Architecture
- **Banco:** SQL Server + Entity Framework Core
- **Frontend:** React + TypeScript + Vite + Tailwind CSS
- **Tempo real:** SignalR

## Funcionalidades

- Dashboard de vendas e desempenho
- Registro e acompanhamento de vendas
- Criação e acompanhamento de metas
- Relatórios em Excel e PDF
- Notificações em tempo real
- Auditoria
- Controle de acesso por perfil

## Estrutura

```text
Backend/
├── MetaReal.Domain
├── MetaReal.Application
├── MetaReal.Infra.Data
├── MetaReal.Infra.Ioc
└── MetaReal.API

Frontend/
└── React + TypeScript
```

## Segurança

- JWT
- Refresh Token com rotação
- BCrypt para senhas
- Controle de acesso por perfil
- Validação de acesso aos próprios registros
- CORS e headers de segurança
- Tratamento global de exceções

## Como executar

### Backend

```bash
cd Backend/MetaReal/MetaReal.API
dotnet run
```

### Frontend

```bash
cd Frontend
npm install
npm run dev
```

Configure a conexão com o banco no `appsettings.json` e a URL da API no `.env.local`.

## Demonstração

O projeto utiliza dados de demonstração para facilitar a visualização das funcionalidades.
