
USE RegressionTests
GO

IF OBJECT_ID('dbo.DBTestsConfig') IS NOT NULL
	DROP TABLE dbo.DBTestsConfig

CREATE TABLE dbo.DBTestsConfig
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	TestName VARCHAR(100) NOT NULL UNIQUE,
	Tag VARCHAR(100) NOT NULL,
	SourceDBObjectName NVARCHAR(MAX) NOT NULL,
	SourceDBObjectType VARCHAR(100) NOT NULL,
	TargetDBObjectName NVARCHAR(MAX) NOT NULL,
	TargetDBObjectType VARCHAR(100) NOT NULL,
	Params VARCHAR(2000) NULL,
	SourceDBServer VARCHAR(100) NOT NULL,
	SourceDB VARCHAR(100) NOT NULL,
	TargetDBServer VARCHAR(100) NOT NULL,
	TargetDB VARCHAR(100) NOT NULL,
	PrimaryKeyColumns VARCHAR(1000)  NULL,
	SortColumnsWhenNoPrimaryKey VARCHAR(1000) NULL,
	ColumnsToIncludeForChecks VARCHAR(MAX) NULL,
	--apart from key columns any columns included for checks will not be shown in the output if they match perfectly. we need another column to override this behavior 
	ColumnsToAlwaysShowDespiteMatching VARCHAR(MAX) NULL,
	ColumnsToExcludeForChecks VARCHAR(MAX) NULL,
	DiffColumnsToInject VARCHAR(MAX) NULL,
	DiffColumnsTolerance DECIMAL(18,9) NULL,
	ColumnOrderSequenceInOutput VARCHAR(MAX) NULL,
	ExecutionToleranceInSeconds INT NULL,
	ShowMatchingColumnValues BIT NOT NULL DEFAULT 0,
	IsEnabled BIT NOT NULL DEFAULT 1,
	ToRecAndSendEmail BIT NOT NULL DEFAULT 0,
	ITRecipients VARCHAR(100) NULL,
	BusinessRecipients VARCHAR(100) NULL,
	ColumnsToCheckForBusinessAlerting VARCHAR(MAX) NULL,
	CreatedDateTime DATETIME NOT NULL DEFAULT GETDATE(),
	CreatedBy VARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
	UpdatedDateTime DATETIME NOT NULL DEFAULT GETDATE(),
	UpdatedBy VARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
	
)
GO


--entry for dummy view for testing

---TestCase1

/*
INSERT INTO dbo.DBTestsConfig(SourceDBObjectName, TargetDBObjectName,  DBObjectType, Params, SourceDBServer,SourceDB, TargetDBServer, TargetDB,
PrimaryKeyColumns, ColumnsToIncludeForChecks, ColumnsToExcludeForChecks, ExecutionToleranceInSeconds, IsEnabled)
--SELECT 'dbo.vwInstrument', 'dbo.vwInstrument', 'View', NULL, 'LIFEDWSQLDEV', 'RegressionTests', 'LIFEDWSQLSUPPORT', 'RegressionTests', 'PorRef, InstrumentId', '', 'DirtyValueQC', 2,1 --params is null
--SELECT 'dbo.vwInstrument', 'dbo.vwInstrument', 'View', NULL, 'LIFEDWSQLDEV', 'RegressionTests', 'LIFEDWSQLSUPPORT', 'RegressionTests', 'InstrumentId', 'InstrumentId, PorRef, MaturityDate, DirtyValueQC', 'LegalEntity',2,1 --params is null with columnsToIncludeAndExclude
SELECT 'dbo.vwInstrument', 'dbo.vwInstrument', 'View', NULL, 'LIFEDWSQLDEV', 'RegressionTests', 'LIFEDWSQLSUPPORT', 'RegressionTests', 'InstrumentId', '', 'DirtyValueQC,LegalEntity',2,1 --params is null with columnsToIncludeAndExclude
--SELECT 'dbo.vwInstrument', 'dbo.vwInstrument', 'View', 'InstrumentId=1,MaturityDate=''2020-01-01''', 'LIFEDWSQLDEV', 'RegressionTests', 'LIFEDWSQLSUPPORT', 'RegressionTests', 'InstrumentId', NULL, '', 2,1 --filtered params
--SELECT 'dbo.vwInstrument', 'dbo.vwInstrument', 'View', 'InstrumentId=2', 'LIFEDWSQLDEV', 'RegressionTests', 'LIFEDWSQLSUPPORT', 'RegressionTests', 'InstrumentId', NULL, 'DirtyValueQC', 2,1 --filtered params
*/

