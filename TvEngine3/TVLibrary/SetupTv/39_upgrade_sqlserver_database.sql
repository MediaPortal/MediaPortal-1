use %TvLibrary%
GO

ALTER TABLE Channel 
  ADD preload bit NOT NULL CONSTRAINT DF_Card_preload  DEFAULT ((0))
GO

UPDATE Version SET versionNumber=39
GO
