use master

IF EXISTS (SELECT name FROM sysdatabases WHERE name = N'TvLibrary')
	DROP DATABASE TvLibrary 
GO