---TestCase2

/*
--dataset doesn't have HOLKEY_REF column so need to find columns which are unique
INSERT INTO dbo.DBTestsConfig(SourceDBObjectName, TargetDBObjectName, DBObjectType, Params, SourceDBServer,SourceDB, TargetDBServer, TargetDB,
PrimaryKeyColumns, ColumnsToIncludeForChecks, ColumnsToExcludeForChecks, ExecutionToleranceInSeconds, IsEnabled)
SELECT 'AMBIT.AMBIT_Contracts_Select', 'RiskModels.AMBIT_Contracts_Select', 'Storedprocedure', '@AsAtDate = ''T-1'',@SnapshotType = ''DAILY''', 'LIFESQLDAILY', 'LM_Common', 'LIFEDWSQLSUPPORT', 'DM_CMF', 'CONTRACT_ID', NULL, 'DIRTY_VALUE_QC',  15,1 
--SELECT 'dbo.SCD_Pfcholdings_select', 'RiskModels.SCD_Pfcholdings_select', 'Storedprocedure', '@EffectiveDate = ''T-1'',@snapshotType = ''Month-End''', 'LIFESQLDAILY', 'LM_Common', 'LIFEDWSQLSUPPORT', 'DM_CMF', 'POR_CALC_HOLDINGS_REF', NULL, NULL,  15,1 
*/

--TestCase3

/*
--entry for real use case DM_CIP.dbo.vwTransactions
INSERT INTO dbo.DBTestsConfig(SourceDBObjectName, TargetDBObjectName, DBObjectType, Params, SourceDBServer,SourceDB, TargetDBServer, TargetDB,
PrimaryKeyColumns, ColumnsToIncludeForChecks, ColumnsToExcludeForChecks, ExecutionToleranceInSeconds, IsEnabled)
SELECT 'vwTransactions', 'vwTransactions', 'View', 'TradeDate =''2019-12-13''', 'LIFEDWSQLUAT', 'DM_CIP', 'LIFEDWSQLRO', 'DM_CIP', 'TransRef', NULL, 'EffectiveFromDate,EffectiveToDate',  15,1 
*/

--entry for real use case DM_CMF.[RiskModels].[SCD_Pfcholdings_select]
/*
INSERT INTO dbo.DBTestsConfig(SourceDBObjectName, TargetDBObjectName, DBObjectType, Params, SourceDBServer,SourceDB, TargetDBServer, TargetDB,
PrimaryKeyColumns, ColumnsToIncludeForChecks, ColumnsToExcludeForChecks, ExecutionToleranceInSeconds, IsEnabled)
SELECT '[RiskModels].[SCD_Pfcholdings_select]', '[RiskModels].[SCD_Pfcholdings_select]', 'Storedprocedure', '@EffectiveDate = ''2019-12-31'',@snapshotType = ''Month-End''', 'LIFEDWSQLUAT', 'DM_CMF', 'LIFEDWSQLRO', 'DM_CMF', 'POR_CALC_HOLDINGS_REF', NULL, NULL,  15,1 
--SELECT '[RiskModels].[SCD_Pfcholdings_select]', '[RiskModels].[SCD_Pfcholdings_select]', 'Storedprocedure', '@EffectiveDate = ''T-1'',@snapshotType = ''Month-End''', 'LIFEDWSQLSUPPORT', 'DM_CMF', 'LIFEDWSQL', 'DM_CMF', 'POR_CALC_HOLDINGS_REF', NULL, NULL,  15,1 
*/

--TestCase4 - (deploy the test objects to LIFESQLDEV and LIFEDWSQLSUPPORT)

