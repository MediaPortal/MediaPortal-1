use %TvLibrary%
GO

ALTER TABLE Program
  DROP COLUMN notify 
GO

ALTER TABLE TuningDetail
  DROP COLUMN pcrPid  
GO

ALTER TABLE TuningDetail
  DROP COLUMN videoPid   
GO

ALTER TABLE TuningDetail
  DROP COLUMN audioPid   
GO
UPDATE Version SET versionNumber=56 
GO
