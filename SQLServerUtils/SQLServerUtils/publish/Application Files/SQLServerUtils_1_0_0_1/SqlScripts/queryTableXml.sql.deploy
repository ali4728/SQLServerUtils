SELECT DISTINCT 
        isc.ORDINAL_POSITION '@columnOrder',
		[column].name AS '@columnName',
		isc.DATA_TYPE AS '@typeName', 
		isc.CHARACTER_MAXIMUM_LENGTH '@maxLength',		
		isc.NUMERIC_PRECISION AS '@precision',
		case [column].is_nullable when 1 then 'Yes' when 0 then 'No' end AS '@is_nullable',
		case [column].is_identity when 1 then 'Yes' when 0 then 'No' end AS '@is_identity'
		
	FROM sys.tables [table]
	INNER JOIN sys.columns [column] ON [table].object_id = [column].object_id	
	INNER JOIN sys.schemas sc ON [table].schema_id = sc.schema_id
	INNER JOIN INFORMATION_SCHEMA.COLUMNS isc ON isc.COLUMN_NAME = [column].name
		AND isc.TABLE_NAME = [table].name 
	WHERE [table].name = @objectName
	  AND sc.name = @schemaName
	ORDER BY isc.ORDINAL_POSITION 
	FOR XML PATH('column'), ROOT('table');