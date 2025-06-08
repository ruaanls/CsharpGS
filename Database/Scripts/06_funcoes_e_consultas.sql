    -- Funções para retorno de dados processados
    CREATE OR REPLACE FUNCTION FN_CALCULAR_MEDIA_TEMPERATURA (
        p_cidade IN VARCHAR2,
        p_data_inicio IN TIMESTAMP,
        p_data_fim IN TIMESTAMP
    ) RETURN NUMBER IS
        v_media NUMBER;
    BEGIN
        SELECT AVG(a.TEMPERATURA)
        INTO v_media
        FROM ALERTA_TEMPERATURA a
        INNER JOIN PREFERENCIAS_NOTIFICACAO p ON a.ID_PREFERENCIA = p.ID_PREFERENCIA
        WHERE p.CIDADE = p_cidade
        AND a.DATA_MEDICAO BETWEEN p_data_inicio AND p_data_fim;
        
        RETURN NVL(v_media, 0);
    END;
    /

    CREATE OR REPLACE FUNCTION FN_CONTAR_ALERTAS_POR_ESTADO (
        p_estado IN VARCHAR2,
        p_status IN VARCHAR2 DEFAULT 'ATIVO'
    ) RETURN NUMBER IS
        v_total NUMBER;
    BEGIN
        SELECT COUNT(*)
        INTO v_total
        FROM ALERTA_TEMPERATURA a
        INNER JOIN PREFERENCIAS_NOTIFICACAO p ON a.ID_PREFERENCIA = p.ID_PREFERENCIA
        WHERE p.ESTADO = p_estado
        AND a.STATUS = p_status;
        
        RETURN v_total;
    END;
    /

    -- Bloco anônimo 1: Análise de alertas por cidade com cursor explícito
    DECLARE
        CURSOR c_alertas_cidade IS
            SELECT p.CIDADE, 
                COUNT(*) as total_alertas,
                AVG(a.TEMPERATURA) as media_temperatura
            FROM ALERTA_TEMPERATURA a
            INNER JOIN PREFERENCIAS_NOTIFICACAO p ON a.ID_PREFERENCIA = p.ID_PREFERENCIA
            WHERE a.DATA_MEDICAO >= SYSTIMESTAMP - 7
            GROUP BY p.CIDADE
            HAVING COUNT(*) > 0
            ORDER BY total_alertas DESC;
        
        v_cidade VARCHAR2(100);
        v_total NUMBER;
        v_media NUMBER;
    BEGIN
        OPEN c_alertas_cidade;
        LOOP
            FETCH c_alertas_cidade INTO v_cidade, v_total, v_media;
            EXIT WHEN c_alertas_cidade%NOTFOUND;
            
            DBMS_OUTPUT.PUT_LINE('Cidade: ' || v_cidade || 
                                ' - Total Alertas: ' || v_total || 
                                ' - Média Temperatura: ' || ROUND(v_media, 2));
        END LOOP;
        CLOSE c_alertas_cidade;
    END;
    /

    -- Bloco anônimo 2: Análise de preferências e alertas
    DECLARE
        v_estado VARCHAR2(2) := 'SP';
        v_total_preferencias NUMBER;
        v_total_alertas NUMBER;
    BEGIN
        -- Contagem de preferências ativas
        SELECT COUNT(*)
        INTO v_total_preferencias
        FROM PREFERENCIAS_NOTIFICACAO
        WHERE ESTADO = v_estado
        AND ATIVO = 'S';
        
        -- Contagem de alertas
        SELECT COUNT(*)
        INTO v_total_alertas
        FROM ALERTA_TEMPERATURA a
        INNER JOIN PREFERENCIAS_NOTIFICACAO p ON a.ID_PREFERENCIA = p.ID_PREFERENCIA
        WHERE p.ESTADO = v_estado
        AND a.STATUS = 'ATIVO';
        
        DBMS_OUTPUT.PUT_LINE('Estado: ' || v_estado);
        DBMS_OUTPUT.PUT_LINE('Total de Preferências Ativas: ' || v_total_preferencias);
        DBMS_OUTPUT.PUT_LINE('Total de Alertas Ativos: ' || v_total_alertas);
        
        IF v_total_alertas > v_total_preferencias THEN
            DBMS_OUTPUT.PUT_LINE('ALERTA: Mais alertas que preferências!');
        END IF;
    END;
    /

    -- Consultas SQL Complexas

    -- 1. Análise de temperatura por cidade e estado
    SELECT 
        p.CIDADE,
        p.ESTADO,
        COUNT(*) as total_alertas,
        AVG(a.TEMPERATURA) as media_temperatura,
        MIN(a.TEMPERATURA) as temp_minima,
        MAX(a.TEMPERATURA) as temp_maxima
    FROM ALERTA_TEMPERATURA a
    INNER JOIN PREFERENCIAS_NOTIFICACAO p ON a.ID_PREFERENCIA = p.ID_PREFERENCIA
    WHERE a.DATA_MEDICAO >= SYSTIMESTAMP - 30
    GROUP BY p.CIDADE, p.ESTADO
    HAVING COUNT(*) > 5
    ORDER BY media_temperatura DESC;

    -- 2. Análise de preferências por faixa de temperatura
    SELECT 
        CASE 
            WHEN TEMPERATURA_MIN < 15 THEN 'Frio'
            WHEN TEMPERATURA_MIN BETWEEN 15 AND 25 THEN 'Ameno'
            ELSE 'Quente'
        END as faixa_temperatura,
        COUNT(*) as total_preferencias,
        COUNT(DISTINCT ESTADO) as total_estados
    FROM PREFERENCIAS_NOTIFICACAO
    WHERE ATIVO = 'S'
    GROUP BY 
        CASE 
            WHEN TEMPERATURA_MIN < 15 THEN 'Frio'
            WHEN TEMPERATURA_MIN BETWEEN 15 AND 25 THEN 'Ameno'
            ELSE 'Quente'
        END
    ORDER BY total_preferencias DESC;

    -- 3. Análise de alertas por período do dia
    SELECT 
        TO_CHAR(a.DATA_MEDICAO, 'HH24') as hora,
        COUNT(*) as total_alertas,
        AVG(a.TEMPERATURA) as media_temperatura
    FROM ALERTA_TEMPERATURA a
    WHERE a.DATA_MEDICAO >= SYSTIMESTAMP - 7
    GROUP BY TO_CHAR(a.DATA_MEDICAO, 'HH24')
    HAVING COUNT(*) > 0
    ORDER BY hora;

    -- 4. Análise de preferências com alertas recentes
    SELECT 
        p.*,
        (SELECT COUNT(*) 
        FROM ALERTA_TEMPERATURA a 
        WHERE a.ID_PREFERENCIA = p.ID_PREFERENCIA 
        AND a.DATA_MEDICAO >= SYSTIMESTAMP - 1) as alertas_ultimas_24h
    FROM PREFERENCIAS_NOTIFICACAO p
    WHERE p.ATIVO = 'S'
    AND EXISTS (
        SELECT 1 
        FROM ALERTA_TEMPERATURA a 
        WHERE a.ID_PREFERENCIA = p.ID_PREFERENCIA 
        AND a.DATA_MEDICAO >= SYSTIMESTAMP - 7
    )
    ORDER BY alertas_ultimas_24h DESC;

    -- 5. Análise de tendência de temperatura
    SELECT 
        p.CIDADE,
        p.ESTADO,
        TRUNC(a.DATA_MEDICAO) as data_medicao,
        AVG(a.TEMPERATURA) as media_temperatura,
        LAG(AVG(a.TEMPERATURA)) OVER (PARTITION BY p.CIDADE ORDER BY TRUNC(a.DATA_MEDICAO)) as media_anterior,
        AVG(a.TEMPERATURA) - LAG(AVG(a.TEMPERATURA)) OVER (PARTITION BY p.CIDADE ORDER BY TRUNC(a.DATA_MEDICAO)) as variacao
    FROM ALERTA_TEMPERATURA a
    INNER JOIN PREFERENCIAS_NOTIFICACAO p ON a.ID_PREFERENCIA = p.ID_PREFERENCIA
    WHERE a.DATA_MEDICAO >= SYSTIMESTAMP - 30
    GROUP BY p.CIDADE, p.ESTADO, TRUNC(a.DATA_MEDICAO)
    HAVING COUNT(*) > 0
    ORDER BY p.CIDADE, data_medicao;

    -- Teste de Procedures de Usuário
    DECLARE
        v_id VARCHAR2(36);
    BEGIN
        -- Inserir usuário
        SP_INSERIR_USUARIO(
            p_nome => 'João Silva',
            p_cidade => 'São Paulo',
            p_idade => 30,
            p_username => 'joao.silva',
            p_password => 'senha123',
            p_tipo_usuario => 'ADMIN',
            p_id_out => v_id
        );
        DBMS_OUTPUT.PUT_LINE('Usuário inserido com ID: ' || v_id);
        
        -- Atualizar usuário
        SP_ATUALIZAR_USUARIO(
            p_id => v_id,
            p_cidade => 'Campinas',
            p_idade => 31
        );
        DBMS_OUTPUT.PUT_LINE('Usuário atualizado');
        
        -- Excluir usuário
        SP_EXCLUIR_USUARIO(p_id => v_id);
        DBMS_OUTPUT.PUT_LINE('Usuário excluído');
    END;
    /

    -- Teste de Procedures de Preferência
    DECLARE
        v_id NUMBER;
    BEGIN
        -- Inserir preferência
        SP_INSERIR_PREFERENCIA(
            p_cidade => 'São Paulo',
            p_estado => 'SP',
            p_temp_min => 15.5,
            p_temp_max => 30.0,
            p_id_out => v_id
        );
        DBMS_OUTPUT.PUT_LINE('Preferência inserida com ID: ' || v_id);
        
        -- Atualizar preferência
        SP_ATUALIZAR_PREFERENCIA(
            p_id => v_id,
            p_temp_min => 16.0,
            p_temp_max => 29.0
        );
        DBMS_OUTPUT.PUT_LINE('Preferência atualizada');
        
        -- Excluir preferência
        SP_EXCLUIR_PREFERENCIA(p_id => v_id);
        DBMS_OUTPUT.PUT_LINE('Preferência excluída');
    END;
    /

    -- Teste da função de média de temperatura
    SELECT FN_CALCULAR_MEDIA_TEMPERATURA(
        'São Paulo',
        SYSTIMESTAMP - 7,
        SYSTIMESTAMP
    ) as media_temperatura
    FROM DUAL;

    -- Teste da função de contagem de alertas
    SELECT FN_CONTAR_ALERTAS_POR_ESTADO('SP') as total_alertas
    FROM DUAL; 