/*
--dataset doesn't have HOLKEY_REF column so need to find columns which are unique
INSERT INTO dbo.DBTestsConfig(SourceDBObjectName, TargetDBObjectName, DBObjectType, Params, SourceDBServer,SourceDB, TargetDBServer, TargetDB,
PrimaryKeyColumns, ColumnsToIncludeForChecks, ColumnsToExcludeForChecks, ExecutionToleranceInSeconds, IsEnabled)
SELECT 'dbo.SCD_Pfcholdings_select', 'BASE.SCD_Pfcholdings_select_Temporal', 'Storedprocedure', '@EffectiveDate = ''2019-12-31'',@snapshotType = ''Month-End''', 'LIFESQLDEV', 'LM_Common', 'LIFEDWSQLSUPPORT', 'DM_Common', 'POR_REF,QUOTATION_CURRENCY,SEC_REF,HOLKEY_REF,LEGNO', NULL, NULL,  15,1 
--SELECT 'dbo.SCD_Pfcholdings_select', 'RiskModels.SCD_Pfcholdings_select', 'Storedprocedure', '@EffectiveDate = ''T-1'',@snapshotType = ''Month-End''', 'LIFESQLDAILY', 'LM_Common', 'LIFEDWSQLSUPPORT', 'DM_CMF', 'POR_CALC_HOLDINGS_REF', NULL, NULL,  15,1 
*/

--entry for real use case to compare PortfolioHoldings between LM_Common and IDH

/*
--POR_CALC_HOLDINGS_REF won't match between IDH and LM_Common because of intra day run in IDH
INSERT INTO dbo.DBTestsConfig(SourceDBObjectName, TargetDBObjectName, DBObjectType, Params, SourceDBServer,SourceDB, TargetDBServer, TargetDB,
PrimaryKeyColumns, ColumnsToIncludeForChecks, ColumnsToExcludeForChecks, ExecutionToleranceInSeconds, IsEnabled)
SELECT 'SELECT PHS.ValuationDate,PHS.SnapshotType, PHS.PorCalcShortName, PH.SEC_REF, PH.POR_REF, PH.QUOTATION_CURRENCY,
PH.HOLKEY_REF, PH.LEGNO, PH.POR_CALC_HOLDINGS_REF, PH.DIRTY_VALUE_QC 
FROM LifeDW.DIMENSION.PortfolioHoldingSnapshot PHS
INNER JOIN LifeDW.DIMENSION.PortfolioHoldings PH
ON PHS.Id = PH.ID and PH.IsOfficial = 1
WHERE PHS.ValuationDate = ''2019-12-31'' 
AND PHS.SnapshotType = ''DAILY''
AND PHS.PorCalcShortName = ''ELM-BATCH'' ', 

'SELECT CAST(SYS_EffectiveDate as DATE) as ValuationDate, SYS_SnapshotType as SnapshotType, 
POR_CALC_SHORT_NAME as PorCalcShortName, SEC_REF, POR_REF, QUOTATION_CURRENCY, HOLKEY_REF,
LEGNO, POR_CALC_HOLDINGS_REF,DIRTY_VALUE_QC 
FROM LM_Common.SCD.vw_Pfc_Holdings_KeyRatios_Adj
WHERE SYS_EffectiveDate = ''2019-12-31''
AND SYS_SnapshotType = ''DAILY''
AND POR_CALC_SHORT_NAME = ''ELM-BATCH'' ',
'Query', NULL, 'LIFEDWSQLSUPPORT', 'LIFEDW', 'LIFESQLDAILY', 'LM_Common', 'POR_CALC_HOLDINGS_REF', NULL, NULL,  15,1 
*/

--TestCase5

