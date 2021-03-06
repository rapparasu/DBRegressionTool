USE [RegressionTests] 

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Change History
-- Description:  
-- Revokes Pemissions for Log table, this procedure will be invoked once the C# bulk copy operation is complete
-- Date 		Who				JIRA			Change				
-- --------------------------------------------------------------
-- 26/05/2020  Ravi Apparasu	FO-387		initial creation
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[uspRevokePermissionsForLogTable] 
(
	@TableName VARCHAR(100)
	
)
WITH EXECUTE AS OWNER

As
Begin	

	
	BEGIN TRY

			DECLARE @sql VARCHAR(MAX)

			--Revoke INSERT permissions on the log table as the data has already bulk copied by C#
			SET @sql = 'REVOKE INSERT ON ' +  @TableName + ' TO [RegressionUser]'
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

EXECUTE [dbo].[uspRevokePermissionsForLogTable] 'dbo.SCD_Pfcholdings_Select_20200331_Month_End_1717'
 
SELECT * FROM dbo.SCD_Pfcholdings_Select_20200331_Month_End_1717

*/






