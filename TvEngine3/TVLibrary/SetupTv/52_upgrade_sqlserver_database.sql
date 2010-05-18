use %TvLibrary%
GO

DELETE FROM ChannelLinkageMap
GO
ALTER TABLE ChannelLinkageMap
  add displayName varchar(200) NOT NULL
GO

UPDATE Version SET versionNumber=52
GO
