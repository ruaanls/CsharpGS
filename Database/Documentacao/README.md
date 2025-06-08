# Documentação de Execução

## Scripts SQL

### 1. Criação das Tabelas
![Criação das Tabelas](prints/01_criacao_tabelas.png)
- Execução do script `01_initial_setup.sql`
- Criação das tabelas USUARIO, DADOS_CHUVA, PREFERENCIAS_NOTIFICACAO e ALERTAS_TEMPERATURA
- Configuração de constraints e índices

### 2. Procedures DML
![Procedures DML](prints/02_procedures.png)
- Execução do script `02_procedures.sql`
- Criação das procedures de INSERT, UPDATE e DELETE
- Exemplo de execução de cada procedure

### 3. Funções
![Funções](prints/03_funcoes.png)
- Execução do script `06_funcoes_e_consultas.sql`
- Criação e teste das funções:
  - FN_CALCULAR_MEDIA_TEMPERATURA
  - FN_CONTAR_ALERTAS_POR_ESTADO

### 4. Blocos Anônimos
![Blocos Anônimos](prints/04_blocos_anonimos.png)
- Execução dos blocos anônimos
- Resultado do processamento com cursores
- Análise de dados por cidade e estado

### 5. Consultas Complexas
![Consultas Complexas](prints/05_consultas_complexas.png)
- Execução das 5 consultas SQL complexas
- Resultados das análises:
  - Temperatura por cidade/estado
  - Preferências por faixa de temperatura
  - Alertas por período
  - Preferências com alertas recentes
  - Tendência de temperatura

## Entity Framework Migrations

### 1. Criação da Migration Inicial
![Migration Inicial](prints/06_migration_inicial.png)
```bash
dotnet ef migrations add InitialCreate
```

### 2. Aplicação da Migration
![Aplicação da Migration](prints/07_aplicacao_migration.png)
```bash
dotnet ef database update
```

### 3. Verificação do Banco
![Verificação do Banco](prints/08_verificacao_banco.png)
- Estrutura das tabelas após a migration
- Constraints e relacionamentos
- Dados iniciais

## Testes

### 1. Testes Unitários
![Testes Unitários](prints/09_testes_unitarios.png)
```bash
dotnet test
```

### 2. Testes de API
![Testes de API](prints/10_testes_api.png)
- Execução da collection Postman
- Testes de endpoints
- Validação de respostas

## Observações
- Todos os prints foram capturados durante a execução real dos scripts
- Os resultados mostram o funcionamento correto de todas as funcionalidades
- As migrations foram aplicadas com sucesso
- Os testes passaram em todos os cenários 