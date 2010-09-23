use %TvLibrary%
GO

ALTER TABLE Card 
  ADD stopgraph bit NOT NULL CONSTRAINT DF_Card_stopgraph  DEFAULT ((0))
GO

UPDATE Version SET versionNumber=54
GO
