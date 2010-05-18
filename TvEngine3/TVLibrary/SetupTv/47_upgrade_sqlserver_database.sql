use %TvLibrary%
GO

ALTER TABLE Recording
  ADD seriesNum varchar(200) NOT NULL CONSTRAINT [DF_Recording_SeriesNum] DEFAULT '',
      episodeNum varchar(200) NOT NULL CONSTRAINT [DF_Recording_EpisodeNum] DEFAULT '',
      episodePart varchar(MAX) NOT NULL CONSTRAINT [DF_Recording_EpisodePart] DEFAULT ''
  
UPDATE Version SET versionNumber=47
GO
