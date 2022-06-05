USE Test
GO 

DROP TABLE IF EXISTS dbo.SEmployee

DROP TABLE IF EXISTS dbo.DEmployee

CREATE TABLE dbo.SEmployee
(
	Id INT IDENTITY(1,1),
	FirstName VARCHAR(100),
	MiddleName VARCHAR(100) NULL,
	LastName VARCHAR(100)	
)
INSERT INTO dbo.SEmployee 
SELECT 'A','B','C'
UNION
SELECT 'D',NULL,'F'
UNION
SELECT 'G', '','H'


CREATE TABLE dbo.DEmployee
(
	Id INT IDENTITY(1,1),
	FirstName VARCHAR(100),
	MiddleName VARCHAR(100) NULL,
	LastName VARCHAR(100)	
)
INSERT INTO dbo.DEmployee 
SELECT 'A','B','C'
UNION
SELECT 'D','','F'
UNION
SELECT 'G', NULL,'H'

SELECT * FROM Test.dbo.SEmployee 
SELECT * FROM Test.dbo.DEmployee 



USE RegressionTests 
GO

EXECUTE [dbo].[uspTestConfigInsert] 
@TestName='TestForNullAndBlanks',
@Tag = 'TestForNullAndBlanks',
@SourceDBObjectName = 'SELECT * FROM dbo.SEmployee',
@SourceDBObjectType = 'QUERY',
@TargetDBObjectName = 'SELECT * FROM dbo.DEmployee',
@TargetDBObjectType = 'QUERY',
@Params = '',
@SourceDBServer = 'LIFEDWSQLDEV',
@SourceDB = 'Test',
@TargetDBServer = 'LIFEDWSQLDEV',
@TargetDB = 'Test',
@PrimaryKeyColumns = 'Id',
@SortColumnsWhenNoPrimaryKey = '',
@ColumnsToIncludeForChecks = '',
@ColumnsToAlwaysShowDespiteMatching = 'FirstName,LastName',
@ColumnsToExcludeForChecks = NULL,
@DiffColumnsToInject = '',
@DiffColumnsTolerance = 0.100000000,
@ColumnOrderSequenceInOutput = '',
@ExecutionToleranceInSeconds = 15,
@ShowMatchingColumnValues = 0,
@IsEnabled = 1,
@ToRecAndSendEmail = 0,
@ITRecipients = 'rapparasu@challenger.com.au',
@BusinessRecipients = NULL,
@ColumnsToCheckForBusinessAlerting = ''

SELECT * FROM dbo.DBTestsConfig WHERE Tag = 'TestForNullAndBlanks'

SELECT * FROM dbo.DBTestResultsSummary
WHERE TestConfigId IN
(
	SELECT Id FROM dbo.DBTestsConfig WHERE Tag = 'TestForNullAndBlanks'
)
ORDER BY Id DESC

SELECT Reason, COUNT(*) FROM dbo.TestForNullAndBlanks_20200928_34340 GROUP BY Reason

SELECT * FROM dbo.TestForNullAndBlanks_20200928_34340
WHERE Reason IN('NoMatch','NewRowInSource','NewRowIntarget')
ORDER BY UnMatchedCount DESC


