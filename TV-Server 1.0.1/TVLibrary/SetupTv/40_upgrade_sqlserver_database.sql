use %TvLibrary%
GO

ALTER TABLE Card 
  ADD CAM bit NOT NULL CONSTRAINT DF_Card_CAM  DEFAULT ((0))
GO

UPDATE Version SET versionNumber=40
GO