/*
INSERT INTO dbo.DBTestsConfig(SourceDBObjectName, TargetDBObjectName, DBObjectType, Params, SourceDBServer,SourceDB, TargetDBServer, TargetDB,
PrimaryKeyColumns, ColumnsToIncludeForChecks, ColumnsToExcludeForChecks, ExecutionToleranceInSeconds, IsEnabled)
SELECT 'SELECT PHS.ValuationDate,PHS.SnapshotType, PHS.PorCalcShortName, PH.SEC_REF, PH.POR_REF, PH.QUOTATION_CURRENCY,
PH.HOLKEY_REF, PH.LEGNO, PH.POR_CALC_HOLDINGS_REF, PH.DIRTY_VALUE_QC 
FROM LifeDW.DIMENSION.PortfolioHoldingSnapshot PHS
INNER JOIN LifeDW.DIMENSION.PortfolioHoldings PH
ON PHS.Id = PH.ID and PH.IsOfficial = 1
WHERE PHS.ValuationDate = ''2019-12-31'' 
AND PHS.SnapshotType = ''DAILY''
AND PHS.PorCalcShortName = ''ELM-BATCH'' ', 

'SELECT CAST(SYS_EffectiveDate as DATE) as ValuationDate, SYS_SnapshotType as SnapshotType, 
POR_CALC_SHORT_NAME as PorCalcShortName, SEC_REF, POR_REF, QUOTATION_CURRENCY, HOLKEY_REF,
LEGNO, POR_CALC_HOLDINGS_REF,DIRTY_VALUE_QC 
FROM LM_Common.SCD.vw_Pfc_Holdings_KeyRatios_Adj
WHERE SYS_EffectiveDate = ''2019-12-31''
AND SYS_SnapshotType = ''DAILY''
AND POR_CALC_SHORT_NAME = ''ELM-BATCH'' ',
'Query', NULL, 'LIFEDWSQLSUPPORT', 'LIFEDW', 'LIFESQLDAILY', 'LM_Common', 'SEC_REF,POR_REF,QUOTATION_CURRENCY,HOLKEY_REF,LEGNO', NULL, '',  15,1 
*/

--Debug Issues
--entry for real use case DM_CMF.[RiskModels].[SCD_Pfcholdings_select]


