use %TvLibrary%
GO

ALTER TABLE CanceledSchedule 
  ADD idChannel int NOT NULL DEFAULT ((0))
GO

UPDATE Version SET versionNumber=51 
GO