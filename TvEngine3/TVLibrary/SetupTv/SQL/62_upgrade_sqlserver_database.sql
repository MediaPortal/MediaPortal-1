use %TvLibrary%
GO

ALTER TABLE Card 
  ADD symbolRateMultiplier int NOT NULL CONSTRAINT DF_symbolRateMultiplier DEFAULT ((0))
GO

UPDATE Version SET versionNumber=62
GO
