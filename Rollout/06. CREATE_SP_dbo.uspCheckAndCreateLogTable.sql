USE [RegressionTests] 

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Change History
-- Description:  
-- Checks and creates Log table for test case to store reconciliation errors
-- Date 		Who				JIRA			Change				
-- --------------------------------------------------------------
-- 26/05/2020  Ravi Apparasu	FO-387		initial creation
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[uspCheckAndCreateLogTable] 
(
	@TableName VARCHAR(100),
	@SchemaName VARCHAR(50),
	@CreateTableSQL VARCHAR(MAX)

)
WITH EXECUTE AS OWNER

As
Begin	

	
	BEGIN TRY

			DECLARE @sql VARCHAR(MAX)

			SET @sql = 'IF OBJECT_ID(''' + @TableName + ''') IS NOT NULL' + 
							' DROP TABLE ' + @TableName
			
			--Delete log table if already exists
			PRINT @sql
			EXECUTE(@sql)

			--Create schema if not already exists
			
			SET @sql = 'IF SCHEMA_ID(''' + @SchemaName + ''') IS NULL ' + 
						 ' EXEC (''' + 'CREATE SCHEMA ' + @SchemaName + ''')'

			PRINT @sql
			EXECUTE(@sql)


			--recreate log table from incoming CreateTableSQL parameter value
			PRINT @CreateTableSQL
			EXECUTE(@CreateTableSQL)

			--GRANT INSERT Permissions on this dynamically created log table to RegressionUser DB role so that C# bulk copy can write data to this table
			--permissions will be revoked by the C# through another storeprocedure when the data is written successfully.

			SET @sql = 'GRANT INSERT ON ' +  @TableName + ' TO [RegressionUser]'
			PRINT @sql
			EXECUTE(@sql)

					
	
	END TRY
	BEGIN CATCH
				
	
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY()

		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	END CATCH
End

/*Testing

[dbo].[uspCheckAndCreateLogTable] 'dbo.SCD_Pfcholdings_Select_20200331_Month_End_1717', 'dbo',
'CREATE TABLE dbo.SCD_Pfcholdings_Select_20200331_Month_End_1717
 (
POR_REF VARCHAR(1000), POR VARCHAR(1000), SEC_REF VARCHAR(1000), SEC_SHORT_NAME VARCHAR(1000), QUOTATION_CURRENCY VARCHAR(1000), LEGNO VARCHAR(1000), GICS VARCHAR(1000), HOLKEY_REF VARCHAR(1000), REASON VARCHAR(1000), UNMATCHEDCOUNT VARCHAR(1000), LoadDateTime DATETIME DEFAULT GETDATE(), LoadedBy VARCHAR(200) DEFAULT SYSTEM_USER
)'
 
SELECT * FROM dbo.SCD_Pfcholdings_Select_20200331_Month_End_1717

*/






