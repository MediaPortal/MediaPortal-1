use %TvLibrary%
GO

ALTER TABLE Recording 
  ADD mediaType int NOT NULL DEFAULT ((1))  
GO

UPDATE Version SET versionNumber=60
GO
