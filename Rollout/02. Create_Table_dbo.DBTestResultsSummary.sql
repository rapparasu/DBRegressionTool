
USE RegressionTests
GO

IF OBJECT_ID('dbo.DBTestResultsSummary') IS NOT NULL
	DROP TABLE dbo.DBTestResultsSummary

CREATE TABLE dbo.DBTestResultsSummary
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	TestConfigId INT NOT NULL,
	SourceExecutionTimeInSeconds INT  NULL,
	TargetExecutionTimeInSeconds INT  NULL,
	SourceRowCount INT  NULL,
	TargetRowCount INT  NULL,
	SourceColumnCount INT  NULL,
	TargetColumnCount INT  NULL,
	UnMatchedCellCount INT  NULL,
	SourceQuery NVARCHAR(MAX) NULL,
	TargetQuery NVARCHAR(MAX) NULL,
	Result BIT NOT NULL,
	LogTableName VARCHAR(100)  NULL,
	LogMessage VARCHAR(MAX) NULL,
	CreatedDateTime DATETIME NOT NULL DEFAULT GETDATE(),
	CreatedBy VARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
	UpdatedDateTime DATETIME NOT NULL DEFAULT GETDATE(),
	UpdatedBy VARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
	
)
GO

SELECT * FROM dbo.DBTestResultsSummary

/*

SELECT * FROM RegressionTests.dbo.DBTestsConfig
SELECT * FROM RegressionTests.dbo.vwInstrument
SELECT * FROM LIFEDWSQLSUPPORT.RegressionTests.dbo.vwInstrument


SELECT * FROM RegressionTests.dbo.DBTestResultsSummary
SELECT * FROM RegressionTests.dbo.vwInstrument_20200212






*/



