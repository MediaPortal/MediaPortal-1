USE TvLibrary
GO
ALTER TABLE ChannelGroup ADD sortOrder int NOT NULL DEFAULT  0
GO
CREATE NONCLUSTERED INDEX IDX_SortOrder ON ChannelGroup
(
	sortOrder ASC
)
INCLUDE ( idGroup, groupName)
GO
delete from version
GO
insert into version(versionNumber) values(23)
GO
