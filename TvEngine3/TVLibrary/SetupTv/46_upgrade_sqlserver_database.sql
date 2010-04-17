use %TvLibrary%
GO

ALTER TABLE Recording
  ADD episodeName varchar(MAX) NOT NULL CONSTRAINT [DF_Recording_EpisodeName] DEFAULT ''
  
UPDATE Version SET versionNumber=46
GO
