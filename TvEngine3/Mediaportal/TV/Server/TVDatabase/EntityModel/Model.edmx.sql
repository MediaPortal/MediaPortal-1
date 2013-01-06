



-- -----------------------------------------------------------
-- Entity Designer DDL Script for MySQL Server 4.1 and higher
-- -----------------------------------------------------------
-- Date Created: 01/05/2013 15:14:27
-- Generated from EDMX file: C:\Development\mediaportal-1-cardres_patch\MediaPortal-1\TvEngine3\Mediaportal\TV\Server\TVDatabase\EntityModel\Model.edmx
-- Target version: 2.0.0.0
-- --------------------------------------------------

DROP DATABASE IF EXISTS `TVE35`;
CREATE DATABASE `TVE35`;
USE `TVE35`;

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- NOTE: if the constraint does not exist, an ignorable error will be reported.
-- --------------------------------------------------

--    ALTER TABLE `GroupMaps` DROP CONSTRAINT `FK_GroupMapChannelGroup`;
--    ALTER TABLE `GroupMaps` DROP CONSTRAINT `FK_GroupMapChannel`;
--    ALTER TABLE `CardGroupMaps` DROP CONSTRAINT `FK_CardGroupMapCard`;
--    ALTER TABLE `DisEqcMotors` DROP CONSTRAINT `FK_DisEqcMotorCard`;
--    ALTER TABLE `Recordings` DROP CONSTRAINT `FK_ChannelRecording`;
--    ALTER TABLE `CardGroupMaps` DROP CONSTRAINT `FK_CardGroupMapCardGroup`;
--    ALTER TABLE `Programs` DROP CONSTRAINT `FK_ChannelProgram`;
--    ALTER TABLE `ChannelMaps` DROP CONSTRAINT `FK_CardChannelMap`;
--    ALTER TABLE `ChannelMaps` DROP CONSTRAINT `FK_ChannelChannelMap`;
--    ALTER TABLE `Schedules` DROP CONSTRAINT `FK_ChannelSchedule`;
--    ALTER TABLE `Schedules` DROP CONSTRAINT `FK_ScheduleParentSchedule`;
--    ALTER TABLE `Programs` DROP CONSTRAINT `FK_ProgramProgramCategory`;
--    ALTER TABLE `ProgramCredits` DROP CONSTRAINT `FK_ProgramProgramCredit`;
--    ALTER TABLE `Histories` DROP CONSTRAINT `FK_ChannelHistory`;
--    ALTER TABLE `TuningDetails` DROP CONSTRAINT `FK_ChannelTuningDetail`;
--    ALTER TABLE `TvMovieMappings` DROP CONSTRAINT `FK_ChannelTvMovieMapping`;
--    ALTER TABLE `DisEqcMotors` DROP CONSTRAINT `FK_DisEqcMotorSatellite`;
--    ALTER TABLE `PersonalTVGuideMaps` DROP CONSTRAINT `FK_ProgramPersonalTVGuideMap`;
--    ALTER TABLE `PersonalTVGuideMaps` DROP CONSTRAINT `FK_KeywordPersonalTVGuideMap`;
--    ALTER TABLE `Recordings` DROP CONSTRAINT `FK_ScheduleRecording`;
--    ALTER TABLE `KeywordMaps` DROP CONSTRAINT `FK_KeywordKeywordMap`;
--    ALTER TABLE `KeywordMaps` DROP CONSTRAINT `FK_KeywordMapChannelGroup`;
--    ALTER TABLE `RecordingCredits` DROP CONSTRAINT `FK_RecordingRecordingCredit`;
--    ALTER TABLE `CanceledSchedules` DROP CONSTRAINT `FK_ScheduleCanceledSchedule`;
--    ALTER TABLE `ChannelLinkageMaps` DROP CONSTRAINT `FK_ChannelLinkMap`;
--    ALTER TABLE `ChannelLinkageMaps` DROP CONSTRAINT `FK_ChannelPortalMap`;
--    ALTER TABLE `Recordings` DROP CONSTRAINT `FK_RecordingProgramCategory`;
--    ALTER TABLE `Conflicts` DROP CONSTRAINT `FK_CardConflict`;
--    ALTER TABLE `Conflicts` DROP CONSTRAINT `FK_ChannelConflict`;
--    ALTER TABLE `Conflicts` DROP CONSTRAINT `FK_ScheduleConflict`;
--    ALTER TABLE `Conflicts` DROP CONSTRAINT `FK_ScheduleConflict1`;
--    ALTER TABLE `Histories` DROP CONSTRAINT `FK_ProgramCategoryHistory`;
--    ALTER TABLE `TuningDetails` DROP CONSTRAINT `FK_LnbTypeTuningDetail`;
--    ALTER TABLE `ProgramCategories` DROP CONSTRAINT `FK_TvGuideCategoryProgramCategory`;

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------
SET foreign_key_checks = 0;
    DROP TABLE IF EXISTS `CanceledSchedules`;
    DROP TABLE IF EXISTS `Cards`;
    DROP TABLE IF EXISTS `CardGroups`;
    DROP TABLE IF EXISTS `CardGroupMaps`;
    DROP TABLE IF EXISTS `Channels`;
    DROP TABLE IF EXISTS `ChannelGroups`;
    DROP TABLE IF EXISTS `ChannelLinkageMaps`;
    DROP TABLE IF EXISTS `ChannelMaps`;
    DROP TABLE IF EXISTS `Conflicts`;
    DROP TABLE IF EXISTS `DisEqcMotors`;
    DROP TABLE IF EXISTS `Favorites`;
    DROP TABLE IF EXISTS `GroupMaps`;
    DROP TABLE IF EXISTS `Histories`;
    DROP TABLE IF EXISTS `Keywords`;
    DROP TABLE IF EXISTS `KeywordMaps`;
    DROP TABLE IF EXISTS `PendingDeletions`;
    DROP TABLE IF EXISTS `PersonalTVGuideMaps`;
    DROP TABLE IF EXISTS `Programs`;
    DROP TABLE IF EXISTS `ProgramCategories`;
    DROP TABLE IF EXISTS `ProgramCredits`;
    DROP TABLE IF EXISTS `Recordings`;
    DROP TABLE IF EXISTS `RuleBasedSchedules`;
    DROP TABLE IF EXISTS `Satellites`;
    DROP TABLE IF EXISTS `Schedules`;
    DROP TABLE IF EXISTS `ScheduleRulesTemplates`;
    DROP TABLE IF EXISTS `Settings`;
    DROP TABLE IF EXISTS `SoftwareEncoders`;
    DROP TABLE IF EXISTS `Timespans`;
    DROP TABLE IF EXISTS `TuningDetails`;
    DROP TABLE IF EXISTS `TvMovieMappings`;
    DROP TABLE IF EXISTS `Versions`;
    DROP TABLE IF EXISTS `LnbTypes`;
    DROP TABLE IF EXISTS `RecordingCredits`;
    DROP TABLE IF EXISTS `TvGuideCategories`;
