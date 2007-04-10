USE TvLibrary
GO
CREATE TABLE ChannelLinkageMap (
	idMapping int IDENTITY(1,1) NOT NULL,
	idPortalChannel int NOT NULL,
	idLinkedChannel int NOT NULL,
 CONSTRAINT PK_ChannelLinkageMap PRIMARY KEY CLUSTERED 
(
	idMapping ASC
)
)
GO
insert into version(versionNumber) values(26)
GO
