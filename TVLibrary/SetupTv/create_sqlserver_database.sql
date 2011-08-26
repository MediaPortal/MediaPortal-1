use master

IF EXISTS (SELECT name FROM sysdatabases WHERE name = N'%TvLibrary%')
BEGIN
    ALTER DATABASE %TvLibrary% set read_only with rollback immediate
    ALTER DATABASE %TvLibrary% set read_write with rollback immediate
	DROP DATABASE %TvLibrary%
END
GO

CREATE DATABASE %TvLibrary% 
GO

use %TvLibrary%
GO


CREATE TABLE ChannelGroup(
	idGroup int IDENTITY(1,1) NOT NULL,
	groupName varchar(200) NOT NULL,
	sortOrder int NOT NULL,
 CONSTRAINT PK_ChannelGroup PRIMARY KEY CLUSTERED 
(
	idGroup ASC
)
)
GO


CREATE NONCLUSTERED INDEX IDX_SortOrder ON ChannelGroup 
(
	sortOrder ASC
)
INCLUDE
(
    idGroup,
    groupName
)
GO


CREATE TABLE Setting(
	idSetting int IDENTITY(1,1) NOT NULL,
	tag varchar(200) NOT NULL,
	value varchar(4096) NOT NULL,
 CONSTRAINT PK_Setting PRIMARY KEY CLUSTERED 
(
	idSetting ASC
)
)
GO

CREATE NONCLUSTERED INDEX IDX_Setting_Tag ON Setting
(
	tag ASC
)
GO

CREATE TABLE TvMovieMapping(
	idMapping int IDENTITY(1,1) NOT NULL,
	idChannel int NOT NULL,
	stationName varchar(200) NOT NULL,
	timeSharingStart varchar(200) NOT NULL,
	timeSharingEnd varchar(200) NOT NULL,
 CONSTRAINT PK_TvMovieMapping PRIMARY KEY CLUSTERED 
(
	idMapping ASC
)
)
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


CREATE TABLE ChannelLinkageMap(
	idMapping int IDENTITY(1,1) NOT NULL,
	idPortalChannel int NOT NULL,
	idLinkedChannel int NOT NULL,
 CONSTRAINT PK_ChannelLinkageMap PRIMARY KEY CLUSTERED 
(
	idMapping ASC
)
)
GO


CREATE TABLE Keyword(
	idKeyword int IDENTITY(1,1) NOT NULL,
	keywordName varchar(200) NOT NULL,
	rating int NOT NULL,
	autoRecord bit NOT NULL,
	searchIn int NOT NULL,
 CONSTRAINT PK_Keyword PRIMARY KEY CLUSTERED 
(
	idKeyword ASC
)
)
GO


CREATE TABLE Timespan(
	idTimespan int IDENTITY(1,1) NOT NULL,
	idKeyword int NOT NULL,
	startTime datetime NOT NULL,
	endTime datetime NOT NULL,
	dayOfWeek int NOT NULL,
 CONSTRAINT PK_Timespan PRIMARY KEY CLUSTERED 
(
	idTimespan ASC
)
)
GO


CREATE TABLE CardGroup(
	idCardGroup int IDENTITY(1,1) NOT NULL,
	name varchar(255) NOT NULL,
 CONSTRAINT PK_CardGroup PRIMARY KEY CLUSTERED 
(
	idCardGroup ASC
)
)
GO


CREATE TABLE RadioChannelGroup(
	idGroup int IDENTITY(1,1) NOT NULL,
	groupName varchar(200) NOT NULL,
	sortOrder int NOT NULL,
 CONSTRAINT PK_RadioChannelGroup PRIMARY KEY CLUSTERED 
(
	idGroup ASC
)
)
GO


CREATE NONCLUSTERED INDEX IDX_SortOrder ON RadioChannelGroup 
(
	sortOrder ASC
)
INCLUDE 
(
    idGroup,
    groupName
)
GO


CREATE TABLE Version(
	idVersion int IDENTITY(1,1) NOT NULL,
	versionNumber int NOT NULL,
 CONSTRAINT PK_Versi PRIMARY KEY CLUSTERED 
(
	idVersion ASC
)
)
GO

INSERT INTO Version (versionNumber) VALUES (38)
GO


CREATE TABLE Server(
	idServer int IDENTITY(1,1) NOT NULL,
	isMaster bit NOT NULL,
	hostName varchar(256) NOT NULL,
 CONSTRAINT PK_Server PRIMARY KEY CLUSTERED 
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
	displayName varchar(200) NOT NULL,
	epgHasGaps bit NOT NULL,
 CONSTRAINT PK_Channels PRIMARY KEY CLUSTERED 
(
	idChannel ASC
)
)
GO

UPDATE Channel SET epgHasGaps=0
GO


CREATE NONCLUSTERED INDEX IX_Channel_IsTV ON Channel 
(
	isTv ASC,
	sortOrder ASC
)
INCLUDE
(
    idChannel,
    name,
    isRadio,
    timesWatched,
    totalTimeWatched,
    grabEpg,
    lastGrabTime,
    visibleInGuide
)
GO


CREATE TABLE KeywordMap(
	idKeywordMap int IDENTITY(1,1) NOT NULL,
	idKeyword int NOT NULL,
	idChannelGroup int NOT NULL,
 CONSTRAINT PK_KeywordMap PRIMARY KEY CLUSTERED 
(
	idKeywordMap ASC
)
)
GO


