USE [RegressionTests] 

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Change History
-- Description:  
-- Checks and Drops Log Table
-- Date 		Who				JIRA			Change				
-- --------------------------------------------------------------
-- 26/05/2020  Ravi Apparasu	FO-387		initial creation
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[uspCheckAndDropLogTable] 
(
	@TableName VARCHAR(100)

)
WITH EXECUTE AS OWNER

As
Begin	
	
	BEGIN TRY

	
			DECLARE @sql VARCHAR(MAX)

			SET @sql = 'IF OBJECT_ID(''' + @TableName + ''') IS NOT NULL' + 
							' DROP TABLE ' + @TableName
			
			--DROP log table if already exists
			PRINT @sql
			EXECUTE(@sql)

			SELECT SYSTEM_USER 
									
	
	END TRY
	BEGIN CATCH
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY()

		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	END CATCH
End

/*Testing

EXECUTE [dbo].[uspCheckAndDropLogTable]  'dbo.SCD_Pfcholdings_Select_20200331_Month_End_1717'
 
SELECT * FROM dbo.SCD_Pfcholdings_Select_20200331_Month_End_1717

*/