--INSERT INTO dbo.DBTestsConfig(SourceDBObjectName, TargetDBObjectName, DBObjectType, Params, SourceDBServer,SourceDB, TargetDBServer, TargetDB,
--PrimaryKeyColumns, ColumnsToIncludeForChecks, ColumnsToExcludeForChecks, DiffColumnsToInject, ExecutionToleranceInSeconds, ShowMatchingColumnValues, IsEnabled)
--SELECT '[RiskModels].[SCD_CurrencyExposure_select]', '[RiskModels].[SCD_CurrencyExposure_select]', 'Storedprocedure', '@EffectiveDate = ''2020-02-20''', 'LIFEDWSQLSTG', 'DM_CMF', 'LIFEDWSQLRO', 'DM_CMF', 'PortfolioRef, Currency', NULL, NULL,  15,1 
--SELECT 'dbo.SCD_Bonds_Select', 'BASE.SCD_Bonds_Select_IsCurrent', 'Storedprocedure', '@EffectiveDate = ''2019-06-30'',@SnapshotType=''Month-End''', 'LIFESQLDAILY', 'LM_COMMON', 'LIFEDWSQLSUPPORT', 'DM_COMMON', 'SEC_REF, POR_REF, QUOTATION_CURRENCY, HOLKEY_REF, LEGNO', NULL, NULL,  15,1 
--UNION
--SELECT 'dbo.SCD_Pfcholdings_Select', 'BASE.SCD_Pfcholdings_Select_Temporal', 'Storedprocedure', '@EffectiveDate = ''2019-06-30'',@SnapshotType=''Month-End''', 'LIFESQLDAILY', 'LM_COMMON', 'LIFEDWSQLSUPPORT', 'DM_COMMON', 'SEC_REF, POR_REF, QUOTATION_CURRENCY, HOLKEY_REF, LEGNO', NULL, NULL, 'ACCRUED_INT_PC, CLEAN_VALUE_AUD, DIRTY_VALUE_AUD', 15,0,1
--UNION
--***duplicate holding in LM_Common possibly a bug
--SELECT 'dbo.SCD_Pfcholdings_Select', 'BASE.SCD_Pfcholdings_Select_Temporal', 'Storedprocedure', '@EffectiveDate = ''2005-05-31'',@SnapshotType=''Month-End''', 'LIFESQLDAILY', 'LM_COMMON', 'LIFEDWSQLSUPPORT', 'DM_COMMON', 'SEC_REF, POR_REF, QUOTATION_CURRENCY, HOLKEY_REF, LEGNO', NULL, NULL, NULL, 15,0,1 
--UNION
--SELECT 'dbo.SCD_Pfcholdings_Select', 'BASE.SCD_Pfcholdings_Select_Temporal', 'Storedprocedure', '@EffectiveDate = ''2019-09-30'',@SnapshotType=''Month-End''', 'LIFESQLDAILY', 'LM_COMMON', 'LIFEDWSQLSUPPORT', 'DM_COMMON', 'SEC_REF, POR_REF, QUOTATION_CURRENCY, HOLKEY_REF, LEGNO', NULL, NULL,  15,1 
--UNION
--SELECT 'dbo.SCD_Pfcholdings_Select', 'BASE.SCD_Pfcholdings_Select_Temporal', 'Storedprocedure', '@EffectiveDate = ''2019-10-31'',@SnapshotType=''Month-End''', 'LIFESQLDAILY', 'LM_COMMON', 'LIFEDWSQLSUPPORT', 'DM_COMMON', 'SEC_REF, POR_REF, QUOTATION_CURRENCY, HOLKEY_REF, LEGNO', NULL, NULL,  15,1 
--UNION
--***duplicate holding in LM_Common possibly a bug
--SELECT 'dbo.SCD_Pfcholdings_Select', 'BASE.SCD_Pfcholdings_Select_Temporal', 'Storedprocedure', '@EffectiveDate = ''2019-11-30'',@SnapshotType=''Month-End''', 'LIFESQLDAILY', 'LM_COMMON', 'LIFEDWSQLSUPPORT', 'DM_COMMON', 'SEC_REF, POR_REF, QUOTATION_CURRENCY, HOLKEY_REF, LEGNO', NULL, NULL, NULL, 15,0,1 
--UNION
--SELECT 'dbo.SCD_Pfcholdings_Select', 'BASE.SCD_Pfcholdings_Select_Temporal', 'Storedprocedure', '@EffectiveDate = ''2019-12-31'',@SnapshotType=''Month-End''', 'LIFESQLDAILY', 'LM_COMMON', 'LIFEDWSQLSUPPORT', 'DM_COMMON', 'SEC_REF, POR_REF, QUOTATION_CURRENCY, HOLKEY_REF, LEGNO', NULL, NULL,  15,1 
--UNION
--SELECT 'dbo.SCD_Pfcholdings_Select', 'BASE.SCD_Pfcholdings_Select_Temporal', 'Storedprocedure', '@EffectiveDate = ''2020-01-31'',@SnapshotType=''Month-End''', 'LIFESQLDAILY', 'LM_COMMON', 'LIFEDWSQLSUPPORT', 'DM_COMMON', 'SEC_REF, POR_REF, QUOTATION_CURRENCY, HOLKEY_REF, LEGNO', NULL, NULL,  15,1 

/*
INSERT INTO dbo.DBTestsConfig(SourceDBObjectName, TargetDBObjectName, DBObjectType, Params, SourceDBServer,SourceDB, TargetDBServer, TargetDB,
PrimaryKeyColumns, ColumnsToIncludeForChecks, ColumnsToExcludeForChecks, DiffColumnsToInject, ExecutionToleranceInSeconds, IsEnabled)
SELECT 'dbo.SCD_Pfcholdings_Select', 'BASE.SCD_Pfcholdings_Select_Temporal', 'Storedprocedure', '@EffectiveDate = ''2019-09-30'',@SnapshotType=''Month-End''', 'LIFESQLDAILY', 'LM_COMMON', 'LIFEDWSQLSUPPORT', 'DM_COMMON', 'SEC_REF, POR_REF, QUOTATION_CURRENCY, HOLKEY_REF, LEGNO', NULL, NULL, 'DIRTY_VALUE_QC,CLEAN_VALUE_AUD,DIRTY_VALUE_AUD',  15,1 
UNION
--SELECT 'dbo.SCD_Pfcholdings_Select', 'BASE.SCD_Pfcholdings_Select_Temporal', 'Storedprocedure', '@EffectiveDate = ''2019-11-30'',@SnapshotType=''Month-End''', 'LIFESQLDAILY', 'LM_COMMON', 'LIFEDWSQLSUPPORT', 'DM_COMMON', 'SEC_REF, POR_REF, QUOTATION_CURRENCY, HOLKEY_REF, LEGNO', NULL, NULL, 'CLEAN_VALUE_AUD,DIRTY_VALUE_AUD',  15,1 
--UNION
SELECT 'dbo.SCD_Pfcholdings_Select', 'BASE.SCD_Pfcholdings_Select_Temporal', 'Storedprocedure', '@EffectiveDate = ''2020-01-31'',@SnapshotType=''Month-End''', 'LIFESQLDAILY', 'LM_COMMON', 'LIFEDWSQLSUPPORT', 'DM_COMMON', 'SEC_REF, POR_REF, QUOTATION_CURRENCY, HOLKEY_REF, LEGNO', NULL, NULL, 'DIRTY_VALUE_QC,CLEAN_VALUE_AUD,DIRTY_VALUE_AUD',  15,1 
*/

