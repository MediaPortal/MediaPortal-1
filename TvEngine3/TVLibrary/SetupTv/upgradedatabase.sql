USE TvLibrary

--- version 30 ---
GO

ALTER TABLE Channel ADD displayName varchar(200)
GO

UPDATE Channel SET DisplayName=(SELECT name FROM Channel p WHERE Channel.idChannel=p.idChannel)
GO

insert into version(versionNumber) values(30)
GO
