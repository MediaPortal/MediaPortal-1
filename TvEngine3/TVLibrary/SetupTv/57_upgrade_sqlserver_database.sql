USE %TvLibrary%
GO

ALTER TABLE channel DROP COLUMN "name"
GO

ALTER TABLE channel DROP COLUMN "freetoair"
GO

UPDATE Version SET versionNumber=57
GO