SELECT * FROM dbo.DBTestsConfig

/*

SELECT * FROM RegressionTests.dbo.DBTestsConfig
SELECT * FROM RegressionTests.dbo.DBTestResultsSummary

SELECT * FROM RegressionTests.dbo.SCD_Bonds_Select_20190731_Month_End
SELECT Reason, COUNT(*) FROM dbo.SCD_Bonds_Select_20190731_Month_End GROUP BY Reason
SELECT * FROM dbo.SCD_Bonds_Select_20190731_Month_End WHERE Reason = 'NoMatch' ORDER BY UnMatchedCount DESC

SELECT * FROM dbo.SCD_Pfcholdings_Select_20190731_Month_End
SELECT Reason, COUNT(*) FROM dbo.SCD_Pfcholdings_Select_20190731_Month_End GROUP BY Reason
SELECT * FROM dbo.SCD_Pfcholdings_Select_20190731_Month_End WHERE Reason = 'NewRowInSource'
SELECT * FROM dbo.SCD_Pfcholdings_Select_20190731_Month_End WHERE Reason = 'NewRowInTarget'
SELECT * FROM dbo.SCD_Pfcholdings_Select_20190731_Month_End WHERE Reason = 'NoMatch' ORDER BY UnMatchedCount DESC
SELECT * FROM dbo.SCD_Pfcholdings_Select_20190731_Month_End WHERE SEC_REF = 284887

SELECT DIRTY_VALUE_AUD, DIRTY_VALUE_AUD_Diff 
FROM dbo.SCD_Pfcholdings_Select_20190731_Month_End 
WHERE Reason = 'NoMatch' 
AND DIRTY_VALUE_AUD_Diff < 1.0
ORDER BY UnMatchedCount DESC

SELECT DIRTY_VALUE_QC
FROM dbo.SCD_Pfcholdings_Select_20190731_Month_End 
WHERE Reason = 'NoMatch' 
AND CHARINDEX('<>', DIRTY_VALUE_QC) > 0
ORDER BY UnMatchedCount DESC


SELECT DISTINCT PARTY_GROUP_NAME FROM dbo.SCD_Pfcholdings_Select_20190731_Month_End WHERE PARTY_GROUP_NAME LIKE '%<>%'



SELECT * FROM RegressionTests.dbo.vwInstrument
SELECT * FROM LIFEDWSQLSUPPORT.RegressionTests.dbo.vwInstrument

SELECT * FROM RegressionTests.dbo.DBTestResultsSummary

SELECT * FROM RegressionTests.dbo.vwInstrument_20200221
SELECT * FROM AMBIT.AMBIT_Contracts_Select_20200221
SELECT * FROM RegressionTests.dbo.vwTransactions_20200221
SELECT * FROM TestCase_1_20200221
SELECT * FROM TestCase_1_20200221 WHERE Reason = 'NewRowInSource'
SELECT * FROM TestCase_1_20200221 WHERE Reason = 'NewRowInTarget'

SELECT * FROM dbo.SCD_Pfcholdings_select_20200221
SELECT SEC_REF, POR_REF, QUOTATION_CURRENCY, HOLKEY_REF, LEGNO FROM dbo.SCD_Pfcholdings_select_20200221

*/








