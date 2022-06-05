--Check and create Login
USE [master]
GO
IF NOT EXISTS(
	SELECT * FROM master.dbo.syslogins WHERE Name = 'CHALLENGERAU\saLifeTestServiceUse'
	)
BEGIN
	CREATE LOGIN [CHALLENGERAU\saLifeTestServiceUse] FROM WINDOWS WITH DEFAULT_DATABASE=[master]
END


--Check and create User
USE [LM_Common]
GO
IF NOT EXISTS(
	SELECT * FROM dbo.sysusers WHERE Name = 'CHALLENGERAU\saLifeTestServiceUse'
	)
BEGIN
	CREATE USER [CHALLENGERAU\saLifeTestServiceUse] FOR LOGIN [CHALLENGERAU\saLifeTestServiceUse]
END

--Check and Create Database Role
use [LM_Common]
GO
IF NOT EXISTS (
	   SELECT * FROM sys.database_principals WHERE Type = 'R' AND Name = 'LifePortal'
	   )
BEGIN
	CREATE ROLE [LifePortal]
	
END

--Add user to the role
ALTER ROLE [LifePortal] ADD MEMBER [CHALLENGERAU\saLifeTestServiceUse]

GRANT EXECUTE ON [dbo].[SCD_Pfcholdings_select] TO [LifePortal]



