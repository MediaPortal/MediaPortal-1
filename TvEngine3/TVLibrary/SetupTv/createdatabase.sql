use master

IF EXISTS (SELECT name FROM sysdatabases WHERE name = N'TvLibrary')
	DROP DATABASE TvLibrary 
GO

CREATE DATABASE TvLibrary 
GO

use TvLibrary
GO
--- create table ---
GO

CREATE TABLE Version(
	idVersion int IDENTITY(1,1) NOT NULL,
	versionNumber int NOT NULL,
 CONSTRAINT PK_Versi PRIMARY KEY  
(
	idVersion ASC
)
)
GO

CREATE TABLE Server(
	idServer int IDENTITY(1,1) NOT NULL,
	isMaster bit NOT NULL,
	hostName varchar(256) NOT NULL,
 CONSTRAINT PK_Server PRIMARY KEY  
(
	idServer ASC
)
)
GO

CREATE TABLE Channel(
	idChannel int IDENTITY(1,1) NOT NULL,
	name varchar(200) NOT NULL,
	isRadio bit NOT NULL,
	isTv bit NOT NULL,
	timesWatched int NOT NULL,
	totalTimeWatched datetime NOT NULL,
	grabEpg bit NOT NULL,
	lastGrabTime datetime NOT NULL,
	sortOrder int NOT NULL,
	visibleInGuide bit NOT NULL,
	externalId varchar(200) NOT NULL,
	freetoair bit NOT NULL,
 CONSTRAINT PK_Channels PRIMARY KEY  
(
	idChannel ASC
)
)
GO

CREATE TABLE ChannelGroup(
	idGroup int IDENTITY(1,1) NOT NULL,
	groupName varchar(200) NOT NULL,
 CONSTRAINT PK_ChannelGroup PRIMARY KEY  
(
	idGroup ASC
)
) 
GO

CREATE TABLE Setting(
	idSetting int IDENTITY(1,1) NOT NULL,
	tag varchar(200) NOT NULL,
	value varchar(4096) NOT NULL,
 CONSTRAINT PK_Setting PRIMARY KEY  
(
	idSetting ASC
)
) 
GO

CREATE TABLE Favorite(
	idFavorite int IDENTITY(1,1) NOT NULL,
	idProgram int NOT NULL,
	priority int NOT NULL,
	timesWatched int NOT NULL,
 CONSTRAINT PK_Favorites PRIMARY KEY  
(
	idFavorite ASC
)
) 
GO

CREATE TABLE CanceledSchedule(
	idCanceledSchedule int IDENTITY(1,1) NOT NULL,
	idSchedule int NOT NULL,
	cancelDateTime datetime NOT NULL,
 CONSTRAINT PK_CanceledSchedule PRIMARY KEY  
(
	idCanceledSchedule ASC
)
) 
GO

CREATE TABLE Card(
	idCard int IDENTITY(1,1) NOT NULL,
	devicePath varchar(2000) NOT NULL,
	name varchar(200) NOT NULL,
	priority int NOT NULL,
	grabEPG bit NOT NULL,
	lastEpgGrab datetime NOT NULL,
	recordingFolder varchar(256) NOT NULL,
	idServer int NOT NULL,
	enabled bit NOT NULL,
	camType int NOT NULL,
	timeshiftingFolder varchar(256) NOT NULL,
	recordingFormat int NOT NULL,
	decryptLimit int NOT NULL,
 CONSTRAINT PK_Cards PRIMARY KEY  
(
	idCard ASC
)
) 
GO

CREATE TABLE Recording(
	idRecording int IDENTITY(1,1) NOT NULL,
	idChannel int NOT NULL,
	startTime datetime NOT NULL,
	endTime datetime NOT NULL,
	title varchar(200) NOT NULL,
	description varchar(4000) NOT NULL,
	genre varchar(200) NOT NULL,
	fileName varchar(1024) NOT NULL,
	keepUntil int NOT NULL,
	keepUntilDate datetime NOT NULL,
	timesWatched int NOT NULL,
	idServer int NOT NULL,
	stopTime int NOT NULL,
 CONSTRAINT PK_Recordings PRIMARY KEY  
(
	idRecording ASC
)
) 
GO

