--Check and create Login

/*
--The login already exists

USE [master]
GO

IF NOT EXISTS(
	SELECT * FROM master.dbo.syslogins WHERE Name = 'CHALLENGERAU\saLifeTestServiceUse'
	)
BEGIN
	CREATE LOGIN [CHALLENGERAU\saLifeTestServiceUse] FROM WINDOWS WITH DEFAULT_DATABASE=[master]
END
*/

---------------RegressionTests database----------------------

--Check and create User
USE [RegressionTests]
GO

IF NOT EXISTS (
	   SELECT * FROM sys.database_principals WHERE Type = 'R' AND Name = 'RegressionUser'
	   )
BEGIN
	CREATE ROLE [RegressionUser]
END

--Test environments 
IF LifeDW.Support.udfsGetEnvironment() not in ('STAGE','PROD')
BEGIN

	IF NOT EXISTS(
	SELECT * FROM dbo.sysusers WHERE Name = 'CHALLENGERAU\saLifeTestServiceUse'
	)
	BEGIN
		CREATE USER [CHALLENGERAU\saLifeTestServiceUse] FOR LOGIN [CHALLENGERAU\saLifeTestServiceUse]
	END

	ALTER ROLE [RegressionUser] ADD MEMBER [CHALLENGERAU\saLifeTestServiceUse]

END

--STG AND PROD environments 
IF LifeDW.Support.udfsGetEnvironment() in ('STAGE','PROD')
BEGIN

	IF NOT EXISTS(
	SELECT * FROM dbo.sysusers WHERE Name = 'CHALLENGERAU\saLifeITJobAdmin'
	)
	BEGIN
		CREATE USER [CHALLENGERAU\saLifeITJobAdmin] FOR LOGIN [CHALLENGERAU\saLifeITJobAdmin]
	END

	ALTER ROLE [RegressionUser] ADD MEMBER [CHALLENGERAU\saLifeITJobAdmin]

END

IF NOT EXISTS(
	SELECT * FROM dbo.sysusers WHERE Name = 'CHALLENGERAU\AAG-SYDA-LifeDW-Development-Users'
	)
BEGIN
	CREATE USER [CHALLENGERAU\AAG-SYDA-LifeDW-Development-Users] FOR LOGIN [CHALLENGERAU\AAG-SYDA-LifeDW-Development-Users]
END

IF NOT EXISTS(
SELECT * FROM dbo.sysusers WHERE Name = 'CHALLENGERAU\sg-SYDA-LifeITSupport'
)
BEGIN
	CREATE USER [CHALLENGERAU\sg-SYDA-LifeITSupport] FOR LOGIN [CHALLENGERAU\sg-SYDA-LifeITSupport]
END
	

--Add users to the role

ALTER ROLE [RegressionUser] ADD MEMBER [CHALLENGERAU\AAG-SYDA-LifeDW-Development-Users]
ALTER ROLE [RegressionUser] ADD MEMBER [CHALLENGERAU\sg-SYDA-LifeITSupport]

GRANT EXECUTE ON [dbo].[uspGetDBTestsConfig] TO [RegressionUser]
GRANT EXECUTE ON [dbo].[uspSaveTestMetricsToDB] TO [RegressionUser]
GRANT EXECUTE ON [dbo].[uspCheckAndDropLogTable] TO [RegressionUser]
GRANT EXECUTE ON [dbo].[uspCheckAndCreateLogTable] TO [RegressionUser]
GRANT EXECUTE ON [dbo].[uspRevokePermissionsForLogTable] TO [RegressionUser]

EXEC sp_addrolemember 'db_datareader','RegressionUser';
--EXEC sp_addrolemember 'db_dataWriter','RegressionUser';


----------------------DM_CMF----------------

USE [DM_CMF]
GO

--Test environments 
IF LifeDW.Support.udfsGetEnvironment() not in ('STAGE','PROD')
BEGIN

	IF NOT EXISTS(
	SELECT * FROM dbo.sysusers WHERE Name = 'CHALLENGERAU\saLifeTestServiceUse'
	)
	BEGIN
		CREATE USER [CHALLENGERAU\saLifeTestServiceUse] FOR LOGIN [CHALLENGERAU\saLifeTestServiceUse]
	END

	ALTER ROLE [MarketRiskCMFModels] ADD MEMBER [CHALLENGERAU\saLifeTestServiceUse]

END

