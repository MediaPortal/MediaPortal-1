use %TvLibrary%
GO

ALTER TABLE Schedule
  ADD series bit NOT NULL CONSTRAINT DF_Schedule_series  DEFAULT ((0))
GO

UPDATE Version SET versionNumber=43
GO
