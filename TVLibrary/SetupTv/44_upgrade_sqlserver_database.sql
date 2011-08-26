use %TvLibrary%
GO

ALTER TABLE Program
  ADD episodeName varchar(MAX) NOT NULL CONSTRAINT [DF_Program_EpisodeName] DEFAULT '',
      episodePart varchar(MAX) NOT NULL CONSTRAINT [DF_Program_EpisodePart] DEFAULT ''
GO

UPDATE Version SET versionNumber=44
GO
