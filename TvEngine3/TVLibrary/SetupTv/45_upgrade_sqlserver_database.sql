use %TvLibrary%
GO

ALTER TABLE Recording
  ALTER COLUMN fileName VARCHAR(260) NOT NULL
GO

CREATE INDEX IDX_Recording_Filename ON Recording(fileName)
GO

UPDATE Version SET versionNumber=45
GO