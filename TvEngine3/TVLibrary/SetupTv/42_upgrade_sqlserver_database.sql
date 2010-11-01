use %TvLibrary%
GO

ALTER TABLE TuningDetail
  ADD isVCRSignal bit NOT NULL CONSTRAINT DF_TuningDetail_isVCRSignal  DEFAULT ((0))
GO

UPDATE Version SET versionNumber=42
GO