SET foreign_key_checks = 1;

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'CanceledSchedules'

CREATE TABLE `CanceledSchedules` (
    `IdCanceledSchedule` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdSchedule` int  NOT NULL,
    `IdChannel` int  NOT NULL,
    `CancelDateTime` datetime  NOT NULL
);

-- Creating table 'Cards'

CREATE TABLE `Cards` (
    `IdCard` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `DevicePath` varchar(2000)  NOT NULL,
    `Name` varchar(200)  NOT NULL,
    `Priority` int  NOT NULL,
    `GrabEPG` bit  NOT NULL,
    `LastEpgGrab` datetime  NULL,
    `RecordingFolder` varchar(256)  NOT NULL,
    `Enabled` bit  NOT NULL,
    `CamType` int  NOT NULL,
    `TimeshiftingFolder` varchar(256)  NOT NULL,
    `DecryptLimit` int  NOT NULL,
    `PreloadCard` bit  NOT NULL,
    `NetProvider` int  NOT NULL,
    `IdleMode` int  NOT NULL,
    `MultiChannelDecryptMode` int  NOT NULL,
    `AlwaysSendDiseqcCommands` bit  NOT NULL,
    `DiseqcCommandRepeatCount` int  NOT NULL,
    `PidFilterMode` int  NOT NULL,
    `UseCustomTuning` bit  NOT NULL,
    `UseConditionalAccess` bit  NOT NULL
);

-- Creating table 'CardGroups'

CREATE TABLE `CardGroups` (
    `IdCardGroup` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `Name` varchar(255)  NOT NULL
);

-- Creating table 'CardGroupMaps'

CREATE TABLE `CardGroupMaps` (
    `IdMapping` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdCard` int  NOT NULL,
    `IdCardGroup` int  NOT NULL
);

-- Creating table 'Channels'

CREATE TABLE `Channels` (
    `IdChannel` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `TimesWatched` int  NOT NULL,
    `TotalTimeWatched` datetime  NULL,
    `GrabEpg` bit  NOT NULL,
    `LastGrabTime` datetime  NULL,
    `SortOrder` int  NOT NULL,
    `VisibleInGuide` bit  NOT NULL,
    `ExternalId` varchar(200)  NULL,
    `DisplayName` varchar(200)  NOT NULL,
    `EpgHasGaps` bit  NOT NULL,
    `MediaType` int  NOT NULL,
    `ChannelNumber` int  NOT NULL
);

