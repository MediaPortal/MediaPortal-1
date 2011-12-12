use %TvLibrary%
GO

ALTER TABLE CanceledSchedule 
  ADD idChannel int NOT NULL CONSTRAINT DF_IdChannel DEFAULT ((0))
GO

UPDATE Version SET versionNumber=51
GO