CREATE TABLE ChannelMap(
	idChannelMap int IDENTITY(1,1) NOT NULL,
	idChannel int NOT NULL,
	idCard int NOT NULL,
 CONSTRAINT PK_ChannelMap PRIMARY KEY  
(
	idChannelMap ASC
)
) 
GO

CREATE TABLE TuningDetail(
	idTuning int IDENTITY(1,1) NOT NULL,
	idChannel int NOT NULL,
	name varchar(200) NOT NULL,
	provider varchar(200) NOT NULL,
	channelType int NOT NULL,
	channelNumber int NOT NULL,
	frequency int NOT NULL,
	countryId int NOT NULL,
	isRadio bit NOT NULL,
	isTv bit NOT NULL,
	networkId int NOT NULL,
	transportId int NOT NULL,
	serviceId int NOT NULL,
	pmtPid int NOT NULL,
	freeToAir bit NOT NULL,
	modulation int NOT NULL,
	polarisation int NOT NULL,
	symbolrate int NOT NULL,
	diseqc int NOT NULL,
	switchingFrequency int NOT NULL,
	bandwidth int NOT NULL,
	majorChannel int NOT NULL,
	minorChannel int NOT NULL,
	pcrPid int NOT NULL,
	videoSource int NOT NULL,
	tuningSource int NOT NULL,
	videoPid int NOT NULL,
	audioPid int NOT NULL,
	band int NOT NULL,
	satIndex int NOT NULL,
	innerFecRate int NOT NULL,
 CONSTRAINT PK_TuningDetail PRIMARY KEY  
(
	idTuning ASC
)
) 
GO

CREATE TABLE GroupMap(
	idMap int IDENTITY(1,1) NOT NULL,
	idGroup int NOT NULL,
	idChannel int NOT NULL,
	SortOrder int NOT NULL,
 CONSTRAINT PK_GroupMap PRIMARY KEY  
(
	idMap ASC
)
) 
GO

CREATE TABLE Program(
	idProgram int IDENTITY(1,1) NOT NULL,
	idChannel int NOT NULL,
	startTime datetime NOT NULL,
	endTime datetime NOT NULL,
	title varchar(2000) NOT NULL,
	description varchar(4000) NOT NULL,
	seriesNum varchar(200) NOT NULL,
	episodeNum varchar(200) NOT NULL,
	genre varchar(200) NOT NULL,
	originalAirDate datetime NOT NULL,	
	classification varchar(200) NOT NULL,
	starRating int NOT NULL,
	notify bit NOT NULL,
 CONSTRAINT PK_Programs PRIMARY KEY  
(
	idProgram ASC
)
) 
GO

CREATE TABLE Schedule(
	id_Schedule int IDENTITY(1,1) NOT NULL,
	idChannel int NOT NULL,
	scheduleType int NOT NULL,
	programName varchar(256) NOT NULL,
	startTime datetime NOT NULL,
	endTime datetime NOT NULL,
	maxAirings int NOT NULL,
	priority int NOT NULL,
	directory varchar(1024) NOT NULL,
	quality int NOT NULL,
	keepMethod int NOT NULL,
	keepDate datetime NOT NULL,
	preRecordInterval int NOT NULL,
	postRecordInterval int NOT NULL,
	canceled datetime NOT NULL,
	recommendedCard int NOT NULL,
 CONSTRAINT PK_Schedule PRIMARY KEY  
(
	id_Schedule ASC
)
) 

CREATE TABLE TvMovieMapping(
	idMapping int IDENTITY(1,1) NOT NULL,
	idChannel int NOT NULL,
	stationName varchar(200) NOT NULL,
	timeSharingStart varchar(200) NOT NULL,
	timeSharingEnd varchar(200) NOT NULL,
 CONSTRAINT PK_TvMovieMapping PRIMARY KEY  
(
	idMapping ASC
)
) 
GO

