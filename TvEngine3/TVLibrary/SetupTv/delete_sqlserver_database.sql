use master
GO

IF EXISTS (SELECT name FROM sysdatabases WHERE name = N'%TvLibrary%') 
BEGIN
    ALTER DATABASE %TvLibrary% set read_only with rollback immediate
    ALTER DATABASE %TvLibrary% set read_write with rollback immediate
	DROP DATABASE %TvLibrary%
END 
