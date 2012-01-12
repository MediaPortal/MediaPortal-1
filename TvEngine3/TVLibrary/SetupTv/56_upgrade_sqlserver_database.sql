use %TvLibrary%
GO

DROP INDEX [IX_TuningDetail_idChannel] ON [dbo].[TuningDetail] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_TuningDetail_idChannel] ON [dbo].[TuningDetail] 
(
	[idChannel] ASC
)
INCLUDE ( [idTuning],
[name],
[provider],
[channelType],
[channelNumber],
[frequency],
[countryId],
[isRadio],
[isTv],
[networkId],
[transportId],
[serviceId],
[pmtPid],
[freeToAir],
[modulation],
[polarisation],
[symbolrate],
[diseqc],
[switchingFrequency],
[bandwidth],
[majorChannel],
[minorChannel],
[videoSource],
[tuningSource]) WITH (STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
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
