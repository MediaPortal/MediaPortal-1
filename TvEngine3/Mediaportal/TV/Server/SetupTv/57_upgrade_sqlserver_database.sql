USE %TvLibrary%
GO

DROP INDEX [IX_Channel_IsTV] ON [dbo].[Channel] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_Channel_IsTV] ON [dbo].[Channel] 
(
	[isTv] ASC,
	[sortOrder] ASC
)
INCLUDE ( [idChannel],
[isRadio],
[timesWatched],
[totalTimeWatched],
[grabEpg],
[lastGrabTime],
[visibleInGuide]) WITH (STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

ALTER TABLE Channel DROP COLUMN "name"
GO

ALTER TABLE Channel DROP COLUMN "freetoair"
GO

UPDATE Version SET versionNumber=57
GO