-- Creating table 'ChannelGroups'

CREATE TABLE `ChannelGroups` (
    `IdGroup` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `GroupName` varchar(200)  NOT NULL,
    `SortOrder` int  NOT NULL,
    `MediaType` int  NOT NULL
);

-- Creating table 'ChannelLinkageMaps'

CREATE TABLE `ChannelLinkageMaps` (
    `IdMapping` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdPortalChannel` int  NOT NULL,
    `IdLinkedChannel` int  NOT NULL,
    `DisplayName` varchar(200)  NOT NULL
);

-- Creating table 'ChannelMaps'

CREATE TABLE `ChannelMaps` (
    `IdChannelMap` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdChannel` int  NOT NULL,
    `IdCard` int  NOT NULL,
    `EpgOnly` bit  NOT NULL
);

-- Creating table 'Conflicts'

CREATE TABLE `Conflicts` (
    `IdConflict` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdSchedule` int  NOT NULL,
    `IdConflictingSchedule` int  NOT NULL,
    `IdChannel` int  NOT NULL,
    `ConflictDate` datetime  NOT NULL,
    `IdCard` int  NULL
);

-- Creating table 'DisEqcMotors'

CREATE TABLE `DisEqcMotors` (
    `IdDiSEqCMotor` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdCard` int  NOT NULL,
    `IdSatellite` int  NOT NULL,
    `Position` int  NOT NULL
);

-- Creating table 'Favorites'

CREATE TABLE `Favorites` (
    `IdFavorite` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdProgram` int  NOT NULL,
    `Priority` int  NOT NULL,
    `TimesWatched` int  NOT NULL
);

-- Creating table 'GroupMaps'

CREATE TABLE `GroupMaps` (
    `IdMap` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdGroup` int  NOT NULL,
    `IdChannel` int  NOT NULL,
    `SortOrder` int  NOT NULL
);

-- Creating table 'Histories'

CREATE TABLE `Histories` (
    `IdHistory` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdChannel` int  NOT NULL,
    `StartTime` datetime  NOT NULL,
    `EndTime` datetime  NOT NULL,
    `Title` varchar(1000)  NOT NULL,
    `Description` varchar(1000)  NOT NULL,
    `Recorded` bit  NOT NULL,
    `Watched` int  NOT NULL,
    `IdProgramCategory` int  NULL
);

-- Creating table 'Keywords'

CREATE TABLE `Keywords` (
    `IdKeyword` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `KeywordName` varchar(200)  NOT NULL,
    `Rating` int  NOT NULL,
    `AutoRecord` bit  NOT NULL,
    `SearchIn` int  NOT NULL
);

-- Creating table 'KeywordMaps'

CREATE TABLE `KeywordMaps` (
    `IdKeywordMap` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdKeyword` int  NOT NULL,
    `IdChannelGroup` int  NOT NULL
);

-- Creating table 'PendingDeletions'

CREATE TABLE `PendingDeletions` (
    `IdPendingDeletion` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `FileName` varchar(max)  NOT NULL
);

-- Creating table 'PersonalTVGuideMaps'

CREATE TABLE `PersonalTVGuideMaps` (
    `IdPersonalTVGuideMap` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdKeyword` int  NOT NULL,
    `IdProgram` int  NOT NULL
);

-- Creating table 'Programs'

CREATE TABLE `Programs` (
    `IdProgram` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdChannel` int  NOT NULL,
    `StartTime` datetime  NOT NULL,
    `EndTime` datetime  NOT NULL,
    `Title` varchar(2000)  NOT NULL,
    `Description` varchar(8000)  NOT NULL,
    `SeriesNum` varchar(200)  NOT NULL,
    `EpisodeNum` varchar(200)  NOT NULL,
    `OriginalAirDate` datetime  NULL,
    `Classification` varchar(200)  NOT NULL,
    `StarRating` int  NOT NULL,
    `ParentalRating` int  NOT NULL,
    `EpisodeName` nvarchar(max)  NOT NULL,
    `EpisodePart` nvarchar(max)  NOT NULL,
    `State` int  NOT NULL,
    `PreviouslyShown` bit  NOT NULL,
    `IdProgramCategory` int  NULL,
    `StartTimeDayOfWeek` smallint  NOT NULL,
    `EndTimeDayOfWeek` smallint  NOT NULL,
    `EndTimeOffset` datetime  NOT NULL,
    `StartTimeOffset` datetime  NOT NULL
);

