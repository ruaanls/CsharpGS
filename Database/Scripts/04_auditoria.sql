-- Tabelas de auditoria
CREATE TABLE AUDIT_PREFERENCIAS (
    AUDIT_ID NUMBER PRIMARY KEY,
    ID_PREFERENCIA NUMBER,
    OPERACAO CHAR(1),  -- I=Insert, U=Update, D=Delete
    DADOS_ANTIGOS CLOB,
    DADOS_NOVOS CLOB,
    DATA_OPERACAO TIMESTAMP,
    USUARIO VARCHAR2(100)
);

CREATE TABLE AUDIT_ALERTAS (
    AUDIT_ID NUMBER PRIMARY KEY,
    ID_ALERTA NUMBER,
    OPERACAO CHAR(1),
    DADOS_ANTIGOS CLOB,
    DADOS_NOVOS CLOB,
    DATA_OPERACAO TIMESTAMP,
    USUARIO VARCHAR2(100)
);

CREATE SEQUENCE SEQ_AUDIT_PREF START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_AUDIT_ALERTA START WITH 1 INCREMENT BY 1;

-- Trigger para auditoria de preferências
CREATE OR REPLACE TRIGGER TRG_AUDIT_PREFERENCIAS
AFTER INSERT OR UPDATE OR DELETE ON PREFERENCIAS_NOTIFICACAO
FOR EACH ROW
DECLARE
    v_operacao CHAR(1);
    v_dados_antigos CLOB;
    v_dados_novos CLOB;
BEGIN
    IF INSERTING THEN
        v_operacao := 'I';
        v_dados_antigos := NULL;
    ELSIF UPDATING THEN
        v_operacao := 'U';
    ELSE
        v_operacao := 'D';
        v_dados_novos := NULL;
    END IF;

    IF NOT INSERTING THEN
        v_dados_antigos := '{"id":' || :OLD.ID_PREFERENCIA || 
                          ',"cidade":"' || :OLD.CIDADE || 
                          '","estado":"' || :OLD.ESTADO || 
                          '","tempMin":' || TO_CHAR(:OLD.TEMPERATURA_MIN) ||
                          ',"tempMax":' || TO_CHAR(:OLD.TEMPERATURA_MAX) ||
                          ',"ativo":"' || :OLD.ATIVO || '"}';
    END IF;

    IF NOT DELETING THEN
        v_dados_novos := '{"id":' || :NEW.ID_PREFERENCIA || 
                        ',"cidade":"' || :NEW.CIDADE || 
                        '","estado":"' || :NEW.ESTADO || 
                        ',"tempMin":' || TO_CHAR(:NEW.TEMPERATURA_MIN) ||
                        ',"tempMax":' || TO_CHAR(:NEW.TEMPERATURA_MAX) ||
                        ',"ativo":"' || :NEW.ATIVO || '"}';
    END IF;

    INSERT INTO AUDIT_PREFERENCIAS (
        AUDIT_ID,
        ID_PREFERENCIA,
        OPERACAO,
        DADOS_ANTIGOS,
        DADOS_NOVOS,
        DATA_OPERACAO,
        USUARIO
    ) VALUES (
        SEQ_AUDIT_PREF.NEXTVAL,
        NVL(:NEW.ID_PREFERENCIA, :OLD.ID_PREFERENCIA),
        v_operacao,
        v_dados_antigos,
        v_dados_novos,
        SYSTIMESTAMP,
        SYS_CONTEXT('USERENV', 'SESSION_USER')
    );
END;
/

-- Trigger para auditoria de alertas
CREATE OR REPLACE TRIGGER TRG_AUDIT_ALERTAS
AFTER INSERT OR UPDATE OR DELETE ON ALERTA_TEMPERATURA
FOR EACH ROW
DECLARE
    v_operacao CHAR(1);
    v_dados_antigos CLOB;
    v_dados_novos CLOB;
BEGIN
    IF INSERTING THEN
        v_operacao := 'I';
        v_dados_antigos := NULL;
    ELSIF UPDATING THEN
        v_operacao := 'U';
    ELSE
        v_operacao := 'D';
        v_dados_novos := NULL;
    END IF;

    IF NOT INSERTING THEN
        v_dados_antigos := '{"id":' || :OLD.ID_ALERTA || 
                          ',"temperatura":' || TO_CHAR(:OLD.TEMPERATURA) ||
                          ',"status":"' || :OLD.STATUS || '"}';
    END IF;

    IF NOT DELETING THEN
        v_dados_novos := '{"id":' || :NEW.ID_ALERTA || 
                        ',"temperatura":' || TO_CHAR(:NEW.TEMPERATURA) ||
                        ',"status":"' || :NEW.STATUS || '"}';
    END IF;

    INSERT INTO AUDIT_ALERTAS (
        AUDIT_ID,
        ID_ALERTA,
        OPERACAO,
        DADOS_ANTIGOS,
        DADOS_NOVOS,
        DATA_OPERACAO,
        USUARIO
    ) VALUES (
        SEQ_AUDIT_ALERTA.NEXTVAL,
        NVL(:NEW.ID_ALERTA, :OLD.ID_ALERTA),
        v_operacao,
        v_dados_antigos,
        v_dados_novos,
        SYSTIMESTAMP,
        SYS_CONTEXT('USERENV', 'SESSION_USER')
    );
END;
/
