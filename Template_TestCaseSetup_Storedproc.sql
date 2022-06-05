USE RegressionTests 
GO

DELETE FROM dbo.DBTestsConfig WHERE Tag = 'Debug_SCD_Bonds_Select'

EXECUTE [dbo].[uspTestConfigInsert] 
@TestName='Debug_SCD_Bonds_Select',
@Tag = 'Debug_SCD_Bonds_Select',
@SourceDBObjectName = 'RiskModels.SCD_Bonds_Select',
@SourceDBObjectType = 'Storedprocedure',
@TargetDBObjectName = 'RiskModels.SCD_Bonds_Select',
@TargetDBObjectType = 'Storedprocedure',
@Params = '@EffectiveDate=''2021-01-22'',@SnapshotType=''DAILY'',@Por_REF=''*''',
@SourceDBServer = 'LIFEDWSQL',
@SourceDB = 'DM_CMF',
@TargetDBServer = 'LIFEDWSQLSUPPORT',
@TargetDB = 'DM_CMF',
@PrimaryKeyColumns = 'POR_CALC_HOLDINGS_REF',
@SortColumnsWhenNoPrimaryKey = '',
@ColumnsToIncludeForChecks = '',
@ColumnsToAlwaysShowDespiteMatching = 'SEC_REF,POR,SEC_SHORT_NAME,LastExecuteDate',
@ColumnsToExcludeForChecks = NULL,
@DiffColumnsToInject = 'VOLUME,DIRTY_VALUE_QC,DIRTY_VALUE_AUD',
@DiffColumnsTolerance = 0.100000000,
@ColumnOrderSequenceInOutput = '',
@ExecutionToleranceInSeconds = 15,
@ShowMatchingColumnValues = 0,
@IsEnabled = 1,
@ToRecAndSendEmail = 1,
@ITRecipients = 'rapparasu@challenger.com.au',
@BusinessRecipients = NULL,
@ColumnsToCheckForBusinessAlerting = ''

/*

SELECT * FROM dbo.DbTestsConfig WHERE Tag = 'Debug_SCD_Bonds_Select'
SELECT * FROM dbo.DbTestsConfig WHERE Tag LIKE 'DV2_%' AND CreatedBy LIKE '%rapparasu%' ORDER BY Id DESC

--UPDATE dbo.DBTestsConfig SET ITRecipients = 'rapparasu@challenger.com.au,bstrobl@challenger.com.au,LifeITServices-TEST@challenger.com.au' WHERE Tag = 'BusinessRecMEHoldingsLMCAndIDH'
--UPDATE dbo.DBTestsConfig SET ITRecipients = 'rapparasu@challenger.com.au,LifeITServices-TEST@challenger.com.au' WHERE Tag = 'BusinessRecMEHoldingsLMCAndIDH'
--UPDATE dbo.DBTestsConfig SET DiffColumnsTolerance = 0.10 WHERE Tag = 'BusinessRecMEHoldingsLMCAndIDH'
*/


/*Testing

SELECT * FROM dbo.DBTestResultsSummary ORDER BY CreatedDateTime DESC

SELECT * FROM dbo.DBTestResultsSummary
WHERE TestConfigId IN
(
	SELECT Id FROM dbo.DBTestsConfig WHERE Tag = 'Debug_SCD_Bonds_Select'
)
ORDER BY Id DESC

--DELETE FROM dbo.DBTestResultsSummary

SELECT * FROM RiskModels.SCD_Bonds_Select_20201031_Month_End_35829

SELECT Reason, COUNT(*) FROM RiskModels.SCD_Bonds_Select_20201031_Month_End_35829 GROUP BY Reason

SELECT * FROM RiskModels.SCD_Bonds_Select_20201031_Month_End_35829
WHERE Reason IN('NoMatch','NewRowInSource','NewRowIntarget')
ORDER BY UnMatchedCount DESC

SELECT * FROM RiskModels.SCD_Bonds_Select_20201031_Month_End_35829
WHERE ISSUER_SHORT_NAME LIKE '%<>%'
ORDER BY UnMatchedCount DESC

SELECT * FROM RiskModels.SCD_Bonds_Select_20201031_Month_End_35829
WHERE ISSUER_SHORT_NAME LIKE '%<>%'
AND SEC_REF = 195534
ORDER BY UnMatchedCount DESC

SELECT * FROM RiskModels.SCD_Bonds_Select_20201031_Month_End_35829
WHERE CAST(DIRTY_VALUE_AUD_DIFF as DECIMAL) > 0.10 OR 
(DIRTY_VALUE_AUD_DIFF IS NULL AND DIRTY_VALUE_AUD <>'')

SELECT * FROM RiskModels.SCD_Bonds_Select_20201031_Month_End_35829
WHERE CAST(DIRTY_VALUE_AUD_DIFF as DECIMAL) = 0.00 AND Reason = 'NOMATCH'
AND DIRTY_VALUE_AUD LIKE '%<>%'


*/

