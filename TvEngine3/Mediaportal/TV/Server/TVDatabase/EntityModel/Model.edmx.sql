
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 09/27/2012 08:03:00
-- Generated from EDMX file: C:\Temp\MP\MP1-Tve35\MediaPortal-MediaPortal-1-96d4203\TvEngine3\Mediaportal\TV\Server\TVDatabase\EntityModel\Model.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [tve35];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_GroupMapChannelGroup]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GroupMaps] DROP CONSTRAINT [FK_GroupMapChannelGroup];
GO
IF OBJECT_ID(N'[dbo].[FK_GroupMapChannel]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GroupMaps] DROP CONSTRAINT [FK_GroupMapChannel];
GO
IF OBJECT_ID(N'[dbo].[FK_CardGroupMapCard]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CardGroupMaps] DROP CONSTRAINT [FK_CardGroupMapCard];
GO
IF OBJECT_ID(N'[dbo].[FK_DisEqcMotorCard]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DisEqcMotors] DROP CONSTRAINT [FK_DisEqcMotorCard];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelRecording]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Recordings] DROP CONSTRAINT [FK_ChannelRecording];
GO
IF OBJECT_ID(N'[dbo].[FK_CardGroupMapCardGroup]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CardGroupMaps] DROP CONSTRAINT [FK_CardGroupMapCardGroup];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelProgram]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Programs] DROP CONSTRAINT [FK_ChannelProgram];
GO
IF OBJECT_ID(N'[dbo].[FK_CardChannelMap]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ChannelMaps] DROP CONSTRAINT [FK_CardChannelMap];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelChannelMap]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ChannelMaps] DROP CONSTRAINT [FK_ChannelChannelMap];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelSchedule]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Schedules] DROP CONSTRAINT [FK_ChannelSchedule];
GO
IF OBJECT_ID(N'[dbo].[FK_ScheduleParentSchedule]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Schedules] DROP CONSTRAINT [FK_ScheduleParentSchedule];
GO
IF OBJECT_ID(N'[dbo].[FK_ProgramProgramCategory]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Programs] DROP CONSTRAINT [FK_ProgramProgramCategory];
GO
IF OBJECT_ID(N'[dbo].[FK_ProgramProgramCredit]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProgramCredits] DROP CONSTRAINT [FK_ProgramProgramCredit];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelHistory]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Histories] DROP CONSTRAINT [FK_ChannelHistory];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelTuningDetail]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TuningDetails] DROP CONSTRAINT [FK_ChannelTuningDetail];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelTvMovieMapping]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TvMovieMappings] DROP CONSTRAINT [FK_ChannelTvMovieMapping];
GO
IF OBJECT_ID(N'[dbo].[FK_DisEqcMotorSatellite]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DisEqcMotors] DROP CONSTRAINT [FK_DisEqcMotorSatellite];
GO
IF OBJECT_ID(N'[dbo].[FK_ProgramPersonalTVGuideMap]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PersonalTVGuideMaps] DROP CONSTRAINT [FK_ProgramPersonalTVGuideMap];
GO
IF OBJECT_ID(N'[dbo].[FK_KeywordPersonalTVGuideMap]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PersonalTVGuideMaps] DROP CONSTRAINT [FK_KeywordPersonalTVGuideMap];
GO
IF OBJECT_ID(N'[dbo].[FK_ScheduleRecording]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Recordings] DROP CONSTRAINT [FK_ScheduleRecording];
GO
IF OBJECT_ID(N'[dbo].[FK_KeywordKeywordMap]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[KeywordMaps] DROP CONSTRAINT [FK_KeywordKeywordMap];
GO
IF OBJECT_ID(N'[dbo].[FK_KeywordMapChannelGroup]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[KeywordMaps] DROP CONSTRAINT [FK_KeywordMapChannelGroup];
GO
IF OBJECT_ID(N'[dbo].[FK_RecordingRecordingCredit]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RecordingCredits] DROP CONSTRAINT [FK_RecordingRecordingCredit];
GO
IF OBJECT_ID(N'[dbo].[FK_ScheduleCanceledSchedule]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CanceledSchedules] DROP CONSTRAINT [FK_ScheduleCanceledSchedule];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelLinkMap]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ChannelLinkageMaps] DROP CONSTRAINT [FK_ChannelLinkMap];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelPortalMap]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ChannelLinkageMaps] DROP CONSTRAINT [FK_ChannelPortalMap];
GO
IF OBJECT_ID(N'[dbo].[FK_RecordingProgramCategory]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Recordings] DROP CONSTRAINT [FK_RecordingProgramCategory];
GO
IF OBJECT_ID(N'[dbo].[FK_CardConflict]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Conflicts] DROP CONSTRAINT [FK_CardConflict];
GO
IF OBJECT_ID(N'[dbo].[FK_ChannelConflict]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Conflicts] DROP CONSTRAINT [FK_ChannelConflict];
GO
IF OBJECT_ID(N'[dbo].[FK_ScheduleConflict]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Conflicts] DROP CONSTRAINT [FK_ScheduleConflict];
GO
IF OBJECT_ID(N'[dbo].[FK_ScheduleConflict1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Conflicts] DROP CONSTRAINT [FK_ScheduleConflict1];
GO
IF OBJECT_ID(N'[dbo].[FK_ProgramCategoryHistory]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Histories] DROP CONSTRAINT [FK_ProgramCategoryHistory];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[CanceledSchedules]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CanceledSchedules];
GO
IF OBJECT_ID(N'[dbo].[Cards]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Cards];
GO
IF OBJECT_ID(N'[dbo].[CardGroups]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CardGroups];
GO
IF OBJECT_ID(N'[dbo].[CardGroupMaps]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CardGroupMaps];
GO
IF OBJECT_ID(N'[dbo].[Channels]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Channels];
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
IF OBJECT_ID(N'[dbo].[Keywords]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Keywords];
GO
IF OBJECT_ID(N'[dbo].[KeywordMaps]', 'U') IS NOT NULL
    DROP TABLE [dbo].[KeywordMaps];
GO
IF OBJECT_ID(N'[dbo].[PendingDeletions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PendingDeletions];
GO
IF OBJECT_ID(N'[dbo].[PersonalTVGuideMaps]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PersonalTVGuideMaps];
GO
IF OBJECT_ID(N'[dbo].[Programs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Programs];
GO
IF OBJECT_ID(N'[dbo].[ProgramCategories]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProgramCategories];
GO
IF OBJECT_ID(N'[dbo].[ProgramCredits]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProgramCredits];
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
IF OBJECT_ID(N'[dbo].[Schedules]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Schedules];
GO
IF OBJECT_ID(N'[dbo].[ScheduleRulesTemplates]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ScheduleRulesTemplates];
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
IF OBJECT_ID(N'[dbo].[RecordingCredits]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RecordingCredits];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'CanceledSchedules'
CREATE TABLE [dbo].[CanceledSchedules] (
    [IdCanceledSchedule] int IDENTITY(1,1) NOT NULL,
    [IdSchedule] int  NOT NULL,
    [IdChannel] int  NOT NULL,
    [CancelDateTime] datetime  NOT NULL
);
GO

-- Creating table 'Cards'
CREATE TABLE [dbo].[Cards] (
    [IdCard] int IDENTITY(1,1) NOT NULL,
    [DevicePath] varchar(2000)  NOT NULL,
    [Name] varchar(200)  NOT NULL,
    [Priority] int  NOT NULL,
    [GrabEPG] bit  NOT NULL,
    [LastEpgGrab] datetime  NULL,
    [RecordingFolder] varchar(256)  NOT NULL,
    [Enabled] bit  NOT NULL,
    [CamType] int  NOT NULL,
    [TimeshiftingFolder] varchar(256)  NOT NULL,
    [RecordingFormat] int  NOT NULL,
    [DecryptLimit] int  NOT NULL,
    [Preload] bit  NOT NULL,
    [CAM] bit  NOT NULL,
    [NetProvider] int  NOT NULL,
    [StopGraph] bit  NOT NULL
);
GO

-- Creating table 'CardGroups'
CREATE TABLE [dbo].[CardGroups] (
    [IdCardGroup] int IDENTITY(1,1) NOT NULL,
    [Name] varchar(255)  NOT NULL
);
GO

-- Creating table 'CardGroupMaps'
CREATE TABLE [dbo].[CardGroupMaps] (
    [IdMapping] int IDENTITY(1,1) NOT NULL,
    [IdCard] int  NOT NULL,
    [IdCardGroup] int  NOT NULL
);
GO

-- Creating table 'Channels'
CREATE TABLE [dbo].[Channels] (
    [IdChannel] int IDENTITY(1,1) NOT NULL,
    [TimesWatched] int  NOT NULL,
    [TotalTimeWatched] datetime  NULL,
    [GrabEpg] bit  NOT NULL,
    [LastGrabTime] datetime  NULL,
    [SortOrder] int  NOT NULL,
    [VisibleInGuide] bit  NOT NULL,
    [ExternalId] varchar(200)  NULL,
    [DisplayName] varchar(200)  NOT NULL,
    [EpgHasGaps] bit  NOT NULL,
    [MediaType] int  NOT NULL
);
GO

-- Creating table 'ChannelGroups'
CREATE TABLE [dbo].[ChannelGroups] (
    [IdGroup] int IDENTITY(1,1) NOT NULL,
    [GroupName] varchar(200)  NOT NULL,
    [SortOrder] int  NOT NULL
);
GO

-- Creating table 'ChannelLinkageMaps'
CREATE TABLE [dbo].[ChannelLinkageMaps] (
    [IdMapping] int IDENTITY(1,1) NOT NULL,
    [IdPortalChannel] int  NOT NULL,
    [IdLinkedChannel] int  NOT NULL,
    [DisplayName] varchar(200)  NOT NULL
);
GO

-- Creating table 'ChannelMaps'
CREATE TABLE [dbo].[ChannelMaps] (
    [IdChannelMap] int IDENTITY(1,1) NOT NULL,
    [IdChannel] int  NOT NULL,
    [IdCard] int  NOT NULL,
    [EpgOnly] bit  NOT NULL
);
GO

-- Creating table 'Conflicts'
CREATE TABLE [dbo].[Conflicts] (
    [IdConflict] int IDENTITY(1,1) NOT NULL,
    [IdSchedule] int  NOT NULL,
    [IdConflictingSchedule] int  NOT NULL,
    [IdChannel] int  NOT NULL,
    [ConflictDate] datetime  NOT NULL,
    [IdCard] int  NULL
);
GO

-- Creating table 'DisEqcMotors'
CREATE TABLE [dbo].[DisEqcMotors] (
    [IdDiSEqCMotor] int IDENTITY(1,1) NOT NULL,
    [IdCard] int  NOT NULL,
    [IdSatellite] int  NOT NULL,
    [Position] int  NOT NULL
);
GO

-- Creating table 'Favorites'
CREATE TABLE [dbo].[Favorites] (
    [IdFavorite] int IDENTITY(1,1) NOT NULL,
    [IdProgram] int  NOT NULL,
    [Priority] int  NOT NULL,
    [TimesWatched] int  NOT NULL
);
GO

-- Creating table 'GroupMaps'
CREATE TABLE [dbo].[GroupMaps] (
    [IdMap] int IDENTITY(1,1) NOT NULL,
    [IdGroup] int  NOT NULL,
    [IdChannel] int  NOT NULL,
    [SortOrder] int  NOT NULL,
    [MediaType] int  NOT NULL
);
GO

-- Creating table 'Histories'
CREATE TABLE [dbo].[Histories] (
    [IdHistory] int IDENTITY(1,1) NOT NULL,
    [IdChannel] int  NOT NULL,
    [StartTime] datetime  NOT NULL,
    [EndTime] datetime  NOT NULL,
    [Title] varchar(1000)  NOT NULL,
    [Description] varchar(1000)  NOT NULL,
    [Recorded] bit  NOT NULL,
    [Watched] int  NOT NULL,
    [IdProgramCategory] int  NULL
);
GO

-- Creating table 'Keywords'
CREATE TABLE [dbo].[Keywords] (
    [IdKeyword] int IDENTITY(1,1) NOT NULL,
    [KeywordName] varchar(200)  NOT NULL,
    [Rating] int  NOT NULL,
    [AutoRecord] bit  NOT NULL,
    [SearchIn] int  NOT NULL
);
GO

-- Creating table 'KeywordMaps'
CREATE TABLE [dbo].[KeywordMaps] (
    [IdKeywordMap] int IDENTITY(1,1) NOT NULL,
    [IdKeyword] int  NOT NULL,
    [IdChannelGroup] int  NOT NULL
);
GO

-- Creating table 'PendingDeletions'
CREATE TABLE [dbo].[PendingDeletions] (
    [IdPendingDeletion] int IDENTITY(1,1) NOT NULL,
    [FileName] varchar(max)  NOT NULL
);
GO

-- Creating table 'PersonalTVGuideMaps'
CREATE TABLE [dbo].[PersonalTVGuideMaps] (
    [IdPersonalTVGuideMap] int IDENTITY(1,1) NOT NULL,
    [IdKeyword] int  NOT NULL,
    [IdProgram] int  NOT NULL
);
GO

-- Creating table 'Programs'
CREATE TABLE [dbo].[Programs] (
    [IdProgram] int IDENTITY(1,1) NOT NULL,
    [IdChannel] int  NOT NULL,
    [StartTime] datetime  NOT NULL,
    [EndTime] datetime  NOT NULL,
    [Title] varchar(2000)  NOT NULL,
    [Description] varchar(8000)  NOT NULL,
    [SeriesNum] varchar(200)  NOT NULL,
    [EpisodeNum] varchar(200)  NOT NULL,
    [OriginalAirDate] datetime  NULL,
    [Classification] varchar(200)  NOT NULL,
    [StarRating] int  NOT NULL,
    [ParentalRating] int  NOT NULL,
    [EpisodeName] nvarchar(max)  NOT NULL,
    [EpisodePart] nvarchar(max)  NOT NULL,
    [State] int  NOT NULL,
    [PreviouslyShown] bit  NOT NULL,
    [IdProgramCategory] int  NULL,
    [StartTimeDayOfWeek] smallint  NOT NULL,
    [EndTimeDayOfWeek] smallint  NOT NULL,
    [EndTimeOffset] datetime  NOT NULL,
    [StartTimeOffset] datetime  NOT NULL
);
GO

-- Creating table 'ProgramCategories'
CREATE TABLE [dbo].[ProgramCategories] (
    [IdProgramCategory] int IDENTITY(1,1) NOT NULL,
    [Category] varchar(50)  NOT NULL
);
GO

-- Creating table 'ProgramCredits'
CREATE TABLE [dbo].[ProgramCredits] (
    [IdProgramCredit] int IDENTITY(1,1) NOT NULL,
    [IdProgram] int  NOT NULL,
    [Person] varchar(200)  NOT NULL,
    [Role] varchar(50)  NOT NULL
);
GO

-- Creating table 'Recordings'
CREATE TABLE [dbo].[Recordings] (
    [IdRecording] int IDENTITY(1,1) NOT NULL,
    [IdChannel] int  NULL,
    [StartTime] datetime  NOT NULL,
    [EndTime] datetime  NOT NULL,
    [Title] varchar(2000)  NOT NULL,
    [Description] varchar(8000)  NOT NULL,
    [FileName] varchar(260)  NOT NULL,
    [KeepUntil] int  NOT NULL,
    [KeepUntilDate] datetime  NULL,
    [TimesWatched] int  NOT NULL,
    [StopTime] int  NOT NULL,
    [EpisodeName] varchar(max)  NOT NULL,
    [SeriesNum] varchar(200)  NOT NULL,
    [EpisodeNum] varchar(200)  NOT NULL,
    [EpisodePart] varchar(max)  NOT NULL,
    [IsRecording] bit  NOT NULL,
    [IdSchedule] int  NULL,
    [MediaType] int  NOT NULL,
    [IdProgramCategory] int  NULL
);
GO

-- Creating table 'RuleBasedSchedules'
CREATE TABLE [dbo].[RuleBasedSchedules] (
    [IdRuleBasedSchedule] int IDENTITY(1,1) NOT NULL,
    [ScheduleName] varchar(256)  NOT NULL,
    [MaxAirings] int  NOT NULL,
    [Priority] int  NOT NULL,
    [Directory] varchar(1024)  NOT NULL,
    [Quality] int  NOT NULL,
    [KeepMethod] int  NOT NULL,
    [KeepDate] datetime  NULL,
    [PreRecordInterval] int  NOT NULL,
    [PostRecordInterval] int  NOT NULL,
    [Rules] varchar(max)  NULL
);
GO

-- Creating table 'Satellites'
CREATE TABLE [dbo].[Satellites] (
    [IdSatellite] int IDENTITY(1,1) NOT NULL,
    [SatelliteName] varchar(200)  NOT NULL,
    [TransponderFileName] varchar(200)  NOT NULL
);
GO

-- Creating table 'Schedules'
CREATE TABLE [dbo].[Schedules] (
    [IdSchedule] int IDENTITY(1,1) NOT NULL,
    [IdChannel] int  NOT NULL,
    [ScheduleType] int  NOT NULL,
    [ProgramName] varchar(256)  NOT NULL,
    [StartTime] datetime  NOT NULL,
    [EndTime] datetime  NOT NULL,
    [MaxAirings] int  NOT NULL,
    [Priority] int  NOT NULL,
    [Directory] varchar(1024)  NOT NULL,
    [Quality] int  NOT NULL,
    [KeepMethod] int  NOT NULL,
    [KeepDate] datetime  NULL,
    [PreRecordInterval] int  NOT NULL,
    [PostRecordInterval] int  NOT NULL,
    [Canceled] datetime  NOT NULL,
    [Series] bit  NOT NULL,
    [IdParentSchedule] int  NULL
);
GO

-- Creating table 'ScheduleRulesTemplates'
CREATE TABLE [dbo].[ScheduleRulesTemplates] (
    [IdScheduleRulesTemplate] int IDENTITY(1,1) NOT NULL,
    [Name] varchar(50)  NOT NULL,
    [Rules] varchar(max)  NOT NULL,
    [Enabled] bit  NOT NULL,
    [Usages] int  NOT NULL,
    [Editable] bit  NOT NULL
);
GO

-- Creating table 'Settings'
CREATE TABLE [dbo].[Settings] (
    [IdSetting] int IDENTITY(1,1) NOT NULL,
    [Tag] varchar(200)  NOT NULL,
    [Value] varchar(4096)  NOT NULL
);
GO

-- Creating table 'SoftwareEncoders'
CREATE TABLE [dbo].[SoftwareEncoders] (
    [IdEncoder] int IDENTITY(1,1) NOT NULL,
    [Priority] int  NOT NULL,
    [Name] varchar(200)  NOT NULL,
    [Type] int  NOT NULL,
    [Reusable] bit  NOT NULL
);
GO

-- Creating table 'Timespans'
CREATE TABLE [dbo].[Timespans] (
    [IdTimespan] int IDENTITY(1,1) NOT NULL,
    [StartTime] datetime  NOT NULL,
    [EndTime] datetime  NOT NULL,
    [DayOfWeek] int  NOT NULL,
    [IdKeyword] int  NOT NULL
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
    [IdMapping] int IDENTITY(1,1) NOT NULL,
    [IdChannel] int  NOT NULL,
    [StationName] varchar(200)  NOT NULL,
    [TimeSharingStart] varchar(200)  NOT NULL,
    [TimeSharingEnd] varchar(200)  NOT NULL
);
GO

-- Creating table 'Versions'
CREATE TABLE [dbo].[Versions] (
    [IdVersion] int IDENTITY(1,1) NOT NULL,
    [VersionNumber] int  NOT NULL
);
GO

-- Creating table 'RecordingCredits'
CREATE TABLE [dbo].[RecordingCredits] (
    [IdRecordingCredit] int IDENTITY(1,1) NOT NULL,
    [IdRecording] int  NOT NULL,
    [Person] varchar(200)  NOT NULL,
    [Role] varchar(50)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [IdCanceledSchedule] in table 'CanceledSchedules'
ALTER TABLE [dbo].[CanceledSchedules]
ADD CONSTRAINT [PK_CanceledSchedules]
    PRIMARY KEY CLUSTERED ([IdCanceledSchedule] ASC);
GO

-- Creating primary key on [IdCard] in table 'Cards'
ALTER TABLE [dbo].[Cards]
ADD CONSTRAINT [PK_Cards]
    PRIMARY KEY CLUSTERED ([IdCard] ASC);
GO

-- Creating primary key on [IdCardGroup] in table 'CardGroups'
ALTER TABLE [dbo].[CardGroups]
ADD CONSTRAINT [PK_CardGroups]
    PRIMARY KEY CLUSTERED ([IdCardGroup] ASC);
GO

-- Creating primary key on [IdMapping] in table 'CardGroupMaps'
ALTER TABLE [dbo].[CardGroupMaps]
ADD CONSTRAINT [PK_CardGroupMaps]
    PRIMARY KEY CLUSTERED ([IdMapping] ASC);
GO

-- Creating primary key on [IdChannel] in table 'Channels'
ALTER TABLE [dbo].[Channels]
ADD CONSTRAINT [PK_Channels]
    PRIMARY KEY CLUSTERED ([IdChannel] ASC);
GO

-- Creating primary key on [IdGroup] in table 'ChannelGroups'
ALTER TABLE [dbo].[ChannelGroups]
ADD CONSTRAINT [PK_ChannelGroups]
    PRIMARY KEY CLUSTERED ([IdGroup] ASC);
GO

-- Creating primary key on [IdMapping] in table 'ChannelLinkageMaps'
ALTER TABLE [dbo].[ChannelLinkageMaps]
ADD CONSTRAINT [PK_ChannelLinkageMaps]
    PRIMARY KEY CLUSTERED ([IdMapping] ASC);
GO

-- Creating primary key on [IdChannelMap] in table 'ChannelMaps'
ALTER TABLE [dbo].[ChannelMaps]
ADD CONSTRAINT [PK_ChannelMaps]
    PRIMARY KEY CLUSTERED ([IdChannelMap] ASC);
GO

-- Creating primary key on [IdConflict] in table 'Conflicts'
ALTER TABLE [dbo].[Conflicts]
ADD CONSTRAINT [PK_Conflicts]
    PRIMARY KEY CLUSTERED ([IdConflict] ASC);
GO

-- Creating primary key on [IdDiSEqCMotor] in table 'DisEqcMotors'
ALTER TABLE [dbo].[DisEqcMotors]
ADD CONSTRAINT [PK_DisEqcMotors]
    PRIMARY KEY CLUSTERED ([IdDiSEqCMotor] ASC);
GO

-- Creating primary key on [IdFavorite] in table 'Favorites'
ALTER TABLE [dbo].[Favorites]
ADD CONSTRAINT [PK_Favorites]
    PRIMARY KEY CLUSTERED ([IdFavorite] ASC);
GO

-- Creating primary key on [IdMap] in table 'GroupMaps'
ALTER TABLE [dbo].[GroupMaps]
ADD CONSTRAINT [PK_GroupMaps]
    PRIMARY KEY CLUSTERED ([IdMap] ASC);
GO

-- Creating primary key on [IdHistory] in table 'Histories'
ALTER TABLE [dbo].[Histories]
ADD CONSTRAINT [PK_Histories]
    PRIMARY KEY CLUSTERED ([IdHistory] ASC);
GO

-- Creating primary key on [IdKeyword] in table 'Keywords'
ALTER TABLE [dbo].[Keywords]
ADD CONSTRAINT [PK_Keywords]
    PRIMARY KEY CLUSTERED ([IdKeyword] ASC);
GO

-- Creating primary key on [IdKeywordMap] in table 'KeywordMaps'
ALTER TABLE [dbo].[KeywordMaps]
ADD CONSTRAINT [PK_KeywordMaps]
    PRIMARY KEY CLUSTERED ([IdKeywordMap] ASC);
GO

-- Creating primary key on [IdPendingDeletion] in table 'PendingDeletions'
ALTER TABLE [dbo].[PendingDeletions]
ADD CONSTRAINT [PK_PendingDeletions]
    PRIMARY KEY CLUSTERED ([IdPendingDeletion] ASC);
GO

-- Creating primary key on [IdPersonalTVGuideMap] in table 'PersonalTVGuideMaps'
ALTER TABLE [dbo].[PersonalTVGuideMaps]
ADD CONSTRAINT [PK_PersonalTVGuideMaps]
    PRIMARY KEY CLUSTERED ([IdPersonalTVGuideMap] ASC);
GO

-- Creating primary key on [IdProgram] in table 'Programs'
ALTER TABLE [dbo].[Programs]
ADD CONSTRAINT [PK_Programs]
    PRIMARY KEY CLUSTERED ([IdProgram] ASC);
GO

-- Creating primary key on [IdProgramCategory] in table 'ProgramCategories'
ALTER TABLE [dbo].[ProgramCategories]
ADD CONSTRAINT [PK_ProgramCategories]
    PRIMARY KEY CLUSTERED ([IdProgramCategory] ASC);
GO

-- Creating primary key on [IdProgramCredit] in table 'ProgramCredits'
ALTER TABLE [dbo].[ProgramCredits]
ADD CONSTRAINT [PK_ProgramCredits]
    PRIMARY KEY CLUSTERED ([IdProgramCredit] ASC);
GO

-- Creating primary key on [IdRecording] in table 'Recordings'
ALTER TABLE [dbo].[Recordings]
ADD CONSTRAINT [PK_Recordings]
    PRIMARY KEY CLUSTERED ([IdRecording] ASC);
GO

-- Creating primary key on [IdRuleBasedSchedule] in table 'RuleBasedSchedules'
ALTER TABLE [dbo].[RuleBasedSchedules]
ADD CONSTRAINT [PK_RuleBasedSchedules]
    PRIMARY KEY CLUSTERED ([IdRuleBasedSchedule] ASC);
GO

-- Creating primary key on [IdSatellite] in table 'Satellites'
ALTER TABLE [dbo].[Satellites]
ADD CONSTRAINT [PK_Satellites]
    PRIMARY KEY CLUSTERED ([IdSatellite] ASC);
GO

-- Creating primary key on [IdSchedule] in table 'Schedules'
ALTER TABLE [dbo].[Schedules]
ADD CONSTRAINT [PK_Schedules]
    PRIMARY KEY CLUSTERED ([IdSchedule] ASC);
GO

-- Creating primary key on [IdScheduleRulesTemplate] in table 'ScheduleRulesTemplates'
ALTER TABLE [dbo].[ScheduleRulesTemplates]
ADD CONSTRAINT [PK_ScheduleRulesTemplates]
    PRIMARY KEY CLUSTERED ([IdScheduleRulesTemplate] ASC);
GO

-- Creating primary key on [IdSetting] in table 'Settings'
ALTER TABLE [dbo].[Settings]
ADD CONSTRAINT [PK_Settings]
    PRIMARY KEY CLUSTERED ([IdSetting] ASC);
GO

-- Creating primary key on [IdEncoder] in table 'SoftwareEncoders'
ALTER TABLE [dbo].[SoftwareEncoders]
ADD CONSTRAINT [PK_SoftwareEncoders]
    PRIMARY KEY CLUSTERED ([IdEncoder] ASC);
GO

-- Creating primary key on [IdTimespan] in table 'Timespans'
ALTER TABLE [dbo].[Timespans]
ADD CONSTRAINT [PK_Timespans]
    PRIMARY KEY CLUSTERED ([IdTimespan] ASC);
GO

-- Creating primary key on [idTuning] in table 'TuningDetails'
ALTER TABLE [dbo].[TuningDetails]
ADD CONSTRAINT [PK_TuningDetails]
    PRIMARY KEY CLUSTERED ([idTuning] ASC);
GO

-- Creating primary key on [IdMapping] in table 'TvMovieMappings'
ALTER TABLE [dbo].[TvMovieMappings]
ADD CONSTRAINT [PK_TvMovieMappings]
    PRIMARY KEY CLUSTERED ([IdMapping] ASC);
GO

-- Creating primary key on [IdVersion] in table 'Versions'
ALTER TABLE [dbo].[Versions]
ADD CONSTRAINT [PK_Versions]
    PRIMARY KEY CLUSTERED ([IdVersion] ASC);
GO

-- Creating primary key on [IdRecordingCredit] in table 'RecordingCredits'
ALTER TABLE [dbo].[RecordingCredits]
ADD CONSTRAINT [PK_RecordingCredits]
    PRIMARY KEY CLUSTERED ([IdRecordingCredit] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [IdGroup] in table 'GroupMaps'
ALTER TABLE [dbo].[GroupMaps]
ADD CONSTRAINT [FK_GroupMapChannelGroup]
    FOREIGN KEY ([IdGroup])
    REFERENCES [dbo].[ChannelGroups]
        ([IdGroup])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GroupMapChannelGroup'
CREATE INDEX [IX_FK_GroupMapChannelGroup]
ON [dbo].[GroupMaps]
    ([IdGroup]);
GO

-- Creating foreign key on [IdChannel] in table 'GroupMaps'
ALTER TABLE [dbo].[GroupMaps]
ADD CONSTRAINT [FK_GroupMapChannel]
    FOREIGN KEY ([IdChannel])
    REFERENCES [dbo].[Channels]
        ([IdChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GroupMapChannel'
CREATE INDEX [IX_FK_GroupMapChannel]
ON [dbo].[GroupMaps]
    ([IdChannel]);
GO

-- Creating foreign key on [IdCard] in table 'CardGroupMaps'
ALTER TABLE [dbo].[CardGroupMaps]
ADD CONSTRAINT [FK_CardGroupMapCard]
    FOREIGN KEY ([IdCard])
    REFERENCES [dbo].[Cards]
        ([IdCard])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardGroupMapCard'
CREATE INDEX [IX_FK_CardGroupMapCard]
ON [dbo].[CardGroupMaps]
    ([IdCard]);
GO

-- Creating foreign key on [IdCard] in table 'DisEqcMotors'
ALTER TABLE [dbo].[DisEqcMotors]
ADD CONSTRAINT [FK_DisEqcMotorCard]
    FOREIGN KEY ([IdCard])
    REFERENCES [dbo].[Cards]
        ([IdCard])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DisEqcMotorCard'
CREATE INDEX [IX_FK_DisEqcMotorCard]
ON [dbo].[DisEqcMotors]
    ([IdCard]);
GO

-- Creating foreign key on [IdChannel] in table 'Recordings'
ALTER TABLE [dbo].[Recordings]
ADD CONSTRAINT [FK_ChannelRecording]
    FOREIGN KEY ([IdChannel])
    REFERENCES [dbo].[Channels]
        ([IdChannel])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelRecording'
CREATE INDEX [IX_FK_ChannelRecording]
ON [dbo].[Recordings]
    ([IdChannel]);
GO

-- Creating foreign key on [IdCardGroup] in table 'CardGroupMaps'
ALTER TABLE [dbo].[CardGroupMaps]
ADD CONSTRAINT [FK_CardGroupMapCardGroup]
    FOREIGN KEY ([IdCardGroup])
    REFERENCES [dbo].[CardGroups]
        ([IdCardGroup])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardGroupMapCardGroup'
CREATE INDEX [IX_FK_CardGroupMapCardGroup]
ON [dbo].[CardGroupMaps]
    ([IdCardGroup]);
GO

-- Creating foreign key on [IdChannel] in table 'Programs'
ALTER TABLE [dbo].[Programs]
ADD CONSTRAINT [FK_ChannelProgram]
    FOREIGN KEY ([IdChannel])
    REFERENCES [dbo].[Channels]
        ([IdChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelProgram'
CREATE INDEX [IX_FK_ChannelProgram]
ON [dbo].[Programs]
    ([IdChannel]);
GO

-- Creating foreign key on [IdCard] in table 'ChannelMaps'
ALTER TABLE [dbo].[ChannelMaps]
ADD CONSTRAINT [FK_CardChannelMap]
    FOREIGN KEY ([IdCard])
    REFERENCES [dbo].[Cards]
        ([IdCard])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardChannelMap'
CREATE INDEX [IX_FK_CardChannelMap]
ON [dbo].[ChannelMaps]
    ([IdCard]);
GO

-- Creating foreign key on [IdChannel] in table 'ChannelMaps'
ALTER TABLE [dbo].[ChannelMaps]
ADD CONSTRAINT [FK_ChannelChannelMap]
    FOREIGN KEY ([IdChannel])
    REFERENCES [dbo].[Channels]
        ([IdChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelChannelMap'
CREATE INDEX [IX_FK_ChannelChannelMap]
ON [dbo].[ChannelMaps]
    ([IdChannel]);
GO

-- Creating foreign key on [IdChannel] in table 'Schedules'
ALTER TABLE [dbo].[Schedules]
ADD CONSTRAINT [FK_ChannelSchedule]
    FOREIGN KEY ([IdChannel])
    REFERENCES [dbo].[Channels]
        ([IdChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelSchedule'
CREATE INDEX [IX_FK_ChannelSchedule]
ON [dbo].[Schedules]
    ([IdChannel]);
GO

-- Creating foreign key on [IdParentSchedule] in table 'Schedules'
ALTER TABLE [dbo].[Schedules]
ADD CONSTRAINT [FK_ScheduleParentSchedule]
    FOREIGN KEY ([IdParentSchedule])
    REFERENCES [dbo].[Schedules]
        ([IdSchedule])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleParentSchedule'
CREATE INDEX [IX_FK_ScheduleParentSchedule]
ON [dbo].[Schedules]
    ([IdParentSchedule]);
GO

-- Creating foreign key on [IdProgramCategory] in table 'Programs'
ALTER TABLE [dbo].[Programs]
ADD CONSTRAINT [FK_ProgramProgramCategory]
    FOREIGN KEY ([IdProgramCategory])
    REFERENCES [dbo].[ProgramCategories]
        ([IdProgramCategory])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramProgramCategory'
CREATE INDEX [IX_FK_ProgramProgramCategory]
ON [dbo].[Programs]
    ([IdProgramCategory]);
GO

-- Creating foreign key on [IdProgram] in table 'ProgramCredits'
ALTER TABLE [dbo].[ProgramCredits]
ADD CONSTRAINT [FK_ProgramProgramCredit]
    FOREIGN KEY ([IdProgram])
    REFERENCES [dbo].[Programs]
        ([IdProgram])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramProgramCredit'
CREATE INDEX [IX_FK_ProgramProgramCredit]
ON [dbo].[ProgramCredits]
    ([IdProgram]);
GO

-- Creating foreign key on [IdChannel] in table 'Histories'
ALTER TABLE [dbo].[Histories]
ADD CONSTRAINT [FK_ChannelHistory]
    FOREIGN KEY ([IdChannel])
    REFERENCES [dbo].[Channels]
        ([IdChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelHistory'
CREATE INDEX [IX_FK_ChannelHistory]
ON [dbo].[Histories]
    ([IdChannel]);
GO

-- Creating foreign key on [idChannel] in table 'TuningDetails'
ALTER TABLE [dbo].[TuningDetails]
ADD CONSTRAINT [FK_ChannelTuningDetail]
    FOREIGN KEY ([idChannel])
    REFERENCES [dbo].[Channels]
        ([IdChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelTuningDetail'
CREATE INDEX [IX_FK_ChannelTuningDetail]
ON [dbo].[TuningDetails]
    ([idChannel]);
GO

-- Creating foreign key on [IdChannel] in table 'TvMovieMappings'
ALTER TABLE [dbo].[TvMovieMappings]
ADD CONSTRAINT [FK_ChannelTvMovieMapping]
    FOREIGN KEY ([IdChannel])
    REFERENCES [dbo].[Channels]
        ([IdChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelTvMovieMapping'
CREATE INDEX [IX_FK_ChannelTvMovieMapping]
ON [dbo].[TvMovieMappings]
    ([IdChannel]);
GO

-- Creating foreign key on [IdSatellite] in table 'DisEqcMotors'
ALTER TABLE [dbo].[DisEqcMotors]
ADD CONSTRAINT [FK_DisEqcMotorSatellite]
    FOREIGN KEY ([IdSatellite])
    REFERENCES [dbo].[Satellites]
        ([IdSatellite])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DisEqcMotorSatellite'
CREATE INDEX [IX_FK_DisEqcMotorSatellite]
ON [dbo].[DisEqcMotors]
    ([IdSatellite]);
GO

-- Creating foreign key on [IdProgram] in table 'PersonalTVGuideMaps'
ALTER TABLE [dbo].[PersonalTVGuideMaps]
ADD CONSTRAINT [FK_ProgramPersonalTVGuideMap]
    FOREIGN KEY ([IdProgram])
    REFERENCES [dbo].[Programs]
        ([IdProgram])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramPersonalTVGuideMap'
CREATE INDEX [IX_FK_ProgramPersonalTVGuideMap]
ON [dbo].[PersonalTVGuideMaps]
    ([IdProgram]);
GO

-- Creating foreign key on [IdKeyword] in table 'PersonalTVGuideMaps'
ALTER TABLE [dbo].[PersonalTVGuideMaps]
ADD CONSTRAINT [FK_KeywordPersonalTVGuideMap]
    FOREIGN KEY ([IdKeyword])
    REFERENCES [dbo].[Keywords]
        ([IdKeyword])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_KeywordPersonalTVGuideMap'
CREATE INDEX [IX_FK_KeywordPersonalTVGuideMap]
ON [dbo].[PersonalTVGuideMaps]
    ([IdKeyword]);
GO

-- Creating foreign key on [IdSchedule] in table 'Recordings'
ALTER TABLE [dbo].[Recordings]
ADD CONSTRAINT [FK_ScheduleRecording]
    FOREIGN KEY ([IdSchedule])
    REFERENCES [dbo].[Schedules]
        ([IdSchedule])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleRecording'
CREATE INDEX [IX_FK_ScheduleRecording]
ON [dbo].[Recordings]
    ([IdSchedule]);
GO

-- Creating foreign key on [IdKeyword] in table 'KeywordMaps'
ALTER TABLE [dbo].[KeywordMaps]
ADD CONSTRAINT [FK_KeywordKeywordMap]
    FOREIGN KEY ([IdKeyword])
    REFERENCES [dbo].[Keywords]
        ([IdKeyword])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_KeywordKeywordMap'
CREATE INDEX [IX_FK_KeywordKeywordMap]
ON [dbo].[KeywordMaps]
    ([IdKeyword]);
GO

-- Creating foreign key on [IdChannelGroup] in table 'KeywordMaps'
ALTER TABLE [dbo].[KeywordMaps]
ADD CONSTRAINT [FK_KeywordMapChannelGroup]
    FOREIGN KEY ([IdChannelGroup])
    REFERENCES [dbo].[ChannelGroups]
        ([IdGroup])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_KeywordMapChannelGroup'
CREATE INDEX [IX_FK_KeywordMapChannelGroup]
ON [dbo].[KeywordMaps]
    ([IdChannelGroup]);
GO

-- Creating foreign key on [IdRecording] in table 'RecordingCredits'
ALTER TABLE [dbo].[RecordingCredits]
ADD CONSTRAINT [FK_RecordingRecordingCredit]
    FOREIGN KEY ([IdRecording])
    REFERENCES [dbo].[Recordings]
        ([IdRecording])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RecordingRecordingCredit'
CREATE INDEX [IX_FK_RecordingRecordingCredit]
ON [dbo].[RecordingCredits]
    ([IdRecording]);
GO

-- Creating foreign key on [IdSchedule] in table 'CanceledSchedules'
ALTER TABLE [dbo].[CanceledSchedules]
ADD CONSTRAINT [FK_ScheduleCanceledSchedule]
    FOREIGN KEY ([IdSchedule])
    REFERENCES [dbo].[Schedules]
        ([IdSchedule])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleCanceledSchedule'
CREATE INDEX [IX_FK_ScheduleCanceledSchedule]
ON [dbo].[CanceledSchedules]
    ([IdSchedule]);
GO

-- Creating foreign key on [IdLinkedChannel] in table 'ChannelLinkageMaps'
ALTER TABLE [dbo].[ChannelLinkageMaps]
ADD CONSTRAINT [FK_ChannelLinkMap]
    FOREIGN KEY ([IdLinkedChannel])
    REFERENCES [dbo].[Channels]
        ([IdChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelLinkMap'
CREATE INDEX [IX_FK_ChannelLinkMap]
ON [dbo].[ChannelLinkageMaps]
    ([IdLinkedChannel]);
GO

-- Creating foreign key on [IdPortalChannel] in table 'ChannelLinkageMaps'
ALTER TABLE [dbo].[ChannelLinkageMaps]
ADD CONSTRAINT [FK_ChannelPortalMap]
    FOREIGN KEY ([IdPortalChannel])
    REFERENCES [dbo].[Channels]
        ([IdChannel])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelPortalMap'
CREATE INDEX [IX_FK_ChannelPortalMap]
ON [dbo].[ChannelLinkageMaps]
    ([IdPortalChannel]);
GO

-- Creating foreign key on [IdProgramCategory] in table 'Recordings'
ALTER TABLE [dbo].[Recordings]
ADD CONSTRAINT [FK_RecordingProgramCategory]
    FOREIGN KEY ([IdProgramCategory])
    REFERENCES [dbo].[ProgramCategories]
        ([IdProgramCategory])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RecordingProgramCategory'
CREATE INDEX [IX_FK_RecordingProgramCategory]
ON [dbo].[Recordings]
    ([IdProgramCategory]);
GO

-- Creating foreign key on [IdCard] in table 'Conflicts'
ALTER TABLE [dbo].[Conflicts]
ADD CONSTRAINT [FK_CardConflict]
    FOREIGN KEY ([IdCard])
    REFERENCES [dbo].[Cards]
        ([IdCard])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardConflict'
CREATE INDEX [IX_FK_CardConflict]
ON [dbo].[Conflicts]
    ([IdCard]);
GO

-- Creating foreign key on [IdChannel] in table 'Conflicts'
ALTER TABLE [dbo].[Conflicts]
ADD CONSTRAINT [FK_ChannelConflict]
    FOREIGN KEY ([IdChannel])
    REFERENCES [dbo].[Channels]
        ([IdChannel])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelConflict'
CREATE INDEX [IX_FK_ChannelConflict]
ON [dbo].[Conflicts]
    ([IdChannel]);
GO

-- Creating foreign key on [IdSchedule] in table 'Conflicts'
ALTER TABLE [dbo].[Conflicts]
ADD CONSTRAINT [FK_ScheduleConflict]
    FOREIGN KEY ([IdSchedule])
    REFERENCES [dbo].[Schedules]
        ([IdSchedule])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleConflict'
CREATE INDEX [IX_FK_ScheduleConflict]
ON [dbo].[Conflicts]
    ([IdSchedule]);
GO

-- Creating foreign key on [IdConflictingSchedule] in table 'Conflicts'
ALTER TABLE [dbo].[Conflicts]
ADD CONSTRAINT [FK_ScheduleConflict1]
    FOREIGN KEY ([IdConflictingSchedule])
    REFERENCES [dbo].[Schedules]
        ([IdSchedule])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleConflict1'
CREATE INDEX [IX_FK_ScheduleConflict1]
ON [dbo].[Conflicts]
    ([IdConflictingSchedule]);
GO

-- Creating foreign key on [IdProgramCategory] in table 'Histories'
ALTER TABLE [dbo].[Histories]
ADD CONSTRAINT [FK_ProgramCategoryHistory]
    FOREIGN KEY ([IdProgramCategory])
    REFERENCES [dbo].[ProgramCategories]
        ([IdProgramCategory])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramCategoryHistory'
CREATE INDEX [IX_FK_ProgramCategoryHistory]
ON [dbo].[Histories]
    ([IdProgramCategory]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------