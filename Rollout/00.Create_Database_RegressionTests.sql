--CREATE Database RegressionTests

USE master;
GO

IF LifeDW.Support.udfsGetEnvironment() in ('PROD', 'STAGE') 
BEGIN
    IF DB_ID('RegressionTests') IS NULL
		CREATE DATABASE RegressionTests
		ON
		( NAME = RegressionTests_dat,
			FILENAME = 'D:\MSSQL13.MSSQLSERVER\MSSQL\DATA\RegressionTests_dat.mdf',
			SIZE = 10,
			MAXSIZE = 500,
			FILEGROWTH = 5 )
		LOG ON
		( NAME = RegressionTests_log,
			FILENAME = 'E:\MSSQL13.MSSQLSERVER\MSSQL\DATA\RegressionTests_log.ldf',
			SIZE = 5MB,
			MAXSIZE = 50MB,
			FILEGROWTH = 5MB ) ;
END

IF LifeDW.Support.udfsGetEnvironment() not in ('PROD', 'STAGE')
BEGIN
	IF DB_ID('RegressionTests') IS NULL
		CREATE DATABASE RegressionTests
		ON
		( NAME = RegressionTests_dat,
			FILENAME = 'D:\MSSQL13.MSSQLSERVER\MSSQL\DATA\RegressionTests_dat.mdf',
			SIZE = 10,
			MAXSIZE = 500,
			FILEGROWTH = 5 )
		LOG ON
		( NAME = RegressionTests_log,
			FILENAME = 'D:\MSSQL13.MSSQLSERVER\MSSQL\DATA\RegressionTests_log.ldf',
			SIZE = 5MB,
			MAXSIZE = 50MB,
			FILEGROWTH = 5MB ) ;
END
GO


----------------------
USE [RegressionTests]
GO

SET ANSI_NULLS ON


SET QUOTED_IDENTIFIER ON

IF OBJECT_ID('[dbo].[SchemaVersions]') IS NULL
BEGIN

	CREATE TABLE [dbo].[SchemaVersions](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[ScriptName] [nvarchar](255) NOT NULL,
		[Applied] [datetime] NOT NULL,
		[Hash] [varchar](100) NULL,
		[UploadedDateTime] [datetime] NULL,
		[UploadedBy] [varchar](50) NULL
	) ON [PRIMARY]


	ALTER TABLE [dbo].[SchemaVersions] ADD  DEFAULT (getdate()) FOR [UploadedDateTime]
	ALTER TABLE [dbo].[SchemaVersions] ADD  DEFAULT (suser_sname()) FOR [UploadedBy]

END

--------------------------------------

--Grant permissions for Octopus service account for deployments

--Test environments
IF LifeDW.Support.udfsGetEnvironment() not in ('PROD', 'STAGE')
BEGIN
	
	IF NOT EXISTS(SELECT principal_id FROM sys.database_principals WHERE name = 'CHALLENGERAU\msaLifeOctTEST$') 
	BEGIN
		EXEC [sp_grantdbaccess] @loginame = 'CHALLENGERAU\msaLifeOctTEST$', @name_in_db = 'CHALLENGERAU\msaLifeOctTEST$'
		EXEC [sp_addrolemember] @rolename = 'db_owner', @membername = 'CHALLENGERAU\msaLifeOctTEST$'
	END

END


--Staging, Production environment
IF LifeDW.Support.udfsGetEnvironment() in ('STAGE','PROD')
BEGIN
	
	IF NOT EXISTS(SELECT principal_id FROM sys.database_principals WHERE name = 'CHALLENGERAU\msaLifeOctSTG$') 
	BEGIN
		EXEC [sp_grantdbaccess] @loginame = 'CHALLENGERAU\msaLifeOctSTG$', @name_in_db = 'CHALLENGERAU\msaLifeOctSTG$'
		EXEC [sp_addrolemember] @rolename = 'db_owner', @membername = 'CHALLENGERAU\msaLifeOctSTG$'
	END

	IF NOT EXISTS(SELECT principal_id FROM sys.database_principals WHERE name = 'CHALLENGERAU\msaLifeOctPROD$') 
	BEGIN
		EXEC [sp_grantdbaccess] @loginame = 'CHALLENGERAU\msaLifeOctPROD$', @name_in_db = 'CHALLENGERAU\msaLifeOctPROD$'
		EXEC [sp_addrolemember] @rolename = 'db_owner', @membername = 'CHALLENGERAU\msaLifeOctPROD$'
	END

END


---Grant Permissions for saLifeDataUser account for ETL 

--Test environments
IF LifeDW.Support.udfsGetEnvironment() not in ('PROD', 'STAGE')
BEGIN
	
	IF NOT EXISTS(SELECT principal_id FROM sys.database_principals WHERE name = 'CHALLENGERAU\saLifeDataTestUser') 
	BEGIN
		EXEC [sp_grantdbaccess] @loginame = 'CHALLENGERAU\saLifeDataTestUser', @name_in_db = 'CHALLENGERAU\saLifeDataTestUser'
		EXEC [sp_addrolemember] @rolename = 'db_owner', @membername = 'CHALLENGERAU\saLifeDataTestUser'
	END

END



--Staging and Prod environment
IF LifeDW.Support.udfsGetEnvironment() in ('STAGE','PROD')
BEGIN
	
	IF NOT EXISTS(SELECT principal_id FROM sys.database_principals WHERE name = 'CHALLENGERAU\saLifeDataUser') 
	BEGIN
		EXEC [sp_grantdbaccess] @loginame = 'CHALLENGERAU\saLifeDataUser', @name_in_db = 'CHALLENGERAU\saLifeDataUser'
		EXEC [sp_addrolemember] @rolename = 'db_owner', @membername = 'CHALLENGERAU\saLifeDataUser'
	END

END