CREATE TABLE History(
	idHistory int IDENTITY(1,1) NOT NULL,
	idChannel int NOT NULL,
	startTime datetime NOT NULL,
	endTime datetime NOT NULL,
	title varchar(1000) NOT NULL,
	description varchar(1000) NOT NULL,
	genre varchar(1000) NOT NULL,
	recorded bit NOT NULL,
	watched int NOT NULL,
 CONSTRAINT PK_History PRIMARY KEY  
(
	idHistory ASC
)
) 

GO
ALTER TABLE History  WITH CHECK ADD  CONSTRAINT FK_History_Channel FOREIGN KEY(idChannel)
REFERENCES Channel (idChannel)
GO
ALTER TABLE Favorite  WITH CHECK ADD  CONSTRAINT FK_Favorites_Programs FOREIGN KEY(idProgram)
REFERENCES Program (idProgram)
GO
ALTER TABLE Favorite CHECK CONSTRAINT FK_Favorites_Programs
GO
ALTER TABLE CanceledSchedule  WITH CHECK ADD  CONSTRAINT FK_CanceledSchedule_Schedule FOREIGN KEY(idSchedule)
REFERENCES Schedule (id_Schedule)
GO
ALTER TABLE CanceledSchedule CHECK CONSTRAINT FK_CanceledSchedule_Schedule
GO
ALTER TABLE Card  WITH CHECK ADD  CONSTRAINT FK_Card_Server FOREIGN KEY(idServer)
REFERENCES Server (idServer)
GO
ALTER TABLE Card CHECK CONSTRAINT FK_Card_Server
GO
ALTER TABLE Recording  WITH CHECK ADD  CONSTRAINT FK_Recording_Server FOREIGN KEY(idServer)
REFERENCES Server (idServer)
GO
ALTER TABLE Recording CHECK CONSTRAINT FK_Recording_Server
GO
ALTER TABLE Recording  WITH CHECK ADD  CONSTRAINT FK_Recordings_Channels FOREIGN KEY(idChannel)
REFERENCES Channel (idChannel)
GO
ALTER TABLE Recording CHECK CONSTRAINT FK_Recordings_Channels
GO
ALTER TABLE ChannelMap  WITH CHECK ADD  CONSTRAINT FK_ChannelMap_Cards FOREIGN KEY(idCard)
REFERENCES Card (idCard)
GO
ALTER TABLE ChannelMap CHECK CONSTRAINT FK_ChannelMap_Cards
GO
ALTER TABLE ChannelMap  WITH CHECK ADD  CONSTRAINT FK_ChannelMap_Channels FOREIGN KEY(idChannel)
REFERENCES Channel (idChannel)
GO
ALTER TABLE ChannelMap CHECK CONSTRAINT FK_ChannelMap_Channels
GO
ALTER TABLE TuningDetail  WITH CHECK ADD  CONSTRAINT FK_TuningDetail_Channel FOREIGN KEY(idChannel)
REFERENCES Channel (idChannel)
GO
ALTER TABLE TuningDetail CHECK CONSTRAINT FK_TuningDetail_Channel
GO
ALTER TABLE GroupMap  WITH CHECK ADD  CONSTRAINT FK_GroupMap_Channel FOREIGN KEY(idChannel)
REFERENCES Channel (idChannel)
GO
ALTER TABLE GroupMap CHECK CONSTRAINT FK_GroupMap_Channel
GO
ALTER TABLE GroupMap  WITH CHECK ADD  CONSTRAINT FK_GroupMap_ChannelGroup FOREIGN KEY(idGroup)
REFERENCES ChannelGroup (idGroup)
GO
ALTER TABLE GroupMap CHECK CONSTRAINT FK_GroupMap_ChannelGroup
GO
ALTER TABLE Program  WITH CHECK ADD  CONSTRAINT FK_Programs_Channels FOREIGN KEY(idChannel)
REFERENCES Channel (idChannel)
GO
ALTER TABLE Program CHECK CONSTRAINT FK_Programs_Channels
GO
ALTER TABLE Schedule  WITH CHECK ADD  CONSTRAINT FK_Schedule_Channel FOREIGN KEY(idChannel)
REFERENCES Channel (idChannel)
GO
ALTER TABLE Schedule CHECK CONSTRAINT FK_Schedule_Channel
GO
ALTER TABLE TvMovieMapping  WITH CHECK ADD  CONSTRAINT FK_TvMovieMapping_Channel FOREIGN KEY(idChannel)
REFERENCES Channel (idChannel)
GO
ALTER TABLE TvMovieMapping CHECK CONSTRAINT FK_TvMovieMapping_Channel
GO



