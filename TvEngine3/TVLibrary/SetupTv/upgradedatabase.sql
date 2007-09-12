USE TvLibrary

--- version 33 ---
GO

CREATE TABLE RadioChannelGroup(
	idGroup int IDENTITY(1,1) NOT NULL,
	groupName varchar(200) NOT NULL,
	sortOrder int NOT NULL DEFAULT 0,
 CONSTRAINT PK_RadioChannelGroup PRIMARY KEY CLUSTERED (idGroup ASC))
GO

CREATE NONCLUSTERED INDEX IDX_SortOrder ON RadioChannelGroup
(
	sortOrder ASC
)
INCLUDE (idGroup,groupName)
GO

CREATE TABLE RadioGroupMap(
	idMap int IDENTITY(1,1) NOT NULL,
	idGroup int NOT NULL,
	idChannel int NOT NULL,
	SortOrder int NOT NULL,
 CONSTRAINT PK_RadioGroupMap PRIMARY KEY CLUSTERED 
(
	idMap ASC
))
GO

ALTER TABLE RadioGroupMap  WITH CHECK ADD  CONSTRAINT FK_RadioGroupMap_Channel FOREIGN KEY(idChannel) REFERENCES Channel (idChannel)
GO

ALTER TABLE RadioGroupMap CHECK CONSTRAINT FK_RadioGroupMap_Channel
GO

ALTER TABLE RadioGroupMap  WITH CHECK ADD  CONSTRAINT FK_RadioGroupMap_RadioChannelGroup FOREIGN KEY(idGroup) REFERENCES RadioChannelGroup (idGroup)
GO

ALTER TABLE RadioGroupMap CHECK CONSTRAINT FK_RadioGroupMap_RadioChannelGroup
GO

DELETE FROM Version
GO

insert into Version(versionNumber) values(33)
GO
