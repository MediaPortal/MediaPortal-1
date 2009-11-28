use %TvLibrary%
GO

ALTER TABLE Card 
  ADD NetProvider tinyint NOT NULL CONSTRAINT DF_NetProvider  DEFAULT ((0))
GO

UPDATE Version SET versionNumber=50 
GO
