# Weather Alert API

API de monitoramento de temperaturas e alertas climáticos desenvolvida para a Global Solution 2025-1.

## Integrantes
- Ruan Lima Silva - RM558775
- Richardy Borges Santana - RM557883
- Marcos Vinicius Pereira de Oliveira - RM557252

## Tecnologias Utilizadas
- .NET 9.0
- Oracle Database
- Dapper (Micro ORM)
- Swagger/OpenAPI
- xUnit para testes

## Pré-requisitos
- .NET 9.0 SDK
- Oracle Database 21c ou superior
- Visual Studio 2022 ou VS Code

## Como Rodar o Projeto

1. Clone o repositório
```powershell
git clone [url-do-repositorio]
```

2. Configure a string de conexão com o Oracle
Abra o arquivo `appsettings.json` e configure a conexão:
```json
{
  "Database": {
    "ConnectionString": "Data Source=seu_tns;User Id=seu_usuario;Password=sua_senha;"
  }
}
```

3. Execute as migrations do banco de dados
- Execute os scripts disponíveis na pasta `Database/Scripts`

4. Execute o projeto
```powershell
cd WeatherAlertAPI
dotnet restore
dotnet run
```

5. Acesse a documentação Swagger
- Abra o navegador em: `https://localhost:5001`

## Como Executar os Testes

1. Testes Unitários
```powershell
cd WeatherAlertAPI.Tests
dotnet test
```

2. Testes de API (usando o arquivo http)
- Abra o arquivo `Tests/complete-api-tests.http`
- Use a extensão REST Client do VS Code para executar os testes

## Estrutura do Projeto

- `Controllers/`: Endpoints da API
- `Models/`: Classes de domínio
- `Services/`: Lógica de negócios
- `Configuration/`: Configurações da aplicação
- `Tests/`: Testes da API

## Funcionalidades

- CRUD de Preferências de Notificação
- Monitoramento de Temperaturas
- Geração de Alertas
- Consultas via Stored Procedures
