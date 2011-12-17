
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 11/21/2011 14:50:00
-- Generated from EDMX file: C:\Development\tve3.exp.test2\Mediaportal\TV\Server\TVDatabase\EntityModel\Model.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [MpTvDb];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_CardChannelMap]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ChannelMaps] DROP CONSTRAINT [FK_CardChannelMap];
GO
IF OBJECT_ID(N'[dbo].[FK_CardConflict]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Conflicts] DROP CONSTRAINT [FK_CardConflict];
GO
IF OBJECT_ID(N'[dbo].[FK_CardGroupMapCard]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CardGroupMaps] DROP CONSTRAINT [FK_CardGroupMapCard];
GO
IF OBJECT_ID(N'[dbo].[FK_CardGroupMapCardGroup]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CardGroupMaps] DROP CONSTRAINT [FK_CardGroupMapCardGroup];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelChannelMap]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ChannelMaps] DROP CONSTRAINT [FK_ChannelChannelMap];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelConflict]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Conflicts] DROP CONSTRAINT [FK_ChannelConflict];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelHistory]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Histories] DROP CONSTRAINT [FK_ChannelHistory];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelLinkMap]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ChannelLinkageMaps] DROP CONSTRAINT [FK_ChannelLinkMap];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelPortalMap]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ChannelLinkageMaps] DROP CONSTRAINT [FK_ChannelPortalMap];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelProgram]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Programs] DROP CONSTRAINT [FK_ChannelProgram];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelRecording]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Recordings] DROP CONSTRAINT [FK_ChannelRecording];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelSchedule]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Schedules] DROP CONSTRAINT [FK_ChannelSchedule];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelTuningDetail]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TuningDetails] DROP CONSTRAINT [FK_ChannelTuningDetail];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelTvMovieMapping]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TvMovieMappings] DROP CONSTRAINT [FK_ChannelTvMovieMapping];
GO
IF OBJECT_ID(N'[dbo].[FK_DisEqcMotorCard]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DisEqcMotors] DROP CONSTRAINT [FK_DisEqcMotorCard];
GO
IF OBJECT_ID(N'[dbo].[FK_DisEqcMotorSatellite]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DisEqcMotors] DROP CONSTRAINT [FK_DisEqcMotorSatellite];
GO
IF OBJECT_ID(N'[dbo].[FK_GroupMapChannel]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GroupMaps] DROP CONSTRAINT [FK_GroupMapChannel];
GO
IF OBJECT_ID(N'[dbo].[FK_GroupMapChannelGroup]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GroupMaps] DROP CONSTRAINT [FK_GroupMapChannelGroup];
GO
IF OBJECT_ID(N'[dbo].[FK_KeywordKeywordMap]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[KeywordMaps] DROP CONSTRAINT [FK_KeywordKeywordMap];
GO
IF OBJECT_ID(N'[dbo].[FK_KeywordMapChannelGroup]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[KeywordMaps] DROP CONSTRAINT [FK_KeywordMapChannelGroup];
GO
IF OBJECT_ID(N'[dbo].[FK_KeywordPersonalTVGuideMap]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PersonalTVGuideMaps] DROP CONSTRAINT [FK_KeywordPersonalTVGuideMap];
GO
IF OBJECT_ID(N'[dbo].[FK_ProgramCategoryHistory]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Histories] DROP CONSTRAINT [FK_ProgramCategoryHistory];
GO
IF OBJECT_ID(N'[dbo].[FK_ProgramPersonalTVGuideMap]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PersonalTVGuideMaps] DROP CONSTRAINT [FK_ProgramPersonalTVGuideMap];
GO
IF OBJECT_ID(N'[dbo].[FK_ProgramProgramCategory]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Programs] DROP CONSTRAINT [FK_ProgramProgramCategory];
GO
IF OBJECT_ID(N'[dbo].[FK_ProgramProgramCredit]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProgramCredits] DROP CONSTRAINT [FK_ProgramProgramCredit];
GO
IF OBJECT_ID(N'[dbo].[FK_RecordingProgramCategory]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Recordings] DROP CONSTRAINT [FK_RecordingProgramCategory];
GO
IF OBJECT_ID(N'[dbo].[FK_RecordingRecordingCredit]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RecordingCredits] DROP CONSTRAINT [FK_RecordingRecordingCredit];
GO
IF OBJECT_ID(N'[dbo].[FK_ScheduleCanceledSchedule]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CanceledSchedules] DROP CONSTRAINT [FK_ScheduleCanceledSchedule];
GO
IF OBJECT_ID(N'[dbo].[FK_ScheduleConflict]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Conflicts] DROP CONSTRAINT [FK_ScheduleConflict];
GO
IF OBJECT_ID(N'[dbo].[FK_ScheduleConflict1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Conflicts] DROP CONSTRAINT [FK_ScheduleConflict1];
GO
IF OBJECT_ID(N'[dbo].[FK_ScheduleParentSchedule]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Schedules] DROP CONSTRAINT [FK_ScheduleParentSchedule];
GO
IF OBJECT_ID(N'[dbo].[FK_ScheduleRecording]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Recordings] DROP CONSTRAINT [FK_ScheduleRecording];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[CanceledSchedules]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CanceledSchedules];
GO
IF OBJECT_ID(N'[dbo].[CardGroupMaps]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CardGroupMaps];
GO
IF OBJECT_ID(N'[dbo].[CardGroups]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CardGroups];
GO
IF OBJECT_ID(N'[dbo].[Cards]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Cards];
GO
IF OBJECT_ID(N'[dbo].[ChannelGroups]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ChannelGroups];
GO
IF OBJECT_ID(N'[dbo].[ChannelLinkageMaps]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ChannelLinkageMaps];
GO
IF OBJECT_ID(N'[dbo].[ChannelMaps]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ChannelMaps];
GO
IF OBJECT_ID(N'[dbo].[Channels]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Channels];
GO
IF OBJECT_ID(N'[dbo].[Conflicts]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Conflicts];
GO
IF OBJECT_ID(N'[dbo].[DisEqcMotors]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DisEqcMotors];
GO
IF OBJECT_ID(N'[dbo].[Favorites]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Favorites];
GO
IF OBJECT_ID(N'[dbo].[GroupMaps]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GroupMaps];
GO
IF OBJECT_ID(N'[dbo].[Histories]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Histories];
GO
IF OBJECT_ID(N'[dbo].[KeywordMaps]', 'U') IS NOT NULL
    DROP TABLE [dbo].[KeywordMaps];
GO
IF OBJECT_ID(N'[dbo].[Keywords]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Keywords];
GO
IF OBJECT_ID(N'[dbo].[PendingDeletions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PendingDeletions];
GO
IF OBJECT_ID(N'[dbo].[PersonalTVGuideMaps]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PersonalTVGuideMaps];
GO
IF OBJECT_ID(N'[dbo].[ProgramCategories]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProgramCategories];
GO
IF OBJECT_ID(N'[dbo].[ProgramCredits]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProgramCredits];
GO
IF OBJECT_ID(N'[dbo].[Programs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Programs];
GO
IF OBJECT_ID(N'[dbo].[RecordingCredits]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RecordingCredits];
GO
IF OBJECT_ID(N'[dbo].[Recordings]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Recordings];
GO
IF OBJECT_ID(N'[dbo].[RuleBasedSchedules]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RuleBasedSchedules];
GO
IF OBJECT_ID(N'[dbo].[Satellites]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Satellites];
GO
IF OBJECT_ID(N'[dbo].[ScheduleRulesTemplates]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ScheduleRulesTemplates];
GO
IF OBJECT_ID(N'[dbo].[Schedules]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Schedules];
GO
IF OBJECT_ID(N'[dbo].[Settings]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Settings];
GO
IF OBJECT_ID(N'[dbo].[SoftwareEncoders]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SoftwareEncoders];
GO
IF OBJECT_ID(N'[dbo].[Timespans]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Timespans];
GO
IF OBJECT_ID(N'[dbo].[TuningDetails]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TuningDetails];
GO
IF OBJECT_ID(N'[dbo].[TvMovieMappings]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TvMovieMappings];
GO
IF OBJECT_ID(N'[dbo].[Versions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Versions];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'CanceledSchedules'
CREATE TABLE [dbo].[CanceledSchedules] (
    [idCanceledSchedule] int IDENTITY(1,1) NOT NULL,
    [idSchedule] int  NOT NULL,
    [idChannel] int  NOT NULL,
    [cancelDateTime] datetime  NOT NULL
);
GO

-- Creating table 'Cards'
CREATE TABLE [dbo].[Cards] (
    [idCard] int IDENTITY(1,1) NOT NULL,
    [devicePath] varchar(2000)  NOT NULL,
    [name] varchar(200)  NOT NULL,
    [priority] int  NOT NULL,
    [grabEPG] bit  NOT NULL,
    [lastEpgGrab] datetime  NULL,
    [recordingFolder] varchar(256)  NOT NULL,
    [enabled] bit  NOT NULL,
    [camType] int  NOT NULL,
    [timeshiftingFolder] varchar(256)  NOT NULL,
    [recordingFormat] int  NOT NULL,
    [decryptLimit] int  NOT NULL,
    [preload] bit  NOT NULL,
    [CAM] bit  NOT NULL,
    [NetProvider] int  NOT NULL,
    [stopgraph] bit  NOT NULL
);
GO

-- Creating table 'CardGroups'
CREATE TABLE [dbo].[CardGroups] (
    [idCardGroup] int IDENTITY(1,1) NOT NULL,
    [name] varchar(255)  NOT NULL
);
GO

-- Creating table 'CardGroupMaps'
CREATE TABLE [dbo].[CardGroupMaps] (
    [idMapping] int IDENTITY(1,1) NOT NULL,
    [idCard] int  NOT NULL,
    [idCardGroup] int  NOT NULL
);
GO

-- Creating table 'Channels'
CREATE TABLE [dbo].[Channels] (
    [idChannel] int IDENTITY(1,1) NOT NULL,
    [timesWatched] int  NOT NULL,
    [totalTimeWatched] datetime  NULL,
    [grabEpg] bit  NOT NULL,
    [lastGrabTime] datetime  NULL,
    [sortOrder] int  NOT NULL,
    [visibleInGuide] bit  NOT NULL,
    [externalId] varchar(200)  NULL,
    [displayName] varchar(200)  NOT NULL,
    [epgHasGaps] bit  NOT NULL,
    [mediaType] int  NOT NULL
);
GO

-- Creating table 'ChannelGroups'
CREATE TABLE [dbo].[ChannelGroups] (
    [idGroup] int IDENTITY(1,1) NOT NULL,
    [groupName] varchar(200)  NOT NULL,
    [sortOrder] int  NOT NULL
);
GO

-- Creating table 'ChannelLinkageMaps'
CREATE TABLE [dbo].[ChannelLinkageMaps] (
    [idMapping] int IDENTITY(1,1) NOT NULL,
    [idPortalChannel] int  NOT NULL,
    [idLinkedChannel] int  NOT NULL,
    [displayName] varchar(200)  NOT NULL
);
GO

-- Creating table 'ChannelMaps'
CREATE TABLE [dbo].[ChannelMaps] (
    [idChannelMap] int IDENTITY(1,1) NOT NULL,
    [idChannel] int  NOT NULL,
    [idCard] int  NOT NULL,
    [epgOnly] bit  NOT NULL
);
GO

-- Creating table 'Conflicts'
CREATE TABLE [dbo].[Conflicts] (
    [idConflict] int IDENTITY(1,1) NOT NULL,
    [idSchedule] int  NOT NULL,
    [idConflictingSchedule] int  NOT NULL,
    [idChannel] int  NOT NULL,
    [conflictDate] datetime  NOT NULL,
    [idCard] int  NULL
);
GO

-- Creating table 'DisEqcMotors'
CREATE TABLE [dbo].[DisEqcMotors] (
    [idDiSEqCMotor] int IDENTITY(1,1) NOT NULL,
    [idCard] int  NOT NULL,
    [idSatellite] int  NOT NULL,
    [position] int  NOT NULL
);
GO

-- Creating table 'Favorites'
CREATE TABLE [dbo].[Favorites] (
    [idFavorite] int IDENTITY(1,1) NOT NULL,
    [idProgram] int  NOT NULL,
    [priority] int  NOT NULL,
    [timesWatched] int  NOT NULL
);
GO

-- Creating table 'GroupMaps'
CREATE TABLE [dbo].[GroupMaps] (
    [idMap] int IDENTITY(1,1) NOT NULL,
    [idGroup] int  NOT NULL,
    [idChannel] int  NOT NULL,
    [SortOrder] int  NOT NULL,
    [mediaType] int  NOT NULL
);
GO

-- Creating table 'Histories'
CREATE TABLE [dbo].[Histories] (
    [idHistory] int IDENTITY(1,1) NOT NULL,
    [idChannel] int  NOT NULL,
    [startTime] datetime  NOT NULL,
    [endTime] datetime  NOT NULL,
    [title] varchar(1000)  NOT NULL,
    [description] varchar(1000)  NOT NULL,
    [recorded] bit  NOT NULL,
    [watched] int  NOT NULL,
    [idProgramCategory] int  NULL
);
GO

-- Creating table 'Keywords'
CREATE TABLE [dbo].[Keywords] (
    [idKeyword] int IDENTITY(1,1) NOT NULL,
    [keywordName] varchar(200)  NOT NULL,
    [rating] int  NOT NULL,
    [autoRecord] bit  NOT NULL,
    [searchIn] int  NOT NULL
);
GO

-- Creating table 'KeywordMaps'
CREATE TABLE [dbo].[KeywordMaps] (
    [idKeywordMap] int IDENTITY(1,1) NOT NULL,
    [idKeyword] int  NOT NULL,
    [idChannelGroup] int  NOT NULL
);
GO

-- Creating table 'PendingDeletions'
CREATE TABLE [dbo].[PendingDeletions] (
    [idPendingDeletion] int IDENTITY(1,1) NOT NULL,
    [fileName] varchar(max)  NOT NULL
);
GO

-- Creating table 'PersonalTVGuideMaps'
CREATE TABLE [dbo].[PersonalTVGuideMaps] (
    [idPersonalTVGuideMap] int IDENTITY(1,1) NOT NULL,
    [idKeyword] int  NOT NULL,
    [idProgram] int  NOT NULL
);
GO

-- Creating table 'Programs'
CREATE TABLE [dbo].[Programs] (
    [idProgram] int IDENTITY(1,1) NOT NULL,
    [idChannel] int  NOT NULL,
    [startTime] datetime  NOT NULL,
    [endTime] datetime  NOT NULL,
    [title] varchar(2000)  NOT NULL,
    [description] varchar(8000)  NOT NULL,
    [seriesNum] varchar(200)  NOT NULL,
    [episodeNum] varchar(200)  NOT NULL,
    [originalAirDate] datetime  NULL,
    [classification] varchar(200)  NOT NULL,
    [starRating] int  NOT NULL,
    [parentalRating] int  NOT NULL,
    [episodeName] nvarchar(max)  NOT NULL,
    [episodePart] nvarchar(max)  NOT NULL,
    [state] int  NOT NULL,
    [previouslyShown] bit  NOT NULL,
    [idProgramCategory] int  NULL,
    [startTimeDayOfWeek] smallint  NOT NULL,
    [endTimeDayOfWeek] smallint  NOT NULL,
    [endTimeOffset] datetime  NOT NULL,
    [startTimeOffset] datetime  NOT NULL
);
GO

-- Creating table 'ProgramCategories'
CREATE TABLE [dbo].[ProgramCategories] (
    [idProgramCategory] int IDENTITY(1,1) NOT NULL,
    [category] varchar(50)  NOT NULL
);
GO

-- Creating table 'ProgramCredits'
CREATE TABLE [dbo].[ProgramCredits] (
    [idProgramCredit] int IDENTITY(1,1) NOT NULL,
    [idProgram] int  NOT NULL,
    [person] varchar(200)  NOT NULL,
    [role] varchar(50)  NOT NULL
);
GO

-- Creating table 'Recordings'
CREATE TABLE [dbo].[Recordings] (
    [idRecording] int IDENTITY(1,1) NOT NULL,
    [idChannel] int  NULL,
    [startTime] datetime  NOT NULL,
    [endTime] datetime  NOT NULL,
    [title] varchar(2000)  NOT NULL,
    [description] varchar(8000)  NOT NULL,
    [fileName] varchar(260)  NOT NULL,
    [keepUntil] int  NOT NULL,
    [keepUntilDate] datetime  NULL,
    [timesWatched] int  NOT NULL,
    [stopTime] int  NOT NULL,
    [episodeName] varchar(max)  NOT NULL,
    [seriesNum] varchar(200)  NOT NULL,
    [episodeNum] varchar(200)  NOT NULL,
    [episodePart] varchar(max)  NOT NULL,
    [isRecording] bit  NOT NULL,
    [idSchedule] int  NULL,
    [mediaType] int  NOT NULL,
    [idProgramCategory] int  NULL
);
GO

-- Creating table 'RuleBasedSchedules'
CREATE TABLE [dbo].[RuleBasedSchedules] (
    [id_RuleBasedSchedule] int IDENTITY(1,1) NOT NULL,
    [scheduleName] varchar(256)  NOT NULL,
    [maxAirings] int  NOT NULL,
    [priority] int  NOT NULL,
    [directory] varchar(1024)  NOT NULL,
    [quality] int  NOT NULL,
    [keepMethod] int  NOT NULL,
    [keepDate] datetime  NULL,
    [preRecordInterval] int  NOT NULL,
    [postRecordInterval] int  NOT NULL,
    [rules] varchar(max)  NULL
);
GO

-- Creating table 'Satellites'
CREATE TABLE [dbo].[Satellites] (
    [idSatellite] int IDENTITY(1,1) NOT NULL,
    [satelliteName] varchar(200)  NOT NULL,
    [transponderFileName] varchar(200)  NOT NULL
);
GO

-- Creating table 'Schedules'
CREATE TABLE [dbo].[Schedules] (
    [id_Schedule] int IDENTITY(1,1) NOT NULL,
    [idChannel] int  NOT NULL,
    [scheduleType] int  NOT NULL,
    [programName] varchar(256)  NOT NULL,
    [startTime] datetime  NOT NULL,
    [endTime] datetime  NOT NULL,
    [maxAirings] int  NOT NULL,
    [priority] int  NOT NULL,
    [directory] varchar(1024)  NOT NULL,
    [quality] int  NOT NULL,
    [keepMethod] int  NOT NULL,
    [keepDate] datetime  NULL,
    [preRecordInterval] int  NOT NULL,
    [postRecordInterval] int  NOT NULL,
    [canceled] datetime  NOT NULL,
    [series] bit  NOT NULL,
    [idParentSchedule] int  NULL
);
GO

-- Creating table 'ScheduleRulesTemplates'
CREATE TABLE [dbo].[ScheduleRulesTemplates] (
    [idScheduleRulesTemplate] int IDENTITY(1,1) NOT NULL,
    [name] varchar(50)  NOT NULL,
    [rules] varchar(max)  NOT NULL,
    [enabled] bit  NOT NULL,
    [usages] int  NOT NULL,
    [editable] bit  NOT NULL
);
GO

-- Creating table 'Settings'
CREATE TABLE [dbo].[Settings] (
    [idSetting] int IDENTITY(1,1) NOT NULL,
    [tag] varchar(200)  NOT NULL,
    [value] varchar(4096)  NOT NULL
);
GO

-- Creating table 'SoftwareEncoders'
CREATE TABLE [dbo].[SoftwareEncoders] (
    [idEncoder] int IDENTITY(1,1) NOT NULL,
    [priority] int  NOT NULL,
    [name] varchar(200)  NOT NULL,
    [type] int  NOT NULL,
    [reusable] bit  NOT NULL
);
GO

-- Creating table 'Timespans'
CREATE TABLE [dbo].[Timespans] (
    [idTimespan] int IDENTITY(1,1) NOT NULL,
    [startTime] datetime  NOT NULL,
    [endTime] datetime  NOT NULL,
    [dayOfWeek] int  NOT NULL,
    [idKeyword] int  NOT NULL
);
GO

-- Creating table 'TuningDetails'
CREATE TABLE [dbo].[TuningDetails] (
    [idTuning] int IDENTITY(1,1) NOT NULL,
    [idChannel] int  NOT NULL,
    [name] varchar(200)  NOT NULL,
    [provider] varchar(200)  NOT NULL,
    [channelType] int  NOT NULL,
    [channelNumber] int  NOT NULL,
    [frequency] int  NOT NULL,
    [countryId] int  NOT NULL,
    [networkId] int  NOT NULL,
    [transportId] int  NOT NULL,
    [serviceId] int  NOT NULL,
    [pmtPid] int  NOT NULL,
    [freeToAir] bit  NOT NULL,
    [modulation] int  NOT NULL,
    [polarisation] int  NOT NULL,
    [symbolrate] int  NOT NULL,
    [diseqc] int  NOT NULL,
    [switchingFrequency] int  NOT NULL,
    [bandwidth] int  NOT NULL,
    [majorChannel] int  NOT NULL,
    [minorChannel] int  NOT NULL,
    [videoSource] int  NOT NULL,
    [tuningSource] int  NOT NULL,
    [band] int  NOT NULL,
    [satIndex] int  NOT NULL,
    [innerFecRate] int  NOT NULL,
    [pilot] int  NOT NULL,
    [rollOff] int  NOT NULL,
    [url] varchar(200)  NOT NULL,
    [bitrate] int  NOT NULL,
    [audioSource] int  NOT NULL,
    [isVCRSignal] bit  NOT NULL,
    [mediaType] int  NOT NULL
);
GO

-- Creating table 'TvMovieMappings'
CREATE TABLE [dbo].[TvMovieMappings] (
    [idMapping] int IDENTITY(1,1) NOT NULL,
    [idChannel] int  NOT NULL,
    [stationName] varchar(200)  NOT NULL,
    [timeSharingStart] varchar(200)  NOT NULL,
    [timeSharingEnd] varchar(200)  NOT NULL
);
GO

-- Creating table 'Versions'
CREATE TABLE [dbo].[Versions] (
    [idVersion] int IDENTITY(1,1) NOT NULL,
    [versionNumber] int  NOT NULL
);
GO

-- Creating table 'RecordingCredits'
CREATE TABLE [dbo].[RecordingCredits] (
    [idRecordingCredit] int IDENTITY(1,1) NOT NULL,
    [idRecording] int  NOT NULL,
    [person] varchar(200)  NOT NULL,
    [role] varchar(50)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [idCanceledSchedule] in table 'CanceledSchedules'
ALTER TABLE [dbo].[CanceledSchedules]
ADD CONSTRAINT [PK_CanceledSchedules]
    PRIMARY KEY CLUSTERED ([idCanceledSchedule] ASC);
GO

-- Creating primary key on [idCard] in table 'Cards'
ALTER TABLE [dbo].[Cards]
ADD CONSTRAINT [PK_Cards]
    PRIMARY KEY CLUSTERED ([idCard] ASC);
GO

-- Creating primary key on [idCardGroup] in table 'CardGroups'
ALTER TABLE [dbo].[CardGroups]
ADD CONSTRAINT [PK_CardGroups]
    PRIMARY KEY CLUSTERED ([idCardGroup] ASC);
GO

-- Creating primary key on [idMapping] in table 'CardGroupMaps'
ALTER TABLE [dbo].[CardGroupMaps]
ADD CONSTRAINT [PK_CardGroupMaps]
    PRIMARY KEY CLUSTERED ([idMapping] ASC);
GO

-- Creating primary key on [idChannel] in table 'Channels'
ALTER TABLE [dbo].[Channels]
ADD CONSTRAINT [PK_Channels]
    PRIMARY KEY CLUSTERED ([idChannel] ASC);
GO

-- Creating primary key on [idGroup] in table 'ChannelGroups'
ALTER TABLE [dbo].[ChannelGroups]
ADD CONSTRAINT [PK_ChannelGroups]
    PRIMARY KEY CLUSTERED ([idGroup] ASC);
GO

-- Creating primary key on [idMapping] in table 'ChannelLinkageMaps'
ALTER TABLE [dbo].[ChannelLinkageMaps]
ADD CONSTRAINT [PK_ChannelLinkageMaps]
    PRIMARY KEY CLUSTERED ([idMapping] ASC);
GO

-- Creating primary key on [idChannelMap] in table 'ChannelMaps'
ALTER TABLE [dbo].[ChannelMaps]
ADD CONSTRAINT [PK_ChannelMaps]
    PRIMARY KEY CLUSTERED ([idChannelMap] ASC);
GO

-- Creating primary key on [idConflict] in table 'Conflicts'
ALTER TABLE [dbo].[Conflicts]
ADD CONSTRAINT [PK_Conflicts]
    PRIMARY KEY CLUSTERED ([idConflict] ASC);
GO

-- Creating primary key on [idDiSEqCMotor] in table 'DisEqcMotors'
ALTER TABLE [dbo].[DisEqcMotors]
ADD CONSTRAINT [PK_DisEqcMotors]
    PRIMARY KEY CLUSTERED ([idDiSEqCMotor] ASC);
GO

-- Creating primary key on [idFavorite] in table 'Favorites'
ALTER TABLE [dbo].[Favorites]
ADD CONSTRAINT [PK_Favorites]
    PRIMARY KEY CLUSTERED ([idFavorite] ASC);
GO

-- Creating primary key on [idMap] in table 'GroupMaps'
ALTER TABLE [dbo].[GroupMaps]
ADD CONSTRAINT [PK_GroupMaps]
    PRIMARY KEY CLUSTERED ([idMap] ASC);
GO

-- Creating primary key on [idHistory] in table 'Histories'
ALTER TABLE [dbo].[Histories]
ADD CONSTRAINT [PK_Histories]
    PRIMARY KEY CLUSTERED ([idHistory] ASC);
GO

-- Creating primary key on [idKeyword] in table 'Keywords'
ALTER TABLE [dbo].[Keywords]
ADD CONSTRAINT [PK_Keywords]
    PRIMARY KEY CLUSTERED ([idKeyword] ASC);
GO

-- Creating primary key on [idKeywordMap] in table 'KeywordMaps'
ALTER TABLE [dbo].[KeywordMaps]
ADD CONSTRAINT [PK_KeywordMaps]
    PRIMARY KEY CLUSTERED ([idKeywordMap] ASC);
GO

-- Creating primary key on [idPendingDeletion] in table 'PendingDeletions'
ALTER TABLE [dbo].[PendingDeletions]
ADD CONSTRAINT [PK_PendingDeletions]
    PRIMARY KEY CLUSTERED ([idPendingDeletion] ASC);
GO

-- Creating primary key on [idPersonalTVGuideMap] in table 'PersonalTVGuideMaps'
ALTER TABLE [dbo].[PersonalTVGuideMaps]
ADD CONSTRAINT [PK_PersonalTVGuideMaps]
    PRIMARY KEY CLUSTERED ([idPersonalTVGuideMap] ASC);
GO

-- Creating primary key on [idProgram] in table 'Programs'
ALTER TABLE [dbo].[Programs]
ADD CONSTRAINT [PK_Programs]
    PRIMARY KEY CLUSTERED ([idProgram] ASC);
GO

-- Creating primary key on [idProgramCategory] in table 'ProgramCategories'
ALTER TABLE [dbo].[ProgramCategories]
ADD CONSTRAINT [PK_ProgramCategories]
    PRIMARY KEY CLUSTERED ([idProgramCategory] ASC);
GO

-- Creating primary key on [idProgramCredit] in table 'ProgramCredits'
ALTER TABLE [dbo].[ProgramCredits]
ADD CONSTRAINT [PK_ProgramCredits]
    PRIMARY KEY CLUSTERED ([idProgramCredit] ASC);
GO

-- Creating primary key on [idRecording] in table 'Recordings'
ALTER TABLE [dbo].[Recordings]
ADD CONSTRAINT [PK_Recordings]
    PRIMARY KEY CLUSTERED ([idRecording] ASC);
GO

-- Creating primary key on [id_RuleBasedSchedule] in table 'RuleBasedSchedules'
ALTER TABLE [dbo].[RuleBasedSchedules]
ADD CONSTRAINT [PK_RuleBasedSchedules]
    PRIMARY KEY CLUSTERED ([id_RuleBasedSchedule] ASC);
GO

-- Creating primary key on [idSatellite] in table 'Satellites'
ALTER TABLE [dbo].[Satellites]
ADD CONSTRAINT [PK_Satellites]
    PRIMARY KEY CLUSTERED ([idSatellite] ASC);
GO

-- Creating primary key on [id_Schedule] in table 'Schedules'
ALTER TABLE [dbo].[Schedules]
ADD CONSTRAINT [PK_Schedules]
    PRIMARY KEY CLUSTERED ([id_Schedule] ASC);
GO

-- Creating primary key on [idScheduleRulesTemplate] in table 'ScheduleRulesTemplates'
ALTER TABLE [dbo].[ScheduleRulesTemplates]
ADD CONSTRAINT [PK_ScheduleRulesTemplates]
    PRIMARY KEY CLUSTERED ([idScheduleRulesTemplate] ASC);
GO

-- Creating primary key on [idSetting] in table 'Settings'
ALTER TABLE [dbo].[Settings]
ADD CONSTRAINT [PK_Settings]
    PRIMARY KEY CLUSTERED ([idSetting] ASC);
GO

-- Creating primary key on [idEncoder] in table 'SoftwareEncoders'
ALTER TABLE [dbo].[SoftwareEncoders]
ADD CONSTRAINT [PK_SoftwareEncoders]
    PRIMARY KEY CLUSTERED ([idEncoder] ASC);
GO

-- Creating primary key on [idTimespan] in table 'Timespans'
ALTER TABLE [dbo].[Timespans]
ADD CONSTRAINT [PK_Timespans]
    PRIMARY KEY CLUSTERED ([idTimespan] ASC);
GO

-- Creating primary key on [idTuning] in table 'TuningDetails'
ALTER TABLE [dbo].[TuningDetails]
ADD CONSTRAINT [PK_TuningDetails]
    PRIMARY KEY CLUSTERED ([idTuning] ASC);
GO

-- Creating primary key on [idMapping] in table 'TvMovieMappings'
ALTER TABLE [dbo].[TvMovieMappings]
ADD CONSTRAINT [PK_TvMovieMappings]
    PRIMARY KEY CLUSTERED ([idMapping] ASC);
GO

-- Creating primary key on [idVersion] in table 'Versions'
ALTER TABLE [dbo].[Versions]
ADD CONSTRAINT [PK_Versions]
    PRIMARY KEY CLUSTERED ([idVersion] ASC);
GO

-- Creating primary key on [idRecordingCredit] in table 'RecordingCredits'
ALTER TABLE [dbo].[RecordingCredits]
ADD CONSTRAINT [PK_RecordingCredits]
    PRIMARY KEY CLUSTERED ([idRecordingCredit] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [idGroup] in table 'GroupMaps'
ALTER TABLE [dbo].[GroupMaps]
ADD CONSTRAINT [FK_GroupMapChannelGroup]
    FOREIGN KEY ([idGroup])
    REFERENCES [dbo].[ChannelGroups]
        ([idGroup])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GroupMapChannelGroup'
CREATE INDEX [IX_FK_GroupMapChannelGroup]
ON [dbo].[GroupMaps]
    ([idGroup]);
GO

-- Creating foreign key on [idChannel] in table 'GroupMaps'
ALTER TABLE [dbo].[GroupMaps]
ADD CONSTRAINT [FK_GroupMapChannel]
    FOREIGN KEY ([idChannel])
    REFERENCES [dbo].[Channels]
        ([idChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GroupMapChannel'
CREATE INDEX [IX_FK_GroupMapChannel]
ON [dbo].[GroupMaps]
    ([idChannel]);
GO

-- Creating foreign key on [idCard] in table 'CardGroupMaps'
ALTER TABLE [dbo].[CardGroupMaps]
ADD CONSTRAINT [FK_CardGroupMapCard]
    FOREIGN KEY ([idCard])
    REFERENCES [dbo].[Cards]
        ([idCard])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardGroupMapCard'
CREATE INDEX [IX_FK_CardGroupMapCard]
ON [dbo].[CardGroupMaps]
    ([idCard]);
GO

-- Creating foreign key on [idCard] in table 'DisEqcMotors'
ALTER TABLE [dbo].[DisEqcMotors]
ADD CONSTRAINT [FK_DisEqcMotorCard]
    FOREIGN KEY ([idCard])
    REFERENCES [dbo].[Cards]
        ([idCard])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DisEqcMotorCard'
CREATE INDEX [IX_FK_DisEqcMotorCard]
ON [dbo].[DisEqcMotors]
    ([idCard]);
GO

-- Creating foreign key on [idChannel] in table 'Recordings'
ALTER TABLE [dbo].[Recordings]
ADD CONSTRAINT [FK_ChannelRecording]
    FOREIGN KEY ([idChannel])
    REFERENCES [dbo].[Channels]
        ([idChannel])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelRecording'
CREATE INDEX [IX_FK_ChannelRecording]
ON [dbo].[Recordings]
    ([idChannel]);
GO

-- Creating foreign key on [idCardGroup] in table 'CardGroupMaps'
ALTER TABLE [dbo].[CardGroupMaps]
ADD CONSTRAINT [FK_CardGroupMapCardGroup]
    FOREIGN KEY ([idCardGroup])
    REFERENCES [dbo].[CardGroups]
        ([idCardGroup])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardGroupMapCardGroup'
CREATE INDEX [IX_FK_CardGroupMapCardGroup]
ON [dbo].[CardGroupMaps]
    ([idCardGroup]);
GO

-- Creating foreign key on [idChannel] in table 'Programs'
ALTER TABLE [dbo].[Programs]
ADD CONSTRAINT [FK_ChannelProgram]
    FOREIGN KEY ([idChannel])
    REFERENCES [dbo].[Channels]
        ([idChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelProgram'
CREATE INDEX [IX_FK_ChannelProgram]
ON [dbo].[Programs]
    ([idChannel]);
GO

-- Creating foreign key on [idCard] in table 'ChannelMaps'
ALTER TABLE [dbo].[ChannelMaps]
ADD CONSTRAINT [FK_CardChannelMap]
    FOREIGN KEY ([idCard])
    REFERENCES [dbo].[Cards]
        ([idCard])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardChannelMap'
CREATE INDEX [IX_FK_CardChannelMap]
ON [dbo].[ChannelMaps]
    ([idCard]);
GO

-- Creating foreign key on [idChannel] in table 'ChannelMaps'
ALTER TABLE [dbo].[ChannelMaps]
ADD CONSTRAINT [FK_ChannelChannelMap]
    FOREIGN KEY ([idChannel])
    REFERENCES [dbo].[Channels]
        ([idChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelChannelMap'
CREATE INDEX [IX_FK_ChannelChannelMap]
ON [dbo].[ChannelMaps]
    ([idChannel]);
GO

-- Creating foreign key on [idChannel] in table 'Schedules'
ALTER TABLE [dbo].[Schedules]
ADD CONSTRAINT [FK_ChannelSchedule]
    FOREIGN KEY ([idChannel])
    REFERENCES [dbo].[Channels]
        ([idChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelSchedule'
CREATE INDEX [IX_FK_ChannelSchedule]
ON [dbo].[Schedules]
    ([idChannel]);
GO

-- Creating foreign key on [idParentSchedule] in table 'Schedules'
ALTER TABLE [dbo].[Schedules]
ADD CONSTRAINT [FK_ScheduleParentSchedule]
    FOREIGN KEY ([idParentSchedule])
    REFERENCES [dbo].[Schedules]
        ([id_Schedule])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleParentSchedule'
CREATE INDEX [IX_FK_ScheduleParentSchedule]
ON [dbo].[Schedules]
    ([idParentSchedule]);
GO

-- Creating foreign key on [idProgramCategory] in table 'Programs'
ALTER TABLE [dbo].[Programs]
ADD CONSTRAINT [FK_ProgramProgramCategory]
    FOREIGN KEY ([idProgramCategory])
    REFERENCES [dbo].[ProgramCategories]
        ([idProgramCategory])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramProgramCategory'
CREATE INDEX [IX_FK_ProgramProgramCategory]
ON [dbo].[Programs]
    ([idProgramCategory]);
GO

-- Creating foreign key on [idProgram] in table 'ProgramCredits'
ALTER TABLE [dbo].[ProgramCredits]
ADD CONSTRAINT [FK_ProgramProgramCredit]
    FOREIGN KEY ([idProgram])
    REFERENCES [dbo].[Programs]
        ([idProgram])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramProgramCredit'
CREATE INDEX [IX_FK_ProgramProgramCredit]
ON [dbo].[ProgramCredits]
    ([idProgram]);
GO

-- Creating foreign key on [idChannel] in table 'Histories'
ALTER TABLE [dbo].[Histories]
ADD CONSTRAINT [FK_ChannelHistory]
    FOREIGN KEY ([idChannel])
    REFERENCES [dbo].[Channels]
        ([idChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelHistory'
CREATE INDEX [IX_FK_ChannelHistory]
ON [dbo].[Histories]
    ([idChannel]);
GO

-- Creating foreign key on [idChannel] in table 'TuningDetails'
ALTER TABLE [dbo].[TuningDetails]
ADD CONSTRAINT [FK_ChannelTuningDetail]
    FOREIGN KEY ([idChannel])
    REFERENCES [dbo].[Channels]
        ([idChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelTuningDetail'
CREATE INDEX [IX_FK_ChannelTuningDetail]
ON [dbo].[TuningDetails]
    ([idChannel]);
GO

-- Creating foreign key on [idChannel] in table 'TvMovieMappings'
ALTER TABLE [dbo].[TvMovieMappings]
ADD CONSTRAINT [FK_ChannelTvMovieMapping]
    FOREIGN KEY ([idChannel])
    REFERENCES [dbo].[Channels]
        ([idChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelTvMovieMapping'
CREATE INDEX [IX_FK_ChannelTvMovieMapping]
ON [dbo].[TvMovieMappings]
    ([idChannel]);
GO

-- Creating foreign key on [idSatellite] in table 'DisEqcMotors'
ALTER TABLE [dbo].[DisEqcMotors]
ADD CONSTRAINT [FK_DisEqcMotorSatellite]
    FOREIGN KEY ([idSatellite])
    REFERENCES [dbo].[Satellites]
        ([idSatellite])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DisEqcMotorSatellite'
CREATE INDEX [IX_FK_DisEqcMotorSatellite]
ON [dbo].[DisEqcMotors]
    ([idSatellite]);
GO

-- Creating foreign key on [idProgram] in table 'PersonalTVGuideMaps'
ALTER TABLE [dbo].[PersonalTVGuideMaps]
ADD CONSTRAINT [FK_ProgramPersonalTVGuideMap]
    FOREIGN KEY ([idProgram])
    REFERENCES [dbo].[Programs]
        ([idProgram])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramPersonalTVGuideMap'
CREATE INDEX [IX_FK_ProgramPersonalTVGuideMap]
ON [dbo].[PersonalTVGuideMaps]
    ([idProgram]);
GO

-- Creating foreign key on [idKeyword] in table 'PersonalTVGuideMaps'
ALTER TABLE [dbo].[PersonalTVGuideMaps]
ADD CONSTRAINT [FK_KeywordPersonalTVGuideMap]
    FOREIGN KEY ([idKeyword])
    REFERENCES [dbo].[Keywords]
        ([idKeyword])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_KeywordPersonalTVGuideMap'
CREATE INDEX [IX_FK_KeywordPersonalTVGuideMap]
ON [dbo].[PersonalTVGuideMaps]
    ([idKeyword]);
GO

-- Creating foreign key on [idSchedule] in table 'Recordings'
ALTER TABLE [dbo].[Recordings]
ADD CONSTRAINT [FK_ScheduleRecording]
    FOREIGN KEY ([idSchedule])
    REFERENCES [dbo].[Schedules]
        ([id_Schedule])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleRecording'
CREATE INDEX [IX_FK_ScheduleRecording]
ON [dbo].[Recordings]
    ([idSchedule]);
GO

-- Creating foreign key on [idKeyword] in table 'KeywordMaps'
ALTER TABLE [dbo].[KeywordMaps]
ADD CONSTRAINT [FK_KeywordKeywordMap]
    FOREIGN KEY ([idKeyword])
    REFERENCES [dbo].[Keywords]
        ([idKeyword])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_KeywordKeywordMap'
CREATE INDEX [IX_FK_KeywordKeywordMap]
ON [dbo].[KeywordMaps]
    ([idKeyword]);
GO

-- Creating foreign key on [idChannelGroup] in table 'KeywordMaps'
ALTER TABLE [dbo].[KeywordMaps]
ADD CONSTRAINT [FK_KeywordMapChannelGroup]
    FOREIGN KEY ([idChannelGroup])
    REFERENCES [dbo].[ChannelGroups]
        ([idGroup])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_KeywordMapChannelGroup'
CREATE INDEX [IX_FK_KeywordMapChannelGroup]
ON [dbo].[KeywordMaps]
    ([idChannelGroup]);
GO

-- Creating foreign key on [idRecording] in table 'RecordingCredits'
ALTER TABLE [dbo].[RecordingCredits]
ADD CONSTRAINT [FK_RecordingRecordingCredit]
    FOREIGN KEY ([idRecording])
    REFERENCES [dbo].[Recordings]
        ([idRecording])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RecordingRecordingCredit'
CREATE INDEX [IX_FK_RecordingRecordingCredit]
ON [dbo].[RecordingCredits]
    ([idRecording]);
GO

-- Creating foreign key on [idSchedule] in table 'CanceledSchedules'
ALTER TABLE [dbo].[CanceledSchedules]
ADD CONSTRAINT [FK_ScheduleCanceledSchedule]
    FOREIGN KEY ([idSchedule])
    REFERENCES [dbo].[Schedules]
        ([id_Schedule])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleCanceledSchedule'
CREATE INDEX [IX_FK_ScheduleCanceledSchedule]
ON [dbo].[CanceledSchedules]
    ([idSchedule]);
GO

-- Creating foreign key on [idLinkedChannel] in table 'ChannelLinkageMaps'
ALTER TABLE [dbo].[ChannelLinkageMaps]
ADD CONSTRAINT [FK_ChannelLinkMap]
    FOREIGN KEY ([idLinkedChannel])
    REFERENCES [dbo].[Channels]
        ([idChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelLinkMap'
CREATE INDEX [IX_FK_ChannelLinkMap]
ON [dbo].[ChannelLinkageMaps]
    ([idLinkedChannel]);
GO

-- Creating foreign key on [idPortalChannel] in table 'ChannelLinkageMaps'
ALTER TABLE [dbo].[ChannelLinkageMaps]
ADD CONSTRAINT [FK_ChannelPortalMap]
    FOREIGN KEY ([idPortalChannel])
    REFERENCES [dbo].[Channels]
        ([idChannel])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelPortalMap'
CREATE INDEX [IX_FK_ChannelPortalMap]
ON [dbo].[ChannelLinkageMaps]
    ([idPortalChannel]);
GO

-- Creating foreign key on [idProgramCategory] in table 'Recordings'
ALTER TABLE [dbo].[Recordings]
ADD CONSTRAINT [FK_RecordingProgramCategory]
    FOREIGN KEY ([idProgramCategory])
    REFERENCES [dbo].[ProgramCategories]
        ([idProgramCategory])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RecordingProgramCategory'
CREATE INDEX [IX_FK_RecordingProgramCategory]
ON [dbo].[Recordings]
    ([idProgramCategory]);
GO

-- Creating foreign key on [idCard] in table 'Conflicts'
ALTER TABLE [dbo].[Conflicts]
ADD CONSTRAINT [FK_CardConflict]
    FOREIGN KEY ([idCard])
    REFERENCES [dbo].[Cards]
        ([idCard])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardConflict'
CREATE INDEX [IX_FK_CardConflict]
ON [dbo].[Conflicts]
    ([idCard]);
GO

-- Creating foreign key on [idChannel] in table 'Conflicts'
ALTER TABLE [dbo].[Conflicts]
ADD CONSTRAINT [FK_ChannelConflict]
    FOREIGN KEY ([idChannel])
    REFERENCES [dbo].[Channels]
        ([idChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelConflict'
CREATE INDEX [IX_FK_ChannelConflict]
ON [dbo].[Conflicts]
    ([idChannel]);
GO

-- Creating foreign key on [idSchedule] in table 'Conflicts'
ALTER TABLE [dbo].[Conflicts]
ADD CONSTRAINT [FK_ScheduleConflict]
    FOREIGN KEY ([idSchedule])
    REFERENCES [dbo].[Schedules]
        ([id_Schedule])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleConflict'
CREATE INDEX [IX_FK_ScheduleConflict]
ON [dbo].[Conflicts]
    ([idSchedule]);
GO

-- Creating foreign key on [idConflictingSchedule] in table 'Conflicts'
ALTER TABLE [dbo].[Conflicts]
ADD CONSTRAINT [FK_ScheduleConflict1]
    FOREIGN KEY ([idConflictingSchedule])
    REFERENCES [dbo].[Schedules]
        ([id_Schedule])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleConflict1'
CREATE INDEX [IX_FK_ScheduleConflict1]
ON [dbo].[Conflicts]
    ([idConflictingSchedule]);
GO

-- Creating foreign key on [idProgramCategory] in table 'Histories'
ALTER TABLE [dbo].[Histories]
ADD CONSTRAINT [FK_ProgramCategoryHistory]
    FOREIGN KEY ([idProgramCategory])
    REFERENCES [dbo].[ProgramCategories]
        ([idProgramCategory])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramCategoryHistory'
CREATE INDEX [IX_FK_ProgramCategoryHistory]
ON [dbo].[Histories]
    ([idProgramCategory]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------