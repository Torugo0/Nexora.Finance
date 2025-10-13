# 💜 Nexora – Mini Central Financeira (C# + Web API)

## 📖 Sobre a Nexora
A **Nexora** nasceu no **Challenge XP Investimentos (FIAP)** para democratizar a educação financeira com tecnologia, personalização e UX acessível.  
Esta versão é um recorte focado em **controle financeiro** (transações, saldo, consultas e integrações).

---

## 👇 O que tem neste repositório
- **Nexora.Finance.CLI** — app **Console** com menu (CRUD completo, resumo, exportação JSON).
- **Nexora.Finance.API** — **ASP.NET Core Web API + EF Core + Swagger**, com:
  - CRUD completo das transações;
  - **Consultas LINQ** (summary, busca filtrada, estatísticas por mês);
  - **Integrações externas** (câmbio com fallback e ViaCEP).

> **Banco Único (compartilhado):** **API e CLI usam o MESMO arquivo SQLite** em  
> **`%LocalAppData%\Nexora\nexora.db`**.  
> Se você fizer **POST** no Swagger, a transação **aparece no Console** (e o contrário também).

---

## 🗄️ Estrutura do projeto

```plaintext
Nexora.Finance/
├─ Nexora.Finance.CLI/                 # Console app
│  ├─ Program.cs
│  ├─ App.cs
│  ├─ Domain/
│  │  ├─ Transaction.cs
│  │  └─ TransactionType.cs
│  ├─ Data/
│  │  ├─ SqliteConnectionFactory.cs
│  │  ├─ DbInitializer.cs
│  │  └─ TransactionRepository.cs
│  └─ Services/
│     ├─ TransactionService.cs
│     └─ FileService.cs
└─ Nexora.Finance.API/                 # Web API (.NET 8 + EF Core + Swagger)
   ├─ Program.cs
   └─ Data/
      └─ TransactionDbContext.cs
```
### Arquitetura em Camadas
![Arquitetura](diagrama_arquitetura.png)

### Diagrama de Classes
![Classes](diagrama_classes.png)

---

## ⚙️ Tecnologias
- **C# 12 / .NET 8**
- **Entity Framework Core** (SQLite)
- **ASP.NET Core Web API** + **Swagger/OpenAPI**
- **System.Text.Json**
- **HttpClient** (integrações externas)

---

## 🚀 Como executar

### 1) Pré-requisitos
- .NET 8 SDK  
- Visual Studio 2022 ou `dotnet CLI`

### 2) Banco de dados (compartilhado)
O caminho do SQLite é **único** para API e CLI:
```
%LocalAppData%\Nexora\nexora.db
```
> O app cria a pasta/arquivo automaticamente na primeira execução.

### 3) Rodar a **API** (Swagger)
- Defina **Nexora.Finance.API** como *Startup Project* e rode (**F5**).  
- Abra: `https://localhost:<porta>/swagger`  
- Teste os endpoints (abaixo).

### 4) Rodar o **Console (CLI)**
- Defina **Nexora.Finance.CLI** como *Startup Project* → **F5**.  
- Use o menu para **Listar/Adicionar/Editar/Excluir**.  
- Como o banco é o mesmo, tudo que você criou na API já aparece aqui.

---

## 📦 Entidade principal

```csharp
Transaction {
  int Id,
  string Descricao,
  decimal Valor,
  TransactionType Tipo, // Entrada=1, Saida=2
  DateTime Data
}
```

---

## 🌐 Endpoints da API

### CRUD (EF Core)
- `GET /api/transactions` — lista (ordem: Data desc, Id desc)  
- `GET /api/transactions/{id}` — por Id  
- `POST /api/transactions` — cria (valida descrição e valor > 0)  
- `PUT /api/transactions/{id}` — atualiza campos enviados  
- `DELETE /api/transactions/{id}` — remove

### Consultas LINQ
- `GET /api/transactions/summary`  
  Retorna `{ entradas, saidas, saldo, totalTransacoes }`.
- `GET /api/transactions/search?from=&to=&tipo=&q=`  
  Filtros opcionais por período, tipo (**1** entrada / **2** saída) e texto.
- `GET /api/transactions/stats/by-month`  
  Agrupa `{ ano, mês, tipo, total, count }`.

### Integrações externas (20%)
- `GET  /api/external/exchange?base=BRL&symbols=USD`  
  Cotação com **fallback** automático (**exchangerate.host** → **frankfurter.app**).  
  Retorna `{ rate, provider, raw, fetchedAt }`.
- `POST /api/external/exchange/transaction?valor=100&from=USD&to=BRL`  
  Converte o valor com a taxa atual e **grava como Entrada** (aparece no CLI).
- `GET  /api/external/cep/{cep}`  
  Busca endereço no **ViaCEP**.

> Todos os endpoints estão documentados no **Swagger UI**.

---

## 📑 Documentação
- **Swagger/OpenAPI** incluído por padrão (`UseSwagger` + `UseSwaggerUI`).  
- Para publicar a documentação junto, basta expor a rota `/swagger` no host.

---

## ☁️ Publicação (resumo)
- **Render**: create *Web Service* → runtime .NET → comando `dotnet Nexora.Finance.API.dll`.  
- **Azure App Service**: `dotnet publish -c Release` e publicar via VS/GitHub Actions.  
- **Railway/Fly.io**: mesma ideia; pode usar Docker opcionalmente.

---

## 🧪 Dicas de teste rápido

1. **Criar** no Swagger:
   ```http
   POST /api/transactions
   {
     "descricao": "Teste Swagger",
     "valor": 120.50,
     "tipo": 1
   }
   ```
2. **Abrir o Console (CLI)** → **Listar**: o registro **já estará lá** (banco único).
3. **Cotação**:
   - `GET /api/external/exchange?base=BRL&symbols=USD`
   - `POST /api/external/exchange/transaction?valor=100&from=USD&to=BRL` → cria **Entrada** automática.

---

## ✅ Requisitos acadêmicos cobertos

- **ASP.NET Core Web API + EF Core com CRUD completo** — **OK**  
- **Pesquisas com LINQ** (summary/search/stats) — **OK**  
- **Endpoints integrando APIs externas** (câmbio com fallback + ViaCEP) — **OK**  
- **Documentação** (Swagger) — **OK**  
- **Arquitetura em diagramas** — **OK** (arquivos `.png` na raiz)  
- **Publicação em Cloud** — pronto para deploy (guia acima)

---

## 👨‍💻 Autores
Projeto desenvolvido pelos integrantes da Nexora (FIAP, 2025):  
Gabriel Machado Carrara Pimentel — RM99880 · Lourenzo Ramos — RM99951 · Letícia Resina — RM98069 · **Vítor Hugo Rodrigues — RM97758**.
