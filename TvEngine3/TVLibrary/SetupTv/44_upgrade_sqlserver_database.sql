use %TvLibrary%
GO

ALTER TABLE Program
  ADD episodeName varchar(MAX) NOT NULL,
  ADD episodePart varchar(MAX) NOT NULL
GO

UPDATE Version SET versionNumber=44
GO
