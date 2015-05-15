IF not EXISTS(SELECT * FROM sys.databases WHERE name = '#{DatabaseName}') 
	BEGIN CREATE DATABASE #{DatabaseName}
END
GO

IF NOT EXISTS (SELECT loginname FROM master.dbo.syslogins WHERE name = '#{ServiceUser}')
BEGIN
	CREATE LOGIN [#{ServiceUser}] FROM WINDOWS WITH DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english]
END

USE [#{DatabaseName}]
GO
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = '#{ServiceUser}')
BEGIN
	CREATE USER [#{ServiceUser}] FOR LOGIN [#{ServiceUser}]
END
GO

EXEC sp_addrolemember N'db_datareader', N'#{ServiceUser}'
GO
EXEC sp_addrolemember N'db_datawriter', N'#{ServiceUser}'
GO
EXEC sp_addrolemember N'db_ddladmin', N'#{ServiceUser}'
