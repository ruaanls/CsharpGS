# Documentação do Banco de Dados - Weather Alert API

## Estrutura do Banco

### Tabelas

1. PREFERENCIAS_NOTIFICACAO
```sql
CREATE TABLE PREFERENCIAS_NOTIFICACAO (
    ID_PREFERENCIA NUMBER PRIMARY KEY,
    CIDADE VARCHAR2(100) NOT NULL,
    ESTADO VARCHAR2(2) NOT NULL,
    TEMPERATURA_MIN NUMBER(5,2),
    TEMPERATURA_MAX NUMBER(5,2),
    ATIVO CHAR(1) DEFAULT 'S',
    DATA_CRIACAO TIMESTAMP DEFAULT SYSTIMESTAMP,
    DATA_ATUALIZACAO TIMESTAMP
);

CREATE SEQUENCE SEQ_PREFERENCIA START WITH 1 INCREMENT BY 1;
```

2. ALERTA_TEMPERATURA
```sql
CREATE TABLE ALERTA_TEMPERATURA (
    ID_ALERTA NUMBER PRIMARY KEY,
    ID_PREFERENCIA NUMBER,
    TEMPERATURA NUMBER(5,2) NOT NULL,
    DATA_MEDICAO TIMESTAMP DEFAULT SYSTIMESTAMP,
    STATUS VARCHAR2(20) DEFAULT 'ATIVO',
    FOREIGN KEY (ID_PREFERENCIA) REFERENCES PREFERENCIAS_NOTIFICACAO(ID_PREFERENCIA)
);

CREATE SEQUENCE SEQ_ALERTA START WITH 1 INCREMENT BY 1;
```

## Procedures

### 1. Inserir Preferência
```sql
CREATE OR REPLACE PROCEDURE SP_INSERIR_PREFERENCIA (
    p_cidade IN VARCHAR2,
    p_estado IN VARCHAR2,
    p_temp_min IN NUMBER,
    p_temp_max IN NUMBER,
    p_id_out OUT NUMBER
) AS
BEGIN
    INSERT INTO PREFERENCIAS_NOTIFICACAO (
        ID_PREFERENCIA,
        CIDADE,
        ESTADO,
        TEMPERATURA_MIN,
        TEMPERATURA_MAX,
        DATA_CRIACAO,
        DATA_ATUALIZACAO
    ) VALUES (
        SEQ_PREFERENCIA.NEXTVAL,
        p_cidade,
        p_estado,
        p_temp_min,
        p_temp_max,
        SYSTIMESTAMP,
        SYSTIMESTAMP
    ) RETURNING ID_PREFERENCIA INTO p_id_out;
    COMMIT;
END;
/

### 2. Atualizar Status de Preferência
```sql
CREATE OR REPLACE PROCEDURE SP_ATUALIZAR_STATUS_PREF (
    p_id IN NUMBER,
    p_ativo IN CHAR
) AS
BEGIN
    UPDATE PREFERENCIAS_NOTIFICACAO
    SET ATIVO = p_ativo,
        DATA_ATUALIZACAO = SYSTIMESTAMP
    WHERE ID_PREFERENCIA = p_id;
    COMMIT;
END;
/

### 3. Criar Alerta
```sql
CREATE OR REPLACE PROCEDURE SP_CRIAR_ALERTA (
    p_id_preferencia IN NUMBER,
    p_temperatura IN NUMBER,
    p_id_out OUT NUMBER
) AS
BEGIN
    INSERT INTO ALERTA_TEMPERATURA (
        ID_ALERTA,
        ID_PREFERENCIA,
        TEMPERATURA,
        DATA_MEDICAO
    ) VALUES (
        SEQ_ALERTA.NEXTVAL,
        p_id_preferencia,
        p_temperatura,
        SYSTIMESTAMP
    ) RETURNING ID_ALERTA INTO p_id_out;
    COMMIT;
END;
/

## Como Usar as Procedures

### PL/SQL Developer
```sql
-- Inserir Preferência
DECLARE
    v_id NUMBER;
BEGIN
    SP_INSERIR_PREFERENCIA('São Paulo', 'SP', 15, 30, :v_id);
    DBMS_OUTPUT.PUT_LINE('ID gerado: ' || v_id);
END;

-- Atualizar Status
BEGIN
    SP_ATUALIZAR_STATUS_PREF(1, 'N');
END;

-- Criar Alerta
DECLARE
    v_id NUMBER;
BEGIN
    SP_CRIAR_ALERTA(1, 32.5, :v_id);
    DBMS_OUTPUT.PUT_LINE('ID do alerta: ' || v_id);
END;
```

### C# (usando Dapper)
```csharp
// Exemplo de chamada da procedure no C#
var parameters = new DynamicParameters();
parameters.Add("p_cidade", "São Paulo");
parameters.Add("p_estado", "SP");
parameters.Add("p_temp_min", 15);
parameters.Add("p_temp_max", 30);
parameters.Add("p_id_out", dbType: DbType.Int32, direction: ParameterDirection.Output);

await conn.ExecuteAsync(
    "SP_INSERIR_PREFERENCIA",
    parameters,
    commandType: CommandType.StoredProcedure
);

var newId = parameters.Get<int>("p_id_out");
```

## Diagrama de Entidade Relacionamento

```
PREFERENCIAS_NOTIFICACAO
+----------------+-------------+----------------+
| ID_PREFERENCIA | PK         | NUMBER        |
| CIDADE         | NOT NULL   | VARCHAR2(100) |
| ESTADO         | NOT NULL   | VARCHAR2(2)   |
| TEMPERATURA_MIN|            | NUMBER(5,2)   |
| TEMPERATURA_MAX|            | NUMBER(5,2)   |
| ATIVO          |            | CHAR(1)       |
| DATA_CRIACAO   |            | TIMESTAMP     |
| DATA_ATUALIZACAO|           | TIMESTAMP     |
+----------------+-------------+----------------+

ALERTA_TEMPERATURA
+----------------+-------------+----------------+
| ID_ALERTA      | PK         | NUMBER        |
| ID_PREFERENCIA | FK         | NUMBER        |
| TEMPERATURA    | NOT NULL   | NUMBER(5,2)   |
| DATA_MEDICAO   |            | TIMESTAMP     |
| STATUS         |            | VARCHAR2(20)  |
+----------------+-------------+----------------+
```