--STG AND PROD environments 
IF LifeDW.Support.udfsGetEnvironment() in ('STAGE','PROD')
BEGIN

	IF NOT EXISTS(
	SELECT * FROM dbo.sysusers WHERE Name = 'CHALLENGERAU\saLifeITJobAdmin'
	)
	BEGIN
		CREATE USER [CHALLENGERAU\saLifeITJobAdmin] FOR LOGIN [CHALLENGERAU\saLifeITJobAdmin]
	END

	ALTER ROLE [MarketRiskCMFModels] ADD MEMBER [CHALLENGERAU\saLifeITJobAdmin]

END


-------------DM_Common------------------------

USE [DM_Common]
GO

IF LifeDW.Support.udfsGetEnvironment() not in ('STAGE','PROD')
BEGIN

	IF NOT EXISTS(
	SELECT * FROM dbo.sysusers WHERE Name = 'CHALLENGERAU\saLifeTestServiceUse'
	)
	BEGIN
		CREATE USER [CHALLENGERAU\saLifeTestServiceUse] FOR LOGIN [CHALLENGERAU\saLifeTestServiceUse]
	END

	ALTER ROLE [MarketRiskCMFModels] ADD MEMBER [CHALLENGERAU\saLifeTestServiceUse]

END

--STG AND PROD environments 
IF LifeDW.Support.udfsGetEnvironment() in ('STAGE','PROD')
BEGIN

	IF NOT EXISTS(
	SELECT * FROM dbo.sysusers WHERE Name = 'CHALLENGERAU\saLifeITJobAdmin'
	)
	BEGIN
		CREATE USER [CHALLENGERAU\saLifeITJobAdmin] FOR LOGIN [CHALLENGERAU\saLifeITJobAdmin]
	END

	ALTER ROLE [MarketRiskCMFModels] ADD MEMBER [CHALLENGERAU\saLifeITJobAdmin]

END


------------IDH_CONSOLID-------------

USE [IDH_CONSOLID]
GO

IF LifeDW.Support.udfsGetEnvironment() not in ('STAGE','PROD')
BEGIN

	IF NOT EXISTS(
	SELECT * FROM dbo.sysusers WHERE Name = 'CHALLENGERAU\saLifeTestServiceUse'
	)
	BEGIN
		CREATE USER [CHALLENGERAU\saLifeTestServiceUse] FOR LOGIN [CHALLENGERAU\saLifeTestServiceUse]
	END

	--triggers load balancesheet procs for populating vault tables and does stats update after each ETL
	ALTER ROLE [db_owner] ADD MEMBER [CHALLENGERAU\saLifeTestServiceUse]

END

--STG AND PROD environments 
IF LifeDW.Support.udfsGetEnvironment() in ('STAGE','PROD')
BEGIN

	IF NOT EXISTS(
	SELECT * FROM dbo.sysusers WHERE Name = 'CHALLENGERAU\saLifeITJobAdmin'
	)
	BEGIN
		CREATE USER [CHALLENGERAU\saLifeITJobAdmin] FOR LOGIN [CHALLENGERAU\saLifeITJobAdmin]
	END

	--triggers load balancesheet procs for populating vault tables and does stats update after each ETL
	ALTER ROLE [db_owner] ADD MEMBER [CHALLENGERAU\saLifeITJobAdmin]

END

--LifeDW

USE [LifeDW]
GO

IF LifeDW.Support.udfsGetEnvironment() not in ('STAGE','PROD')
BEGIN

	IF NOT EXISTS(
	SELECT * FROM dbo.sysusers WHERE Name = 'CHALLENGERAU\saLifeTestServiceUse'
	)
	BEGIN
		CREATE USER [CHALLENGERAU\saLifeTestServiceUse] FOR LOGIN [CHALLENGERAU\saLifeTestServiceUse]
	END

	--triggers load balancesheet procs for populating vault tables and does stats update after each ETL
	ALTER ROLE [db_datareader] ADD MEMBER [CHALLENGERAU\saLifeTestServiceUse]
	ALTER ROLE [LifePortal] ADD MEMBER [CHALLENGERAU\saLifeITJobAdmin]

END

--STG AND PROD environments 
IF LifeDW.Support.udfsGetEnvironment() in ('STAGE','PROD')
BEGIN

	IF NOT EXISTS(
	SELECT * FROM dbo.sysusers WHERE Name = 'CHALLENGERAU\saLifeITJobAdmin'
	)
	BEGIN
		CREATE USER [CHALLENGERAU\saLifeITJobAdmin] FOR LOGIN [CHALLENGERAU\saLifeITJobAdmin]
	END

	--triggers load balancesheet procs for populating vault tables and does stats update after each ETL
	ALTER ROLE [db_datareader] ADD MEMBER [CHALLENGERAU\saLifeITJobAdmin]

END

