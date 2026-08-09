
# MetaReal

Sistema de gestão de rendimento e **metas** de vendedores: dashboard gerencial, lançamento diário de vendas, acompanhamento de metas e relatórios exportáveis — substituindo a antiga planilha de controle.

Backend em **C# / .NET (Web API + Clean Architecture)** com segurança de ponta a ponta (JWT + refresh token rotativo, RBAC, auditoria, rate limiting), e frontend em **React + TypeScript (PWA, mobile-first)**.

## Arquitetura

```
Backend/MetaReal/
  MetaReal.Domain        Entidades: Vendedor, RegistroVenda, MetaVenda, Usuario, RefreshToken, Auditoria
  MetaReal.Application   DTOs, Services (sem repositórios — consultam via IMetaRealDbContext),
                         regras de RBAC/posse e validação de entrada
  MetaReal.Infra.Data    MetaRealDbContext, Mappings (EF Core), Migrations, Seed
  MetaReal.Infra.Ioc     Composição de DI
  MetaReal.API           Web API (Controllers), JWT, middlewares de segurança,
                         SignalR (notificações) e geração de relatórios (Excel/PDF)

Frontend/                React 18 + TypeScript + Vite + Tailwind CSS, PWA
```

### Funcionalidades

- **Dashboard gerencial** — faturamento, ticket médio, taxa de conversão, ranking e evolução no tempo.
- **Lançamento diário** — atendimentos, vendas, garantia, crediário e pretas mistas, um registro por vendedor por dia.
- **Metas mensais** — definidas pelo Gerente (individualmente ou em lote para toda a equipe), com acompanhamento gamificado e **notificação em tempo real via SignalR** quando a meta é definida e quando é batida.
- **Relatórios** — 7 relatórios de negócio (desempenho, metas, evolução, produtividade, consistência de lançamentos, garantias e crediário), com exportação para **Excel e PDF**.
- **Auditoria** — trilha de quem alterou o quê e quando.

### Segurança

- Login com **JWT** (access token de 15 min) + **refresh token** opaco e rotativo, guardado hasheado no banco e entregue em cookie `HttpOnly; Secure; SameSite=Strict`.
- Senhas com **BCrypt** (fator de custo 12).
- **RBAC**: papéis `Gerente` e `Vendedor` — o Vendedor só lança e enxerga os próprios registros (checagem de posse no Service, não só no Controller).
- **Validação de entrada** dentro de cada Service, sem duplicação com o Controller.
- **Rate limiting** (janela restrita em `/api/auth/*`, mais generosa no resto da API).
- **Headers de segurança** (`X-Frame-Options`, `Content-Security-Policy`, etc.) e **CORS** restrito à origin do frontend.
- **Tratamento global de exceções**: nunca vaza stack trace em produção, sempre responde `{ success, error }`.
- **Transações** nas operações que gravam dado + auditoria juntos.

## Como Executar

Pré-requisitos: **.NET SDK 8+**, **Node.js 18+**, LocalDB (ou outro SQL Server acessível).

### 1. Backend
```bash
cd Backend/MetaReal/MetaReal.API
dotnet run
```
O banco é criado, migrado e populado automaticamente no primeiro `dotnet run` (connection string em `MetaReal.API/appsettings.json`) — inclui os dados reais de Fevereiro/2026 da planilha original e 3 usuários de demonstração:

| Papel | E-mail | Senha |
|---|---|---|
| Gerente | gerente@gmail.com | Gerente@123 |
| Vendedor (Luciana) | luciana@gmail.com | Vendedor@123 |
| Vendedor (Georgete) | georgete@gmail.com | Vendedor@123 |

> Senhas de demonstração — troque-as (ou remova o seed) antes de usar em produção.

A API sobe em `http://localhost:5025` (Swagger em `/swagger` no ambiente de desenvolvimento).

### 2. Frontend
```bash
cd Frontend
npm install
npm run dev
```
Abre em `http://localhost:5173`. A URL da API é configurada em `Frontend/.env.local` (`VITE_API_URL`).
