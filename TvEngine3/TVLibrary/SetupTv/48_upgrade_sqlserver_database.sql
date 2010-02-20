use %TvLibrary%
GO

ALTER TABLE Server
  ADD rtspPort int NOT NULL CONSTRAINT [DF_Server_RtspPort] DEFAULT 554
  
UPDATE Version SET versionNumber=48
GO
