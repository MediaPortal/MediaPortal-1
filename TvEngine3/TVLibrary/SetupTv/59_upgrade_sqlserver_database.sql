use %TvLibrary%
GO

ALTER TABLE Card ALTER COLUMN stopgraph DEFAULT ((1))
GO

UPDATE Version SET versionNumber=59
GO
