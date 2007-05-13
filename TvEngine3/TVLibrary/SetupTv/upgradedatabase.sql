USE TvLibrary

--- version 27 ---
GO

CREATE TABLE Keyword(
	idKeyword int IDENTITY(1,1) NOT NULL,
	keywordName varchar(200) NOT NULL,
	rating int NOT NULL,
	autoRecord bit NOT NULL,
	searchIn int NOT NULL,
	CONSTRAINT PK_Keyword PRIMARY KEY  
(
	idKeyword ASC
)
) 
GO

CREATE TABLE Timespan(
	idTimespan int IDENTITY(1,1) NOT NULL,
	startTime datetime NOT NULL,
	endTime datetime NOT NULL,
	dayOfWeek int NOT NULL,
	CONSTRAINT PK_Timespan PRIMARY KEY  
(
	idTimespan ASC
)
) 
GO


CREATE TABLE PersoanlTVGuideMap(
	idPersonalTVGuideMap int IDENTITY(1,1) NOT NULL,
	idKeyword int NOT NULL,
	idProgram int NOT NULL,
	CONSTRAINT PK_PersoanlTVGuideMap PRIMARY KEY  
(
	idPersonalTVGuideMap ASC
)
) 
GO

ALTER TABLE PersoanlTVGuideMap  WITH CHECK ADD  CONSTRAINT FK_PersoanlTVGuideMap_Keyword FOREIGN KEY(idKeyword)
REFERENCES Keyword (idKeyword)
GO

ALTER TABLE PersoanlTVGuideMap  WITH CHECK ADD  CONSTRAINT FK_PersoanlTVGuideMap_Progrm FOREIGN KEY(idProgram)
REFERENCES Program (idProgram)
GO

CREATE TABLE KeywordMap(
	idKeywordMap int IDENTITY(1,1) NOT NULL,
	idKeyword int NOT NULL,
	idChannelGroup int NOT NULL,
	CONSTRAINT PK_KeywordMap PRIMARY KEY  
(
	idKeywordMap ASC
)
) 
GO

ALTER TABLE KeywordMap  WITH CHECK ADD  CONSTRAINT FK_KeywordMap_Keyword FOREIGN KEY(idKeyword)
REFERENCES Keyword (idKeyword)
GO

ALTER TABLE KeywordMap  WITH CHECK ADD  CONSTRAINT FK_KeywordMap_ChannelGroup FOREIGN KEY(idChannelGroup)
REFERENCES ChannelGroup (idGroup)
GO

insert into version(versionNumber) values(27)
GO
