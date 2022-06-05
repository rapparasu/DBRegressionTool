USE [RegressionTests] 
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Change History
-- Description:  
-- Storedproc to Update a Test Case. Proc will only update those columns specified in the @ColumnsToBeUpdated as a comma separated list. 
-- given the table has many columns you don't need to pass in every value if you are only updating one or two columns. 
-- Date 		Who				JIRA			Change				
-- --------------------------------------------------------------
-- 26/05/2020  Ravi Apparasu	FO-387		initial creation
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[uspTestConfigUpdate] 
(
	@ColumnsToBeUpdated VARCHAR(MAX), --comma separated list of columns to be updated, the rest everything can be left NULL or DEFAULT
	@TestName VARCHAR(100),
	@Tag VARCHAR(100) = NULL,
	@SourceDBObjectName NVARCHAR(MAX) = NULL,
	@SourceDBObjectType VARCHAR(100) = NULL,
	@TargetDBObjectName NVARCHAR(MAX) = NULL,
	@TargetDBObjectType VARCHAR(100) = NULL,
	@Params VARCHAR(2000) = NULL,
	@SourceDBServer VARCHAR(100) = NULL,
	@SourceDB VARCHAR(100) = NULL,
	@TargetDBServer VARCHAR(100) = NULL,
	@TargetDB VARCHAR(100) = NULL,
	@PrimaryKeyColumns VARCHAR(1000) = NULL,
	@SortColumnsWhenNoPrimaryKey VARCHAR(1000) = NULL,
	@ColumnsToIncludeForChecks VARCHAR(MAX) = NULL,
	@ColumnsToAlwaysShowDespiteMatching VARCHAR(MAX) = NULL,
	@ColumnsToExcludeForChecks VARCHAR(MAX) = NULL,
	@DiffColumnsToInject VARCHAR(MAX) = NULL,
	@DiffColumnsTolerance DECIMAL(18,9) = NULL,
	@ColumnOrderSequenceInOutput VARCHAR(MAX) = NULL,
	@ExecutionToleranceInSeconds INT = NULL,
	@ShowMatchingColumnValues BIT  = NULL,
	@IsEnabled BIT  = NULL,
	@ToRecAndSendEmail BIT  = NULL,
	@ITRecipients VARCHAR(100) = NULL,
	@BusinessRecipients VARCHAR(100) = NULL,
	@ColumnsToCheckForBusinessAlerting VARCHAR(MAX) = NULL

)