-- Creating table 'ProgramCategories'

CREATE TABLE `ProgramCategories` (
    `IdProgramCategory` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `Category` varchar(50)  NOT NULL,
    `IdTvGuideCategory` int  NULL
);

-- Creating table 'ProgramCredits'

CREATE TABLE `ProgramCredits` (
    `IdProgramCredit` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdProgram` int  NOT NULL,
    `Person` varchar(200)  NOT NULL,
    `Role` varchar(50)  NOT NULL
);

-- Creating table 'Recordings'

CREATE TABLE `Recordings` (
    `IdRecording` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdChannel` int  NULL,
    `StartTime` datetime  NOT NULL,
    `EndTime` datetime  NOT NULL,
    `Title` varchar(2000)  NOT NULL,
    `Description` varchar(8000)  NOT NULL,
    `FileName` varchar(260)  NOT NULL,
    `KeepUntil` int  NOT NULL,
    `KeepUntilDate` datetime  NULL,
    `TimesWatched` int  NOT NULL,
    `StopTime` int  NOT NULL,
    `EpisodeName` varchar(max)  NOT NULL,
    `SeriesNum` varchar(200)  NOT NULL,
    `EpisodeNum` varchar(200)  NOT NULL,
    `EpisodePart` varchar(max)  NOT NULL,
    `IsRecording` bit  NOT NULL,
    `IdSchedule` int  NULL,
    `MediaType` int  NOT NULL,
    `IdProgramCategory` int  NULL
);

-- Creating table 'RuleBasedSchedules'

CREATE TABLE `RuleBasedSchedules` (
    `IdRuleBasedSchedule` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `ScheduleName` varchar(256)  NOT NULL,
    `MaxAirings` int  NOT NULL,
    `Priority` int  NOT NULL,
    `Directory` varchar(1024)  NOT NULL,
    `Quality` int  NOT NULL,
    `KeepMethod` int  NOT NULL,
    `KeepDate` datetime  NULL,
    `PreRecordInterval` int  NOT NULL,
    `PostRecordInterval` int  NOT NULL,
    `Rules` varchar(max)  NULL
);

-- Creating table 'Satellites'

CREATE TABLE `Satellites` (
    `IdSatellite` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `SatelliteName` varchar(200)  NOT NULL,
    `TransponderFileName` varchar(200)  NOT NULL
);

-- Creating table 'Schedules'

CREATE TABLE `Schedules` (
    `IdSchedule` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdChannel` int  NOT NULL,
    `ScheduleType` int  NOT NULL,
    `ProgramName` varchar(256)  NOT NULL,
    `StartTime` datetime  NOT NULL,
    `EndTime` datetime  NOT NULL,
    `MaxAirings` int  NOT NULL,
    `Priority` int  NOT NULL,
    `Directory` varchar(1024)  NOT NULL,
    `Quality` int  NOT NULL,
    `KeepMethod` int  NOT NULL,
    `KeepDate` datetime  NULL,
    `PreRecordInterval` int  NOT NULL,
    `PostRecordInterval` int  NOT NULL,
    `Canceled` datetime  NOT NULL,
    `Series` bit  NOT NULL,
    `IdParentSchedule` int  NULL
);

-- Creating table 'ScheduleRulesTemplates'

CREATE TABLE `ScheduleRulesTemplates` (
    `IdScheduleRulesTemplate` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `Name` varchar(50)  NOT NULL,
    `Rules` varchar(max)  NOT NULL,
    `Enabled` bit  NOT NULL,
    `Usages` int  NOT NULL,
    `Editable` bit  NOT NULL
);

-- Creating table 'Settings'

CREATE TABLE `Settings` (
    `IdSetting` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `Tag` varchar(200)  NOT NULL,
    `Value` varchar(4096)  NOT NULL
);

-- Creating table 'SoftwareEncoders'

CREATE TABLE `SoftwareEncoders` (
    `IdEncoder` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `Priority` int  NOT NULL,
    `Name` varchar(200)  NOT NULL,
    `Type` int  NOT NULL,
    `Reusable` bit  NOT NULL
);

-- Creating table 'Timespans'

CREATE TABLE `Timespans` (
    `IdTimespan` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `StartTime` datetime  NOT NULL,
    `EndTime` datetime  NOT NULL,
    `DayOfWeek` int  NOT NULL,
    `IdKeyword` int  NOT NULL
);

-- Creating table 'TuningDetails'

CREATE TABLE `TuningDetails` (
    `IdTuning` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdChannel` int  NOT NULL,
    `Name` varchar(200)  NOT NULL,
    `Provider` varchar(200)  NOT NULL,
    `ChannelType` int  NOT NULL,
    `ChannelNumber` int  NOT NULL,
    `Frequency` int  NOT NULL,
    `CountryId` int  NOT NULL,
    `NetworkId` int  NOT NULL,
    `TransportId` int  NOT NULL,
    `ServiceId` int  NOT NULL,
    `PmtPid` int  NOT NULL,
    `FreeToAir` bit  NOT NULL,
    `Modulation` int  NOT NULL,
    `Polarisation` int  NOT NULL,
    `Symbolrate` int  NOT NULL,
    `DiSEqC` int  NOT NULL,
    `Bandwidth` int  NOT NULL,
    `MajorChannel` int  NOT NULL,
    `MinorChannel` int  NOT NULL,
    `VideoSource` int  NOT NULL,
    `TuningSource` int  NOT NULL,
    `Band` int  NOT NULL,
    `SatIndex` int  NOT NULL,
    `InnerFecRate` int  NOT NULL,
    `Pilot` int  NOT NULL,
    `RollOff` int  NOT NULL,
    `Url` varchar(200)  NOT NULL,
    `Bitrate` int  NOT NULL,
    `AudioSource` int  NOT NULL,
    `IsVCRSignal` bit  NOT NULL,
    `MediaType` int  NOT NULL,
    `IdLnbType` int  NULL
);