---- create indexes -----
GO
CREATE STATISTICS _dta_stat_645577338_4_3 ON Program(endTime, startTime)
GO
CREATE STATISTICS _dta_stat_645577338_2_3 ON Program(idChannel, startTime)
GO

CREATE INDEX _dta_index_Program_7_645577338__K3_1_2_4_5_6_7_8 ON Program 
(
	startTime ASC
)
INCLUDE ( idProgram,
idChannel,
endTime,
title,
description,
genre,
notify)
GO

CREATE STATISTICS _dta_stat_565577053_9 ON Channel(sortOrder)
GO

CREATE INDEX _dta_index_Channel_7_565577053__K4_K9_1_2_3_5_6_7_8_10 ON Channel 
(
	isTv ASC,
	sortOrder ASC
)
INCLUDE ( idChannel,
name,
isRadio,
timesWatched,
totalTimeWatched,
grabEpg,
lastGrabTime,
visibleInGuide)
GO

CREATE INDEX _dta_index_Program_7_645577338__K2_1_3_4_5_6_7_8 ON Program 
(
	idChannel ASC
)
INCLUDE ( idProgram,
startTime,
endTime,
title,
description,
genre,
notify)

GO

CREATE STATISTICS _dta_stat_645577338_4_2_3 ON Program(endTime, idChannel, startTime)
GO

CREATE INDEX _dta_index_Program_7_645577338__K2_K3_K4_1_5_6_7_8 ON Program 
(
	idChannel ASC,
	startTime ASC,
	endTime ASC
)
INCLUDE ( idProgram,
title,
description,
genre,
notify)
GO

CREATE INDEX _dta_index_TuningDetail_7_709577566__K2_1_3_4_5_6_7_8_9_10_11_12_13_14_15_16_17_18_19_20_21_22_23_24_25_26 ON TuningDetail 
(
	idChannel ASC
)
INCLUDE ( idTuning,
name,
provider,
channelType,
channelNumber,
frequency,
countryId,
isRadio,
isTv,
networkId,
transportId,
serviceId,
pmtPid,
freeToAir,
modulation,
polarisation,
symbolrate,
diseqc,
switchingFrequency,
bandwidth,
majorChannel,
minorChannel,
pcrPid,
videoSource,
tuningSource)
GO
--- version 12 ----
GO

CREATE TABLE Conflict(
	idConflict int IDENTITY(1,1) NOT NULL,
	idSchedule int NOT NULL,
	idConflictingSchedule int NOT NULL,
	idChannel int NOT NULL,
	conflictDate datetime NOT NULL,
	idCard int NULL,
	 CONSTRAINT PK_Conflict PRIMARY KEY  
	(
		idConflict ASC
	)
) 
GO

ALTER TABLE Conflict  WITH CHECK ADD  CONSTRAINT FK_Conflict_Channel FOREIGN KEY(idChannel)
REFERENCES Channel (idChannel)
GO
ALTER TABLE Conflict CHECK CONSTRAINT FK_Conflict_Channel
GO
ALTER TABLE Conflict  WITH CHECK ADD  CONSTRAINT FK_Conflict_Schedule FOREIGN KEY(idSchedule)
REFERENCES Schedule (id_Schedule)
GO
ALTER TABLE Conflict CHECK CONSTRAINT FK_Conflict_Schedule
GO
ALTER TABLE Conflict  WITH CHECK ADD  CONSTRAINT FK_Conflict_Schedule1 FOREIGN KEY(idConflictingSchedule)
REFERENCES Schedule (id_Schedule)
GO
ALTER TABLE Conflict CHECK CONSTRAINT FK_Conflict_Schedule1
GO
--- version 12 ----
GO

