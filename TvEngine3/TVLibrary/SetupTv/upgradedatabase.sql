USE TvLibrary
GO
ALTER TABLE Recording ADD stopTime int NOT NULL DEFAULT  0
GO
delete from version
GO
insert into version(versionNumber) values(24)
GO
