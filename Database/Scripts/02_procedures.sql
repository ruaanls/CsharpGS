-- Procedures para CRUD de Alertas e Preferências

-- Inserir novo alerta
CREATE OR REPLACE PROCEDURE SP_INSERIR_ALERTA(
    p_cidade IN VARCHAR2,
    p_estado IN VARCHAR2,
    p_temperatura IN NUMBER,
    p_tipo_alerta IN VARCHAR2,
    p_mensagem IN VARCHAR2,
    p_id_alerta OUT NUMBER
)
IS
BEGIN
    INSERT INTO ALERTAS_TEMPERATURA (
        CIDADE,
        ESTADO,
        TEMPERATURA,
        TIPO_ALERTA,
        MENSAGEM,
        DATA_HORA,
        STATUS
    ) VALUES (
        p_cidade,
        p_estado,
        p_temperatura,
        p_tipo_alerta,
        p_mensagem,
        SYSDATE,
        'ATIVO'
    ) RETURNING ID_ALERTA INTO p_id_alerta;
    
    COMMIT;
END;
/

-- Inserir nova preferência
CREATE OR REPLACE PROCEDURE SP_INSERIR_PREFERENCIA(
    p_cidade IN VARCHAR2,
    p_estado IN VARCHAR2,
    p_temperatura_min IN NUMBER,
    p_temperatura_max IN NUMBER,
    p_ativo IN NUMBER,
    p_id_preferencia OUT NUMBER
)
IS
BEGIN
    INSERT INTO PREFERENCIAS_NOTIFICACAO (
        CIDADE,
        ESTADO,
        TEMPERATURA_MIN,
        TEMPERATURA_MAX,
        ATIVO,
        DATA_CRIACAO,
        DATA_ATUALIZACAO
    ) VALUES (
        p_cidade,
        p_estado,
        p_temperatura_min,
        p_temperatura_max,
        CASE WHEN p_ativo = 1 THEN 1 ELSE 0 END,
        SYSDATE,
        SYSDATE
    ) RETURNING ID_PREFERENCIA INTO p_id_preferencia;
    
    COMMIT;
END;
/

-- Atualizar status do alerta
CREATE OR REPLACE PROCEDURE SP_ATUALIZAR_STATUS_ALERTA(
    p_id_alerta IN NUMBER,
    p_status IN VARCHAR2
)
IS
BEGIN
    UPDATE ALERTAS_TEMPERATURA
    SET STATUS = p_status
    WHERE ID_ALERTA = p_id_alerta;
    
    IF SQL%ROWCOUNT = 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Alerta não encontrado');
    END IF;
    
    COMMIT;
END;
/

-- Atualizar preferência
CREATE OR REPLACE PROCEDURE SP_ATUALIZAR_PREFERENCIA(
    p_id_preferencia IN NUMBER,
    p_cidade IN VARCHAR2,
    p_estado IN VARCHAR2,
    p_temperatura_min IN NUMBER,
    p_temperatura_max IN NUMBER,
    p_ativo IN NUMBER
)
IS
BEGIN
    UPDATE PREFERENCIAS_NOTIFICACAO
    SET CIDADE = p_cidade,
        ESTADO = p_estado,
        TEMPERATURA_MIN = p_temperatura_min,
        TEMPERATURA_MAX = p_temperatura_max,
        ATIVO = CASE WHEN p_ativo = 1 THEN 1 ELSE 0 END,
        DATA_ATUALIZACAO = SYSDATE
    WHERE ID_PREFERENCIA = p_id_preferencia;
    
    IF SQL%ROWCOUNT = 0 THEN
        RAISE_APPLICATION_ERROR(-20002, 'Preferência não encontrada');
    END IF;
    
    COMMIT;
END;
/

-- Consultar alertas por cidade/estado
CREATE OR REPLACE PROCEDURE SP_CONSULTAR_ALERTAS(
    p_cidade IN VARCHAR2 DEFAULT NULL,
    p_estado IN VARCHAR2 DEFAULT NULL,
    p_alertas OUT SYS_REFCURSOR
)
IS
BEGIN
    OPEN p_alertas FOR
    SELECT 
        ID_ALERTA,
        CIDADE,
        ESTADO,
        TEMPERATURA,
        TIPO_ALERTA,
        MENSAGEM,
        DATA_HORA,
        STATUS
    FROM ALERTAS_TEMPERATURA
    WHERE (p_cidade IS NULL OR CIDADE = p_cidade)
    AND (p_estado IS NULL OR ESTADO = p_estado)
    ORDER BY DATA_HORA DESC;
END;
/

-- Consultar preferências por cidade/estado
CREATE OR REPLACE PROCEDURE SP_CONSULTAR_PREFERENCIAS(
    p_cidade IN VARCHAR2 DEFAULT NULL,
    p_estado IN VARCHAR2 DEFAULT NULL,
    p_preferencias OUT SYS_REFCURSOR
)
IS
BEGIN
    OPEN p_preferencias FOR
    SELECT 
        ID_PREFERENCIA,
        CIDADE,
        ESTADO,
        TEMPERATURA_MIN,
        TEMPERATURA_MAX,
        ATIVO,
        DATA_CRIACAO,
        DATA_ATUALIZACAO
    FROM PREFERENCIAS_NOTIFICACAO
    WHERE (p_cidade IS NULL OR CIDADE = p_cidade)
    AND (p_estado IS NULL OR ESTADO = p_estado)
    ORDER BY DATA_CRIACAO DESC;
END;
/

-- Buscar alerta por ID
CREATE OR REPLACE PROCEDURE SP_BUSCAR_ALERTA_POR_ID(
    p_id_alerta IN NUMBER,
    p_alerta OUT SYS_REFCURSOR
)
IS
BEGIN
    OPEN p_alerta FOR
    SELECT 
        ID_ALERTA,
        CIDADE,
        ESTADO,
        TEMPERATURA,
        TIPO_ALERTA,
        MENSAGEM,
        DATA_HORA,
        STATUS
    FROM ALERTAS_TEMPERATURA
    WHERE ID_ALERTA = p_id_alerta;
END;
/

-- Buscar preferência por ID
CREATE OR REPLACE PROCEDURE SP_BUSCAR_PREFERENCIA_POR_ID(
    p_id_preferencia IN NUMBER,
    p_preferencia OUT SYS_REFCURSOR
)
IS
BEGIN
    OPEN p_preferencia FOR
    SELECT 
        ID_PREFERENCIA,
        CIDADE,
        ESTADO,
        TEMPERATURA_MIN,
        TEMPERATURA_MAX,
        ATIVO,
        DATA_CRIACAO,
        DATA_ATUALIZACAO
    FROM PREFERENCIAS_NOTIFICACAO
    WHERE ID_PREFERENCIA = p_id_preferencia;
END;
/

-- Excluir preferência
CREATE OR REPLACE PROCEDURE SP_EXCLUIR_PREFERENCIA(
    p_id_preferencia IN NUMBER
)
IS
BEGIN
    DELETE FROM PREFERENCIAS_NOTIFICACAO
    WHERE ID_PREFERENCIA = p_id_preferencia;
    
    IF SQL%ROWCOUNT = 0 THEN
        RAISE_APPLICATION_ERROR(-20003, 'Preferência não encontrada');
    END IF;
    
    COMMIT;
END;
