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
-- Storedproc to Insert a Test Case
-- Date 		Who				JIRA			Change				
-- --------------------------------------------------------------
-- 26/05/2020  Ravi Apparasu	FO-387		 initial creation
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[uspTestConfigInsert] 
(
	@TestName VARCHAR(100),
	@Tag VARCHAR(100),
	@SourceDBObjectName NVARCHAR(MAX),
	@SourceDBObjectType VARCHAR(100),
	@TargetDBObjectName NVARCHAR(MAX),
	@TargetDBObjectType VARCHAR(100),
	@Params VARCHAR(2000) = NULL,
	@SourceDBServer VARCHAR(100),
	@SourceDB VARCHAR(100),
	@TargetDBServer VARCHAR(100),
	@TargetDB VARCHAR(100),
	@PrimaryKeyColumns VARCHAR(1000) = NULL,
	@SortColumnsWhenNoPrimaryKey VARCHAR(1000) = NULL,
	@ColumnsToIncludeForChecks VARCHAR(MAX) = NULL,
	@ColumnsToAlwaysShowDespiteMatching VARCHAR(MAX) = NULL,
	@ColumnsToExcludeForChecks VARCHAR(MAX) = NULL,
	@DiffColumnsToInject VARCHAR(MAX) = NULL,
	@DiffColumnsTolerance DECIMAL(18,9) = NULL,
	@ColumnOrderSequenceInOutput VARCHAR(MAX) = NULL,
	@ExecutionToleranceInSeconds INT = NULL,
	@ShowMatchingColumnValues BIT,
	@IsEnabled BIT,
	@ToRecAndSendEmail BIT,
	@ITRecipients VARCHAR(100) = NULL,
	@BusinessRecipients VARCHAR(100) = NULL,
	@ColumnsToCheckForBusinessAlerting VARCHAR(MAX) = NULL

)

As
BEGIN	

	
	BEGIN TRY

			--begin the transaction
			BEGIN TRANSACTION

			DELETE FROM dbo.DBTestsConfig WHERE TestName = @TestName 
			
			INSERT INTO [dbo].[DBTestsConfig]
           ([TestName]
           ,[Tag]
           ,[SourceDBObjectName]
           ,[SourceDBObjectType]
           ,[TargetDBObjectName]
           ,[TargetDBObjectType]
           ,[Params]
           ,[SourceDBServer]
           ,[SourceDB]
           ,[TargetDBServer]
           ,[TargetDB]
           ,[PrimaryKeyColumns]
           ,[SortColumnsWhenNoPrimaryKey]
           ,[ColumnsToIncludeForChecks]
           ,[ColumnsToAlwaysShowDespiteMatching]
           ,[ColumnsToExcludeForChecks]
           ,[DiffColumnsToInject]
           ,[DiffColumnsTolerance]
           ,[ColumnOrderSequenceInOutput]
           ,[ExecutionToleranceInSeconds]
           ,[ShowMatchingColumnValues]
           ,[IsEnabled]
           ,[ToRecAndSendEmail]
           ,[ITRecipients]
           ,[BusinessRecipients]
           ,[ColumnsToCheckForBusinessAlerting]
		   )
		   VALUES
		   (
				@TestName
			   ,@Tag
			   ,@SourceDBObjectName
			   ,@SourceDBObjectType
			   ,@TargetDBObjectName
			   ,@TargetDBObjectType
			   ,@Params
			   ,@SourceDBServer
			   ,@SourceDB
			   ,@TargetDBServer
			   ,@TargetDB
			   ,@PrimaryKeyColumns
			   ,@SortColumnsWhenNoPrimaryKey
			   ,@ColumnsToIncludeForChecks
			   ,@ColumnsToAlwaysShowDespiteMatching
			   ,@ColumnsToExcludeForChecks
			   ,@DiffColumnsToInject
			   ,@DiffColumnsTolerance
			   ,@ColumnOrderSequenceInOutput
			   ,@ExecutionToleranceInSeconds
			   ,@ShowMatchingColumnValues
			   ,@IsEnabled
			   ,@ToRecAndSendEmail
			   ,@ITRecipients
			   ,@BusinessRecipients
			   ,@ColumnsToCheckForBusinessAlerting
		   )
		
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


EXECUTE [dbo].[uspTestConfigInsert] 
@TestName='T-1Rec Month-End Holdings between LM_Common and IDH',
@Tag = 'T-1BusinessRecMEHoldingsLMCAndIDH',
@SourceDBObjectName = 'dbo.SCD_Pfcholdings_Select',
@SourceDBObjectType = 'Storedprocedure',
@TargetDBObjectName = 'RiskModels.SCD_Pfcholdings_Select',
@TargetDBObjectType = 'Storedprocedure',
@Params = '@EffectiveDate=''{T-1}'',@SnapshotType=''Month-End''',
@SourceDBServer = 'LIFESQLDEV',
@SourceDB = 'LM_COMMON',
@TargetDBServer = 'LIFEDWSQLDEV',
@TargetDB = 'DM_CMF',
@PrimaryKeyColumns = '',
@SortColumnsWhenNoPrimaryKey = 'SEC_REF, POR_REF, QUOTATION_CURRENCY, HOLKEY_REF, LEGNO',
@ColumnsToIncludeForChecks = 'POR,SEC_SHORT_NAME,VOLUME,DIRTY_VALUE_QC,DIRTY_VALUE_AUD,ISSUE_RATING,LIQUIDITY_SCORE,MATURITY_DATE,Business_Class_Level_3,Business_Class_Level_4,Business_Class_Level_5,STAT_CAP_BCLEV2,STAT_CAP_BCLEV3,STAT_CAP_BCLEV4,STAT_CAP_BCLEV5,GICS,GICSXP,AM_Sector',
@ColumnsToAlwaysShowDespiteMatching = 'POR,SEC_SHORT_NAME',
@ColumnsToExcludeForChecks = NULL,
@DiffColumnsToInject = 'VOLUME,DIRTY_VALUE_QC,DIRTY_VALUE_AUD',
@DiffColumnsTolerance = 0.100000000,
@ColumnOrderSequenceInOutput = 'POR_REF, POR, SEC_REF, SEC_SHORT_NAME, QUOTATION_CURRENCY, ISSUE_RATING, MATURITY_DATE, LIQUIDITY_SCORE, LEGNO, VOLUME, VOLUME_DIFF, DIRTY_VALUE_QC, DIRTY_VALUE_QC_DIFF, DIRTY_VALUE_AUD, DIRTY_VALUE_AUD_DIFF, BUSINESS_CLASS_LEVEL_3, BUSINESS_CLASS_LEVEL_4, BUSINESS_CLASS_LEVEL_5, STAT_CAP_BCLEV2,STAT_CAP_BCLEV3,STAT_CAP_BCLEV4,STAT_CAP_BCLEV5, GICS, GICSXP,AM_Sector, HOLKEY_REF, REASON, UNMATCHEDCOUNT',
@ExecutionToleranceInSeconds = 15,
@ShowMatchingColumnValues = 0,
@IsEnabled = 1,
@ToRecAndSendEmail = 1,
@ITRecipients = 'rapparasu@challenger.com.au,LifeITServices-TEST@challenger.com.au',
@BusinessRecipients = NULL,
@ColumnsToCheckForBusinessAlerting = 'VOLUME,DIRTY_VALUE_QC,DIRTY_VALUE_AUD'

SELECT * FROM dbo.DbTestsConfig
 

*/