-- Creating table 'TvMovieMappings'

CREATE TABLE `TvMovieMappings` (
    `IdMapping` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdChannel` int  NOT NULL,
    `StationName` varchar(200)  NOT NULL,
    `TimeSharingStart` varchar(200)  NOT NULL,
    `TimeSharingEnd` varchar(200)  NOT NULL
);

-- Creating table 'Versions'

CREATE TABLE `Versions` (
    `IdVersion` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `VersionNumber` int  NOT NULL
);

-- Creating table 'LnbTypes'

CREATE TABLE `LnbTypes` (
    `IdLnbType` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `Name` nvarchar(max)  NOT NULL,
    `LowBandFrequency` int  NOT NULL,
    `HighBandFrequency` int  NOT NULL,
    `SwitchFrequency` int  NOT NULL,
    `IsBandStacked` bit  NOT NULL,
    `IsToroidal` bit  NOT NULL
);

-- Creating table 'RecordingCredits'

CREATE TABLE `RecordingCredits` (
    `IdRecordingCredit` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `IdRecording` int  NOT NULL,
    `Person` varchar(200)  NOT NULL,
    `Role` varchar(50)  NOT NULL
);

-- Creating table 'TvGuideCategories'

CREATE TABLE `TvGuideCategories` (
    `IdTvGuideCategory` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `Name` nvarchar(max)  NOT NULL,
    `IsMovie` bit  NOT NULL,
    `IsEnabled` bit  NOT NULL
);



-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------



-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on `IdGroup` in table 'GroupMaps'

ALTER TABLE `GroupMaps`
ADD CONSTRAINT `FK_GroupMapChannelGroup`
    FOREIGN KEY (`IdGroup`)
    REFERENCES `ChannelGroups`
        (`IdGroup`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GroupMapChannelGroup'

CREATE INDEX `IX_FK_GroupMapChannelGroup` 
    ON `GroupMaps`
    (`IdGroup`);

-- Creating foreign key on `IdChannel` in table 'GroupMaps'

ALTER TABLE `GroupMaps`
ADD CONSTRAINT `FK_GroupMapChannel`
    FOREIGN KEY (`IdChannel`)
    REFERENCES `Channels`
        (`IdChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GroupMapChannel'

CREATE INDEX `IX_FK_GroupMapChannel` 
    ON `GroupMaps`
    (`IdChannel`);

-- Creating foreign key on `IdCard` in table 'CardGroupMaps'

ALTER TABLE `CardGroupMaps`
ADD CONSTRAINT `FK_CardGroupMapCard`
    FOREIGN KEY (`IdCard`)
    REFERENCES `Cards`
        (`IdCard`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardGroupMapCard'

CREATE INDEX `IX_FK_CardGroupMapCard` 
    ON `CardGroupMaps`
    (`IdCard`);

-- Creating foreign key on `IdCard` in table 'DisEqcMotors'

ALTER TABLE `DisEqcMotors`
ADD CONSTRAINT `FK_DisEqcMotorCard`
    FOREIGN KEY (`IdCard`)
    REFERENCES `Cards`
        (`IdCard`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DisEqcMotorCard'

CREATE INDEX `IX_FK_DisEqcMotorCard` 
    ON `DisEqcMotors`
    (`IdCard`);

-- Creating foreign key on `IdChannel` in table 'Recordings'

ALTER TABLE `Recordings`
ADD CONSTRAINT `FK_ChannelRecording`
    FOREIGN KEY (`IdChannel`)
    REFERENCES `Channels`
        (`IdChannel`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelRecording'

CREATE INDEX `IX_FK_ChannelRecording` 
    ON `Recordings`
    (`IdChannel`);

-- Creating foreign key on `IdCardGroup` in table 'CardGroupMaps'

ALTER TABLE `CardGroupMaps`
ADD CONSTRAINT `FK_CardGroupMapCardGroup`
    FOREIGN KEY (`IdCardGroup`)
    REFERENCES `CardGroups`
        (`IdCardGroup`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardGroupMapCardGroup'

CREATE INDEX `IX_FK_CardGroupMapCardGroup` 
    ON `CardGroupMaps`
    (`IdCardGroup`);

-- Creating foreign key on `IdChannel` in table 'Programs'

ALTER TABLE `Programs`
ADD CONSTRAINT `FK_ChannelProgram`
    FOREIGN KEY (`IdChannel`)
    REFERENCES `Channels`
        (`IdChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelProgram'

CREATE INDEX `IX_FK_ChannelProgram` 
    ON `Programs`
    (`IdChannel`);

-- Creating foreign key on `IdCard` in table 'ChannelMaps'

ALTER TABLE `ChannelMaps`
ADD CONSTRAINT `FK_CardChannelMap`
    FOREIGN KEY (`IdCard`)
    REFERENCES `Cards`
        (`IdCard`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardChannelMap'

CREATE INDEX `IX_FK_CardChannelMap` 
    ON `ChannelMaps`
    (`IdCard`);

-- Creating foreign key on `IdChannel` in table 'ChannelMaps'

ALTER TABLE `ChannelMaps`
ADD CONSTRAINT `FK_ChannelChannelMap`
    FOREIGN KEY (`IdChannel`)
    REFERENCES `Channels`
        (`IdChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelChannelMap'

CREATE INDEX `IX_FK_ChannelChannelMap` 
    ON `ChannelMaps`
    (`IdChannel`);

-- Creating foreign key on `IdChannel` in table 'Schedules'

ALTER TABLE `Schedules`
ADD CONSTRAINT `FK_ChannelSchedule`
    FOREIGN KEY (`IdChannel`)
    REFERENCES `Channels`
        (`IdChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelSchedule'

CREATE INDEX `IX_FK_ChannelSchedule` 
    ON `Schedules`
    (`IdChannel`);

-- Creating foreign key on `IdParentSchedule` in table 'Schedules'

ALTER TABLE `Schedules`
ADD CONSTRAINT `FK_ScheduleParentSchedule`
    FOREIGN KEY (`IdParentSchedule`)
    REFERENCES `Schedules`
        (`IdSchedule`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleParentSchedule'

CREATE INDEX `IX_FK_ScheduleParentSchedule` 
    ON `Schedules`
    (`IdParentSchedule`);

-- Creating foreign key on `IdProgramCategory` in table 'Programs'

ALTER TABLE `Programs`
ADD CONSTRAINT `FK_ProgramProgramCategory`
    FOREIGN KEY (`IdProgramCategory`)
    REFERENCES `ProgramCategories`
        (`IdProgramCategory`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramProgramCategory'

CREATE INDEX `IX_FK_ProgramProgramCategory` 
    ON `Programs`
    (`IdProgramCategory`);

-- Creating foreign key on `IdProgram` in table 'ProgramCredits'

ALTER TABLE `ProgramCredits`
ADD CONSTRAINT `FK_ProgramProgramCredit`
    FOREIGN KEY (`IdProgram`)
    REFERENCES `Programs`
        (`IdProgram`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramProgramCredit'

CREATE INDEX `IX_FK_ProgramProgramCredit` 
    ON `ProgramCredits`
    (`IdProgram`);

-- Creating foreign key on `IdChannel` in table 'Histories'

ALTER TABLE `Histories`
ADD CONSTRAINT `FK_ChannelHistory`
    FOREIGN KEY (`IdChannel`)
    REFERENCES `Channels`
        (`IdChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelHistory'

CREATE INDEX `IX_FK_ChannelHistory` 
    ON `Histories`
    (`IdChannel`);

-- Creating foreign key on `IdChannel` in table 'TuningDetails'

ALTER TABLE `TuningDetails`
ADD CONSTRAINT `FK_ChannelTuningDetail`
    FOREIGN KEY (`IdChannel`)
    REFERENCES `Channels`
        (`IdChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelTuningDetail'

CREATE INDEX `IX_FK_ChannelTuningDetail` 
    ON `TuningDetails`
    (`IdChannel`);

-- Creating foreign key on `IdChannel` in table 'TvMovieMappings'

ALTER TABLE `TvMovieMappings`
ADD CONSTRAINT `FK_ChannelTvMovieMapping`
    FOREIGN KEY (`IdChannel`)
    REFERENCES `Channels`
        (`IdChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelTvMovieMapping'

CREATE INDEX `IX_FK_ChannelTvMovieMapping` 
    ON `TvMovieMappings`
    (`IdChannel`);

-- Creating foreign key on `IdSatellite` in table 'DisEqcMotors'

ALTER TABLE `DisEqcMotors`
ADD CONSTRAINT `FK_DisEqcMotorSatellite`
    FOREIGN KEY (`IdSatellite`)
    REFERENCES `Satellites`
        (`IdSatellite`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DisEqcMotorSatellite'

CREATE INDEX `IX_FK_DisEqcMotorSatellite` 
    ON `DisEqcMotors`
    (`IdSatellite`);

-- Creating foreign key on `IdProgram` in table 'PersonalTVGuideMaps'

ALTER TABLE `PersonalTVGuideMaps`
ADD CONSTRAINT `FK_ProgramPersonalTVGuideMap`
    FOREIGN KEY (`IdProgram`)
    REFERENCES `Programs`
        (`IdProgram`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramPersonalTVGuideMap'

CREATE INDEX `IX_FK_ProgramPersonalTVGuideMap` 
    ON `PersonalTVGuideMaps`
    (`IdProgram`);

-- Creating foreign key on `IdKeyword` in table 'PersonalTVGuideMaps'

ALTER TABLE `PersonalTVGuideMaps`
ADD CONSTRAINT `FK_KeywordPersonalTVGuideMap`
    FOREIGN KEY (`IdKeyword`)
    REFERENCES `Keywords`
        (`IdKeyword`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_KeywordPersonalTVGuideMap'

CREATE INDEX `IX_FK_KeywordPersonalTVGuideMap` 
    ON `PersonalTVGuideMaps`
    (`IdKeyword`);

-- Creating foreign key on `IdSchedule` in table 'Recordings'

ALTER TABLE `Recordings`
ADD CONSTRAINT `FK_ScheduleRecording`
    FOREIGN KEY (`IdSchedule`)
    REFERENCES `Schedules`
        (`IdSchedule`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleRecording'

CREATE INDEX `IX_FK_ScheduleRecording` 
    ON `Recordings`
    (`IdSchedule`);

-- Creating foreign key on `IdKeyword` in table 'KeywordMaps'

ALTER TABLE `KeywordMaps`
ADD CONSTRAINT `FK_KeywordKeywordMap`
    FOREIGN KEY (`IdKeyword`)
    REFERENCES `Keywords`
        (`IdKeyword`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_KeywordKeywordMap'

CREATE INDEX `IX_FK_KeywordKeywordMap` 
    ON `KeywordMaps`
    (`IdKeyword`);

-- Creating foreign key on `IdChannelGroup` in table 'KeywordMaps'

ALTER TABLE `KeywordMaps`
ADD CONSTRAINT `FK_KeywordMapChannelGroup`
    FOREIGN KEY (`IdChannelGroup`)
    REFERENCES `ChannelGroups`
        (`IdGroup`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_KeywordMapChannelGroup'

CREATE INDEX `IX_FK_KeywordMapChannelGroup` 
    ON `KeywordMaps`
    (`IdChannelGroup`);

-- Creating foreign key on `IdRecording` in table 'RecordingCredits'

ALTER TABLE `RecordingCredits`
ADD CONSTRAINT `FK_RecordingRecordingCredit`
    FOREIGN KEY (`IdRecording`)
    REFERENCES `Recordings`
        (`IdRecording`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RecordingRecordingCredit'

CREATE INDEX `IX_FK_RecordingRecordingCredit` 
    ON `RecordingCredits`
    (`IdRecording`);

-- Creating foreign key on `IdSchedule` in table 'CanceledSchedules'

ALTER TABLE `CanceledSchedules`
ADD CONSTRAINT `FK_ScheduleCanceledSchedule`
    FOREIGN KEY (`IdSchedule`)
    REFERENCES `Schedules`
        (`IdSchedule`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleCanceledSchedule'

CREATE INDEX `IX_FK_ScheduleCanceledSchedule` 
    ON `CanceledSchedules`
    (`IdSchedule`);

-- Creating foreign key on `IdLinkedChannel` in table 'ChannelLinkageMaps'

ALTER TABLE `ChannelLinkageMaps`
ADD CONSTRAINT `FK_ChannelLinkMap`
    FOREIGN KEY (`IdLinkedChannel`)
    REFERENCES `Channels`
        (`IdChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelLinkMap'

CREATE INDEX `IX_FK_ChannelLinkMap` 
    ON `ChannelLinkageMaps`
    (`IdLinkedChannel`);

-- Creating foreign key on `IdPortalChannel` in table 'ChannelLinkageMaps'

ALTER TABLE `ChannelLinkageMaps`
ADD CONSTRAINT `FK_ChannelPortalMap`
    FOREIGN KEY (`IdPortalChannel`)
    REFERENCES `Channels`
        (`IdChannel`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelPortalMap'

CREATE INDEX `IX_FK_ChannelPortalMap` 
    ON `ChannelLinkageMaps`
    (`IdPortalChannel`);

-- Creating foreign key on `IdProgramCategory` in table 'Recordings'

ALTER TABLE `Recordings`
ADD CONSTRAINT `FK_RecordingProgramCategory`
    FOREIGN KEY (`IdProgramCategory`)
    REFERENCES `ProgramCategories`
        (`IdProgramCategory`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RecordingProgramCategory'

CREATE INDEX `IX_FK_RecordingProgramCategory` 
    ON `Recordings`
    (`IdProgramCategory`);

-- Creating foreign key on `IdCard` in table 'Conflicts'

ALTER TABLE `Conflicts`
ADD CONSTRAINT `FK_CardConflict`
    FOREIGN KEY (`IdCard`)
    REFERENCES `Cards`
        (`IdCard`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardConflict'

CREATE INDEX `IX_FK_CardConflict` 
    ON `Conflicts`
    (`IdCard`);

-- Creating foreign key on `IdChannel` in table 'Conflicts'

ALTER TABLE `Conflicts`
ADD CONSTRAINT `FK_ChannelConflict`
    FOREIGN KEY (`IdChannel`)
    REFERENCES `Channels`
        (`IdChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelConflict'

CREATE INDEX `IX_FK_ChannelConflict` 
    ON `Conflicts`
    (`IdChannel`);

-- Creating foreign key on `IdSchedule` in table 'Conflicts'

ALTER TABLE `Conflicts`
ADD CONSTRAINT `FK_ScheduleConflict`
    FOREIGN KEY (`IdSchedule`)
    REFERENCES `Schedules`
        (`IdSchedule`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleConflict'

CREATE INDEX `IX_FK_ScheduleConflict` 
    ON `Conflicts`
    (`IdSchedule`);

-- Creating foreign key on `IdConflictingSchedule` in table 'Conflicts'

ALTER TABLE `Conflicts`
ADD CONSTRAINT `FK_ScheduleConflict1`
    FOREIGN KEY (`IdConflictingSchedule`)
    REFERENCES `Schedules`
        (`IdSchedule`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleConflict1'

CREATE INDEX `IX_FK_ScheduleConflict1` 
    ON `Conflicts`
    (`IdConflictingSchedule`);

-- Creating foreign key on `IdProgramCategory` in table 'Histories'

ALTER TABLE `Histories`
ADD CONSTRAINT `FK_ProgramCategoryHistory`
    FOREIGN KEY (`IdProgramCategory`)
    REFERENCES `ProgramCategories`
        (`IdProgramCategory`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramCategoryHistory'

CREATE INDEX `IX_FK_ProgramCategoryHistory` 
    ON `Histories`
    (`IdProgramCategory`);

-- Creating foreign key on `IdLnbType` in table 'TuningDetails'

ALTER TABLE `TuningDetails`
ADD CONSTRAINT `FK_LnbTypeTuningDetail`
    FOREIGN KEY (`IdLnbType`)
    REFERENCES `LnbTypes`
        (`IdLnbType`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_LnbTypeTuningDetail'

CREATE INDEX `IX_FK_LnbTypeTuningDetail` 
    ON `TuningDetails`
    (`IdLnbType`);

-- Creating foreign key on `IdTvGuideCategory` in table 'ProgramCategories'

ALTER TABLE `ProgramCategories`
ADD CONSTRAINT `FK_TvGuideCategoryProgramCategory`
    FOREIGN KEY (`IdTvGuideCategory`)
    REFERENCES `TvGuideCategories`
        (`IdTvGuideCategory`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TvGuideCategoryProgramCategory'

CREATE INDEX `IX_FK_TvGuideCategoryProgramCategory` 
    ON `ProgramCategories`
    (`IdTvGuideCategory`);

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
