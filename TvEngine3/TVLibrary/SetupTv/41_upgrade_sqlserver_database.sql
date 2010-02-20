use %TvLibrary%
GO

ALTER TABLE TuningDetail
  ADD audioSource int NOT NULL CONSTRAINT DF_TuningDetail_audioSource  DEFAULT ((1))
GO

UPDATE Version SET versionNumber=41
GO
