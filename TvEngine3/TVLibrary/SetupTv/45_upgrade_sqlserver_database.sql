use %TvLibrary%
GO

ALTER TABLE Recording
  ALTER COLUMN fileName VARCHAR(260) NOT NULL
GO

CREATE INDEX IDX_Recording_Filename ON Recording(fileName)
GO
CREATE INDEX IDX_Program_Notify ON Program(notify)
GO

UPDATE Version SET versionNumber=45
GO