GO
--- version 15 ----
GO
---- update version -----
GO


CREATE TABLE Satellite(
	idSatellite int IDENTITY(1,1) NOT NULL,
	satelliteName varchar(200) NOT NULL,
	transponderFileName varchar(200) NOT NULL,
 CONSTRAINT PK_Satellite PRIMARY KEY CLUSTERED 
(
	idSatellite ASC
)
)
GO
CREATE TABLE DisEqcMotor(
	idDiSEqCMotor int IDENTITY(1,1) NOT NULL,
	idCard int NOT NULL,
	idSatellite int NOT NULL,
	position int NOT NULL,
 CONSTRAINT PK_DisEqcMotor PRIMARY KEY CLUSTERED 
(
	idDiSEqCMotor ASC
) 
) 
GO
ALTER TABLE DisEqcMotor  WITH CHECK ADD  CONSTRAINT FK_DisEqcMotor_Satellite FOREIGN KEY(idSatellite)
REFERENCES Satellite (idSatellite)
GO
ALTER TABLE DisEqcMotor CHECK CONSTRAINT FK_DisEqcMotor_Satellite
GO
ALTER TABLE DisEqcMotor  WITH CHECK ADD  CONSTRAINT FK_DisEqcMotor_Card FOREIGN KEY(idCard)
REFERENCES Card (idCard)
GO
ALTER TABLE DisEqcMotor CHECK CONSTRAINT FK_DisEqcMotor_Card
GO
--- version 15 ----
GO
--- version 20 ----
GO
CREATE TABLE CardGroup (
	idCardGroup int IDENTITY (1, 1) NOT NULL ,
	name varchar (255) NOT NULL 
) 
GO

CREATE TABLE CardGroupMap (
	idMapping int IDENTITY (1, 1) NOT NULL ,
	idCard int NOT NULL ,
	idCardGroup int NOT NULL 
) 
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

ALTER TABLE CardGroup WITH NOCHECK ADD 
	CONSTRAINT PK_CardGroup PRIMARY KEY  CLUSTERED 
	(
		idCardGroup
	)   
GO

ALTER TABLE CardGroupMap WITH NOCHECK ADD 
	CONSTRAINT PK_CardGroupMap PRIMARY KEY  CLUSTERED 
	(
		idMapping
	)  
GO

ALTER TABLE CardGroupMap ADD 
	CONSTRAINT FK_CardGroupMap_Card FOREIGN KEY 
	(
		idCard
	) REFERENCES Card (
		idCard
	),
	CONSTRAINT FK_CardGroupMap_CardGroup FOREIGN KEY 
	(
		idCardGroup
	) REFERENCES CardGroup (
		idCardGroup
	)
GO
--- version 23 ----
GO

ALTER TABLE ChannelGroup ADD sortOrder int NOT NULL DEFAULT  0
GO

CREATE NONCLUSTERED INDEX IDX_SortOrder ON ChannelGroup
(
	sortOrder ASC
)
INCLUDE ( idGroup, groupName)
GO

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


CREATE TABLE PersonalTVGuideMap(
	idPersonalTVGuideMap int IDENTITY(1,1) NOT NULL,
	idKeyword int NOT NULL,
	idProgram int NOT NULL,
	CONSTRAINT PK_PersonalTVGuideMap PRIMARY KEY  
(
	idPersonalTVGuideMap ASC
)
) 
GO

ALTER TABLE PersonalTVGuideMap  WITH CHECK ADD  CONSTRAINT FK_PersonalTVGuideMap_Keyword FOREIGN KEY(idKeyword)
REFERENCES Keyword (idKeyword)
GO

ALTER TABLE PersonalTVGuideMap  WITH CHECK ADD  CONSTRAINT FK_PersonalTVGuideMap_Progrm FOREIGN KEY(idProgram)
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


delete from version
GO

insert into version(versionNumber) values(28)
GO
