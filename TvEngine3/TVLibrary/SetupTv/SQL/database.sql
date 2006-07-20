IF EXISTS (SELECT name FROM master.sysdatabases WHERE name = N'TvLibrary')
	DROP DATABASE TvLibrary
GO

CREATE DATABASE TvLibrary  ON (NAME = N'TvLibrary_Data', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL\data\TvLibrary_Data.MDF' , SIZE = 2, FILEGROWTH = 10%) LOG ON (NAME = N'TvLibrary_Log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL\data\TvLibrary_Log.LDF' , SIZE = 1, FILEGROWTH = 10%)
 COLLATE SQL_Latin1_General_CP1_CI_AS
GO

exec sp_dboption N'TvLibrary', N'autoclose', N'false'
GO

exec sp_dboption N'TvLibrary', N'bulkcopy', N'false'
GO

exec sp_dboption N'TvLibrary', N'trunc. log', N'false'
GO

exec sp_dboption N'TvLibrary', N'torn page detection', N'true'
GO

exec sp_dboption N'TvLibrary', N'read only', N'false'
GO

exec sp_dboption N'TvLibrary', N'dbo use', N'false'
GO

exec sp_dboption N'TvLibrary', N'single', N'false'
GO

exec sp_dboption N'TvLibrary', N'autoshrink', N'false'
GO

exec sp_dboption N'TvLibrary', N'ANSI null default', N'false'
GO

exec sp_dboption N'TvLibrary', N'recursive triggers', N'false'
GO

exec sp_dboption N'TvLibrary', N'ANSI nulls', N'false'
GO

exec sp_dboption N'TvLibrary', N'concat null yields null', N'false'
GO

exec sp_dboption N'TvLibrary', N'cursor close on commit', N'false'
GO

exec sp_dboption N'TvLibrary', N'default to local cursor', N'false'
GO

exec sp_dboption N'TvLibrary', N'quoted identifier', N'false'
GO

exec sp_dboption N'TvLibrary', N'ANSI warnings', N'false'
GO

exec sp_dboption N'TvLibrary', N'auto create statistics', N'true'
GO

exec sp_dboption N'TvLibrary', N'auto update statistics', N'true'
GO

if( (@@microsoftversion / power(2, 24) = 8) and (@@microsoftversion & 0xffff >= 724) )
	exec sp_dboption N'TvLibrary', N'db chaining', N'false'
GO

use TvLibrary
GO

if exists (select * from sysobjects where id = object_id(N'FK_ChannelMap_Cards') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE ChannelMap DROP CONSTRAINT FK_ChannelMap_Cards
GO

if exists (select * from sysobjects where id = object_id(N'FK_ChannelMap_Channels') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE ChannelMap DROP CONSTRAINT FK_ChannelMap_Channels
GO

if exists (select * from sysobjects where id = object_id(N'FK_GroupMap_Channel') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE GroupMap DROP CONSTRAINT FK_GroupMap_Channel
GO

if exists (select * from sysobjects where id = object_id(N'FK_Programs_Channels') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE Program DROP CONSTRAINT FK_Programs_Channels
GO

if exists (select * from sysobjects where id = object_id(N'FK_Recordings_Channels') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE Recording DROP CONSTRAINT FK_Recordings_Channels
GO

if exists (select * from sysobjects where id = object_id(N'FK_TuningDetail_Channel') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE TuningDetail DROP CONSTRAINT FK_TuningDetail_Channel
GO

if exists (select * from sysobjects where id = object_id(N'FK_GroupMap_ChannelGroup') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE GroupMap DROP CONSTRAINT FK_GroupMap_ChannelGroup
GO

if exists (select * from sysobjects where id = object_id(N'FK_Airings_Programs') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE Airing DROP CONSTRAINT FK_Airings_Programs
GO

if exists (select * from sysobjects where id = object_id(N'FK_Favorites_Programs') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE Favorite DROP CONSTRAINT FK_Favorites_Programs
GO

if exists (select * from sysobjects where id = object_id(N'FK_Schedules_Programs') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE Schedule DROP CONSTRAINT FK_Schedules_Programs
GO

if exists (select * from sysobjects where id = object_id(N'Airing') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table Airing
GO

if exists (select * from sysobjects where id = object_id(N'Card') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table Card
GO

if exists (select * from sysobjects where id = object_id(N'Channel') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table Channel
GO

if exists (select * from sysobjects where id = object_id(N'ChannelGroup') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table ChannelGroup
GO

if exists (select * from sysobjects where id = object_id(N'ChannelMap') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table ChannelMap
GO

if exists (select * from sysobjects where id = object_id(N'Favorite') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table Favorite
GO

if exists (select * from sysobjects where id = object_id(N'GroupMap') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table GroupMap
GO

if exists (select * from sysobjects where id = object_id(N'Program') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table Program
GO

if exists (select * from sysobjects where id = object_id(N'Recording') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table Recording
GO

if exists (select * from sysobjects where id = object_id(N'Schedule') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table Schedule
GO

if exists (select * from sysobjects where id = object_id(N'TuningDetail') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table TuningDetail
GO

CREATE TABLE Airing (
	idAiring int IDENTITY (1, 1) NOT NULL ,
	idProgram int NOT NULL ,
	startTime datetime NOT NULL ,
	endTime datetime NOT NULL ,
	title varchar (200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	description varchar (8000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	genre varchar (200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) 
GO

CREATE TABLE Card (
	idCard int IDENTITY (1, 1) NOT NULL ,
	devicePath varchar (2000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	name varchar (200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	priority int NOT NULL ,
	grabEPG bit NOT NULL ,
	lastEpgGrab datetime NOT NULL 
) 
GO

CREATE TABLE Channel (
	idChannel int IDENTITY (1, 1) NOT NULL ,
	name varchar (200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	isRadio bit NOT NULL ,
	isTv bit NOT NULL ,
	timesWatched int NOT NULL ,
	totalTimeWatched datetime NOT NULL ,
	grabEpg bit NOT NULL ,
	lastGrabTime datetime NOT NULL ,
	sortOrder int NOT NULL 
) 
GO

CREATE TABLE ChannelGroup (
	idGroup int IDENTITY (1, 1) NOT NULL ,
	groupName varchar (200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) 
GO

CREATE TABLE ChannelMap (
	idChannelMap int IDENTITY (1, 1) NOT NULL ,
	idChannel int NOT NULL ,
	idCard int NOT NULL 
) 
GO

CREATE TABLE Favorite (
	idFavorite int IDENTITY (1, 1) NOT NULL ,
	idProgram int NOT NULL ,
	priority int NOT NULL ,
	timesWatched int NOT NULL 
) 
GO

CREATE TABLE GroupMap (
	idMap int IDENTITY (1, 1) NOT NULL ,
	idGroup int NOT NULL ,
	idChannel int NOT NULL 
) 
GO

CREATE TABLE Program (
	idProgram int IDENTITY (1, 1) NOT NULL ,
	idChannel int NOT NULL ,
	startTime datetime NOT NULL ,
	endTime datetime NOT NULL ,
	title varchar (2000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	description varchar (8000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	genre varchar (200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) 
GO

CREATE TABLE Recording (
	idRecording int IDENTITY (1, 1) NOT NULL ,
	idChannel int NOT NULL ,
	startTime datetime NOT NULL ,
	endTime int NOT NULL ,
	title varchar (200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	description varchar (8000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	genre varchar (200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	fileName varchar (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	keepUntil int NOT NULL ,
	keepUntilDate datetime NOT NULL ,
	timesWatched int NOT NULL 
) 
GO

CREATE TABLE Schedule (
	id_Schedule int IDENTITY (1, 1) NOT NULL ,
	idProgram int NOT NULL ,
	scheduleType int NOT NULL ,
	maxAirings int NOT NULL ,
	priority int NOT NULL ,
	directory varchar (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	quality int NOT NULL 
) 
GO

CREATE TABLE TuningDetail (
	idTuning int IDENTITY (1, 1) NOT NULL ,
	idChannel int NOT NULL ,
	name varchar (200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	provider varchar (200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	channelType int NOT NULL ,
	channelNumber int NOT NULL ,
	frequency int NOT NULL ,
	countryId int NOT NULL ,
	isRadio bit NOT NULL ,
	isTv bit NOT NULL ,
	networkId int NOT NULL ,
	transportId int NOT NULL ,
	serviceId int NOT NULL ,
	pmtPid int NOT NULL ,
	freeToAir bit NOT NULL ,
	modulation int NOT NULL ,
	polarisation int NOT NULL ,
	symbolrate int NOT NULL ,
	diseqc int NOT NULL ,
	switchingFrequency int NOT NULL ,
	bandwidth int NOT NULL ,
	majorChannel int NOT NULL ,
	minorChannel int NOT NULL ,
	pcrPid int NOT NULL ,
	videoSource int NOT NULL ,
	tuningSource int NOT NULL 
) 
GO

ALTER TABLE Airing WITH NOCHECK ADD 
	CONSTRAINT PK_Airings PRIMARY KEY  CLUSTERED 
	(
		idAiring
	)   
GO

ALTER TABLE Card WITH NOCHECK ADD 
	CONSTRAINT PK_Cards PRIMARY KEY  CLUSTERED 
	(
		idCard
	)   
GO

ALTER TABLE Channel WITH NOCHECK ADD 
	CONSTRAINT PK_Channels PRIMARY KEY  CLUSTERED 
	(
		idChannel
	)   
GO

ALTER TABLE ChannelGroup WITH NOCHECK ADD 
	CONSTRAINT PK_ChannelGroup PRIMARY KEY  CLUSTERED 
	(
		idGroup
	)   
GO

ALTER TABLE ChannelMap WITH NOCHECK ADD 
	CONSTRAINT PK_ChannelMap PRIMARY KEY  CLUSTERED 
	(
		idChannelMap
	)   
GO

ALTER TABLE Favorite WITH NOCHECK ADD 
	CONSTRAINT PK_Favorites PRIMARY KEY  CLUSTERED 
	(
		idFavorite
	)   
GO

ALTER TABLE GroupMap WITH NOCHECK ADD 
	CONSTRAINT PK_GroupMap PRIMARY KEY  CLUSTERED 
	(
		idMap
	)   
GO

ALTER TABLE Program WITH NOCHECK ADD 
	CONSTRAINT PK_Programs PRIMARY KEY  CLUSTERED 
	(
		idProgram
	)   
GO

ALTER TABLE Recording WITH NOCHECK ADD 
	CONSTRAINT PK_Recordings PRIMARY KEY  CLUSTERED 
	(
		idRecording
	)   
GO

ALTER TABLE Schedule WITH NOCHECK ADD 
	CONSTRAINT PK_Schedules PRIMARY KEY  CLUSTERED 
	(
		id_Schedule
	)   
GO

ALTER TABLE TuningDetail WITH NOCHECK ADD 
	CONSTRAINT PK_TuningDetail PRIMARY KEY  CLUSTERED 
	(
		idTuning
	)   
GO

ALTER TABLE Airing ADD 
	CONSTRAINT FK_Airings_Programs FOREIGN KEY 
	(
		idProgram
	) REFERENCES Program (
		idProgram
	)
GO

ALTER TABLE ChannelMap ADD 
	CONSTRAINT FK_ChannelMap_Cards FOREIGN KEY 
	(
		idCard
	) REFERENCES Card (
		idCard
	),
	CONSTRAINT FK_ChannelMap_Channels FOREIGN KEY 
	(
		idChannel
	) REFERENCES Channel (
		idChannel
	)
GO

ALTER TABLE Favorite ADD 
	CONSTRAINT FK_Favorites_Programs FOREIGN KEY 
	(
		idProgram
	) REFERENCES Program (
		idProgram
	)
GO

ALTER TABLE GroupMap ADD 
	CONSTRAINT FK_GroupMap_Channel FOREIGN KEY 
	(
		idChannel
	) REFERENCES Channel (
		idChannel
	),
	CONSTRAINT FK_GroupMap_ChannelGroup FOREIGN KEY 
	(
		idGroup
	) REFERENCES ChannelGroup (
		idGroup
	)
GO

ALTER TABLE Program ADD 
	CONSTRAINT FK_Programs_Channels FOREIGN KEY 
	(
		idChannel
	) REFERENCES Channel (
		idChannel
	)
GO

ALTER TABLE Recording ADD 
	CONSTRAINT FK_Recordings_Channels FOREIGN KEY 
	(
		idChannel
	) REFERENCES Channel (
		idChannel
	)
GO

ALTER TABLE Schedule ADD 
	CONSTRAINT FK_Schedules_Programs FOREIGN KEY 
	(
		idProgram
	) REFERENCES Program (
		idProgram
	)
GO

ALTER TABLE TuningDetail ADD 
	CONSTRAINT FK_TuningDetail_Channel FOREIGN KEY 
	(
		idChannel
	) REFERENCES Channel (
		idChannel
	)
GO