As
BEGIN	

	
	BEGIN TRY

			--begin the transaction
			BEGIN TRANSACTION

			
			UPDATE dbo.DBTestsConfig
			SET 
			TestName							= CASE WHEN 'TestName' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @TestName ELSE TestName END ,
			Tag									= CASE WHEN 'Tag' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @Tag ELSE Tag END ,
			SourceDBObjectName					= CASE WHEN 'SourceDBObjectName' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @SourceDBObjectName  ELSE SourceDBObjectName  END,
			SourceDBObjectType					= CASE WHEN 'SourceDBObjectType' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @SourceDBObjectType ELSE SourceDBObjectType  END,

			TargetDBObjectName					= CASE WHEN 'TargetDBObjectName' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @TargetDBObjectName ELSE TargetDBObjectName  END,
			TargetDBObjectType					= CASE WHEN 'TargetDBObjectType' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @TargetDBObjectType ELSE TargetDBObjectType  END,
			Params								= CASE WHEN 'Params'			  IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @Params ELSE Params  END,
			SourceDBServer						= CASE WHEN 'SourceDBServer'	  IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @SourceDBServer ELSE SourceDBServer  END,
			SourceDB							= CASE WHEN 'SourceDB'		      IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @SourceDB ELSE SourceDB  END,
			TargetDBServer						= CASE WHEN 'TargetDBServer'     IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @TargetDBServer ELSE TargetDBServer  END,

			TargetDB							= CASE WHEN 'TargetDB' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @TargetDB ELSE TargetDB  END,
			PrimaryKeyColumns					= CASE WHEN 'PrimaryKeyColumns' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @PrimaryKeyColumns ELSE PrimaryKeyColumns  END,
			SortColumnsWhenNoPrimaryKey			= CASE WHEN 'SortColumnsWhenNoPrimaryKey' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @SortColumnsWhenNoPrimaryKey ELSE SortColumnsWhenNoPrimaryKey  END,
			ColumnsToIncludeForChecks			= CASE WHEN 'ColumnsToIncludeForChecks' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @ColumnsToIncludeForChecks ELSE ColumnsToIncludeForChecks  END,
			ColumnsToAlwaysShowDespiteMatching  = CASE WHEN 'ColumnsToAlwaysShowDespiteMatching' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @ColumnsToAlwaysShowDespiteMatching ELSE ColumnsToAlwaysShowDespiteMatching  END,
			ColumnsToExcludeForChecks			= CASE WHEN 'ColumnsToExcludeForChecks' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @ColumnsToExcludeForChecks ELSE ColumnsToExcludeForChecks  END,
			DiffColumnsToInject					= CASE WHEN 'DiffColumnsToInject' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @DiffColumnsToInject ELSE DiffColumnsToInject  END,
			DiffColumnsTolerance				= CASE WHEN 'DiffColumnsTolerance' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @DiffColumnsTolerance ELSE DiffColumnsTolerance  END,
			ColumnOrderSequenceInOutput			= CASE WHEN 'ColumnOrderSequenceInOutput' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @ColumnOrderSequenceInOutput ELSE ColumnOrderSequenceInOutput  END,
			ExecutionToleranceInSeconds			= CASE WHEN 'ExecutionToleranceInSeconds' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @ExecutionToleranceInSeconds ELSE ExecutionToleranceInSeconds  END,
			ShowMatchingColumnValues			= CASE WHEN 'ShowMatchingColumnValues' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @ShowMatchingColumnValues ELSE ShowMatchingColumnValues  END,
			IsEnabled							= CASE WHEN 'IsEnabled' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @IsEnabled ELSE IsEnabled  END,
			ToRecAndSendEmail					= CASE WHEN 'ToRecAndSendEmail' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @ToRecAndSendEmail ELSE ToRecAndSendEmail  END,
			ITRecipients						= CASE WHEN 'ITRecipients' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @ITRecipients ELSE ITRecipients  END,
			BusinessRecipients					= CASE WHEN 'BusinessRecipients' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @BusinessRecipients ELSE BusinessRecipients  END,
			ColumnsToCheckForBusinessAlerting	= CASE WHEN 'ColumnsToCheckForBusinessAlerting' IN (SELECT LTRIM(RTRIM(value)) FROM string_split(@ColumnsToBeUpdated, ',')) THEN @ColumnsToCheckForBusinessAlerting ELSE ColumnsToCheckForBusinessAlerting  END
			WHERE TestName = @TestName
			
			SELECT * FROM dbo.DBTestsConfig WHERE TestName = @TestName
			
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

SELECT * FROM dbo.DbTestsConfig WHERE TestName = 'Test_Rec Month-End Holdings between LM_Common and IDH'

EXECUTE [dbo].[uspTestConfigUpdate] 
@ColumnsToBeUpdated = 'SourceDBServer,TargetDBServer,BusinessRecipients, IsEnabled',
@TestName='Test_Rec Month-End Holdings between LM_Common and IDH',
@SourceDBServer = 'LIFESQLUAT',
@TargetDBServer = 'LIFEDWSQLUAT',
@BusinessRecipients = 'rapparasu@challenger.com.au',
@IsEnabled = 0

EXECUTE [dbo].[uspTestConfigUpdate] 
@ColumnsToBeUpdated = 'Params',
@TestName='Rec Month-End Holdings between LM_Common and IDH',
@Params = '@DynamicParameters,@Por_REF=''*''',

EXECUTE [dbo].[uspTestConfigUpdate] 
@ColumnsToBeUpdated = 'Params,ColumnsToIncludeForChecks',
@TestName='Rec Month-End Holdings between LM_Common and IDH',
@Params = '@DynamicParameters,@Por_REF=''*''',
@ColumnsToIncludeForChecks = 'POR,SEC_SHORT_NAME,VOLUME,DIRTY_VALUE_QC,DIRTY_VALUE_AUD,ISSUE_RATING,LIQUIDITY_SCORE,MATURITY_DATE,Business_Class_Level_3,Business_Class_Level_4,Business_Class_Level_5,STAT_CAP_BCLEV2,STAT_CAP_BCLEV3,STAT_CAP_BCLEV4,STAT_CAP_BCLEV5,GICS,GICSXP,AM_Sector'


 

*/