CREATE TABLE GroupMap(
	idMap int IDENTITY(1,1) NOT NULL,
	idGroup int NOT NULL,
	idChannel int NOT NULL,
	SortOrder int NOT NULL,
 CONSTRAINT PK_GroupMap PRIMARY KEY CLUSTERED 
(
	idMap ASC
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


CREATE TABLE ChannelMap(
	idChannelMap int IDENTITY(1,1) NOT NULL,
	idChannel int NOT NULL,
	idCard int NOT NULL,
	epgOnly bit NOT NULL,
 CONSTRAINT PK_ChannelMap PRIMARY KEY CLUSTERED 
(
	idChannelMap ASC
)
)
GO

UPDATE ChannelMap SET epgOnly=0
GO


CREATE TABLE CardGroupMap(
	idMapping int IDENTITY(1,1) NOT NULL,
	idCard int NOT NULL,
	idCardGroup int NOT NULL,
 CONSTRAINT PK_CardGroupMap PRIMARY KEY CLUSTERED 
(
	idMapping ASC
)
)
GO


CREATE TABLE PersonalTVGuideMap(
	idPersonalTVGuideMap int IDENTITY(1,1) NOT NULL,
	idKeyword int NOT NULL,
	idProgram int NOT NULL,
 CONSTRAINT PK_PersonalTVGuideMap PRIMARY KEY CLUSTERED 
(
	idPersonalTVGuideMap ASC
)
)
GO


CREATE TABLE Favorite(
	idFavorite int IDENTITY(1,1) NOT NULL,
	idProgram int NOT NULL,
	priority int NOT NULL,
	timesWatched int NOT NULL,
 CONSTRAINT PK_Favorites PRIMARY KEY CLUSTERED 
(
	idFavorite ASC
)
)
GO


CREATE TABLE CanceledSchedule(
	idCanceledSchedule int IDENTITY(1,1) NOT NULL,
	idSchedule int NOT NULL,
	cancelDateTime datetime NOT NULL,
 CONSTRAINT PK_CanceledSchedule PRIMARY KEY CLUSTERED 
(
	idCanceledSchedule ASC
)
)
GO


CREATE TABLE Conflict(
	idConflict int IDENTITY(1,1) NOT NULL,
	idSchedule int NOT NULL,
	idConflictingSchedule int NOT NULL,
	idChannel int NOT NULL,
	conflictDate datetime NOT NULL,
	idCard int NULL,
 CONSTRAINT PK_Conflict PRIMARY KEY CLUSTERED 
(
	idConflict ASC
)
)
GO

CREATE TABLE RadioGroupMap(
	idMap int IDENTITY(1,1) NOT NULL,
	idGroup int NOT NULL,
	idChannel int NOT NULL,
	SortOrder int NOT NULL,
 CONSTRAINT PK_RadioGroupMap PRIMARY KEY CLUSTERED 
(
	idMap ASC
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
 CONSTRAINT PK_Cards PRIMARY KEY CLUSTERED 
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
	title varchar(2000) NOT NULL,
	description varchar(8000) NOT NULL,
	genre varchar(200) NOT NULL,
	fileName varchar(1024) NOT NULL,
	keepUntil int NOT NULL,
	keepUntilDate datetime NOT NULL,
	timesWatched int NOT NULL,
	idServer int NOT NULL,
	stopTime int NOT NULL,
 CONSTRAINT PK_Recordings PRIMARY KEY CLUSTERED 
(
	idRecording ASC
)
)
GO


CREATE TABLE Program(
	idProgram int IDENTITY(1,1) NOT NULL,
	idChannel int NOT NULL,
	startTime datetime NOT NULL,
	endTime datetime NOT NULL,
	title varchar(MAX) NOT NULL,
	description varchar(MAX) NOT NULL,
	seriesNum varchar(200) NOT NULL,
	episodeNum varchar(200) NOT NULL,
	genre varchar(200) NOT NULL,
	originalAirDate datetime NOT NULL,
	classification varchar(200) NOT NULL,
	starRating int NOT NULL,
	notify bit NOT NULL,
	parentalRating int NOT NULL,
 CONSTRAINT PK_Programs PRIMARY KEY CLUSTERED 
(
	idProgram ASC
)
)
GO


CREATE UNIQUE INDEX IX_Program_EPG_Lookup ON Program 
(
	idChannel ASC,
	startTime ASC,
	endTime ASC
)
INCLUDE
(
    title,
    description,
    seriesNum,
    episodeNum,
    genre,
    originalAirDate,
    classification,
    starRating,
    notify,
    parentalRating
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
 CONSTRAINT PK_Schedule PRIMARY KEY CLUSTERED 
(
	id_Schedule ASC
)
)
GO

CREATE NONCLUSTERED INDEX IDX_Schedule_ScheduleType ON Schedule
(
	scheduleType ASC
)
INCLUDE
(
    idChannel,
    programName,
    startTime,
    endTime
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
	pilot int NOT NULL,
	rollOff int NOT NULL,
	url varchar(200) NOT NULL,
	bitrate int NOT NULL,
 CONSTRAINT PK_TuningDetail PRIMARY KEY CLUSTERED 
(
	idTuning ASC
)
)
GO


CREATE NONCLUSTERED INDEX IX_TuningDetail_idChannel ON TuningDetail 
(
	idChannel ASC
)
INCLUDE 
( 
    idTuning,
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
    tuningSource
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
 CONSTRAINT PK_History PRIMARY KEY CLUSTERED 
(
	idHistory ASC
)
)
GO
