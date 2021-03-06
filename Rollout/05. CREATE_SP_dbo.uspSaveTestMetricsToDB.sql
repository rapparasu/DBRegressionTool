USE [RegressionTests] 
GO
/****** Object:  StoredProcedure [RAW].[uspLoadHubPortfolio]    Script Date: 26/05/2020 10:29:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Change History
-- Description:  
-- Saves Test Metrics to Database
-- Date 		Who				JIRA			Change				
-- --------------------------------------------------------------
-- 26/05/2020  Ravi Apparasu	FO-387		initial creation
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[uspSaveTestMetricsToDB] 
(
	@TestConfigId INT = NULL,
	@SourceExecutionTimeInSeconds INT = NULL,
	@TargetExecutionTimeInSeconds INT = NULL,
	@SourceRowCount INT = NULL,
	@TargetRowCount INT = NULL,
	@SourceColumnCount INT = NULL,
	@TargetColumnCount INT = NULL,
	@UnMatchedCellCount INT = NULL,
	@SourceQuery VARCHAR(MAX) = NULL,
	@TargetQuery VARCHAR(MAX) = NULL,
	@Result BIT = NULL,
	@LogTableName VARCHAR(100) = NULL,
	@LogMessage VARCHAR(MAX) = NULL

)

As
Begin	

	
	BEGIN TRY

			--begin the transaction
			BEGIN TRANSACTION

			--DELETE FROM dbo.DBTestResultsSummary WHERE TestConfigId = @TestConfigId 

			INSERT INTO dbo.DBTestResultsSummary( TestConfigId,SourceExecutionTimeInSeconds,TargetExecutionTimeInSeconds,SourceRowCount,TargetRowCount,SourceColumnCount,TargetColumnCount,UnMatchedCellCount,SourceQuery,TargetQuery,Result,LogTableName,LogMessage) VALUES
			(@TestConfigId,@SourceExecutionTimeInSeconds,@TargetExecutionTimeInSeconds,@SourceRowCount,@TargetRowCount,@SourceColumnCount,@TargetColumnCount,@UnMatchedCellCount,@SourceQuery,@TargetQuery,@Result,@LogTableName,@LogMessage)
						
			
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

 SELECT * FROM RegressionTests.dbo.DBTestResultsSummary WHERE TestConfigId = 9999
 EXECUTE RegressionTests.dbo.uspSaveTestMetricsToDB 9999,1,2,3,4,5,6,7,'AMBIT LMC', 'AMBIT IDH',0,'Test',NULL
 

*/