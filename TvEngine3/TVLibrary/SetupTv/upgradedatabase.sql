USE TvLibrary

--- version 31 ---
GO

ALTER TABLE Program ADD parentalRating int
GO

UPDATE Program SET parentalRating=0
GO

DELETE FROM version
GO

insert into version(versionNumber) values(31)
GO
