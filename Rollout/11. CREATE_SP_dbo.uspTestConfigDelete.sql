USE [RegressionTests] 
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Change History
-- Description:  
-- Storedproc to Delete a tes case
-- Date 		Who				JIRA			Change				
-- --------------------------------------------------------------
-- 26/05/2020  Ravi Apparasu	FO-387		initial creation
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[uspTestConfigDelete] 
(
	@TestName VARCHAR(100)

)

As
BEGIN	

	
	BEGIN TRY

			--begin the transaction
			BEGIN TRANSACTION

			DECLARE @LogMessage NVARCHAR(MAX) = ''
			DECLARE @AffectedRowCount INT
			
			DELETE FROM dbo.DBTestsConfig WHERE TestName = @TestName

			SET @AffectedRowCount = @@ROWCOUNT 

			IF (@AffectedRowCount > 0)
			BEGIN
				SET @LogMessage = 'Test: ' + @TestName + ' Deleted successfully'
				RAISERROR(@LogMessage, 0, 1) WITH NOWAIT
			END
			
			COMMIT
						
	
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK TRANSACTION
		END
	
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY()

		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	END CATCH
End



/*Testing

EXECUTE [dbo].[uspTestConfigDelete] 
@TestName='Test_Rec Month-End Holdings between LM_Common and IDH'

SELECT * FROM dbo.DbTestsConfig WHERE TestName = 'Test_Rec Month-End Holdings between LM_Common and IDH'
 
*/