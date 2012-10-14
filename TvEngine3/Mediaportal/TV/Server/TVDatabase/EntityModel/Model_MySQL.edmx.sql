



-- -----------------------------------------------------------
-- Entity Designer DDL Script for MySQL Server 4.1 and higher
-- -----------------------------------------------------------
-- Date Created: 01/09/2012 19:28:23
-- Generated from EDMX file: D:\svnroot\MediaPortal-1-commit_Compare\TvEngine3\Mediaportal\TV\Server\TVDatabase\EntityModel\Model.edmx
-- Target version: 2.0.0.0
-- --------------------------------------------------

DROP DATABASE IF EXISTS `tve35mysql`;
CREATE DATABASE `tve35mysql`;
USE `tve35mysql`;

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
    DROP TABLE IF EXISTS `RecordingCredits`;
SET foreign_key_checks = 1;

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'CanceledSchedules'

CREATE TABLE `CanceledSchedules` (
    `idCanceledSchedule` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idSchedule` int  NOT NULL,
    `idChannel` int  NOT NULL,
    `cancelDateTime` datetime  NOT NULL
);

-- Creating table 'Cards'

CREATE TABLE `Cards` (
    `idCard` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `devicePath` mediumtext  NOT NULL,
    `name` mediumtext  NOT NULL,
    `priority` int  NOT NULL,
    `grabEPG` bool  NOT NULL,
    `lastEpgGrab` datetime  NULL,
    `recordingFolder` mediumtext  NOT NULL,
    `enabled` bool  NOT NULL,
    `camType` int  NOT NULL,
    `timeshiftingFolder` mediumtext  NOT NULL,
    `recordingFormat` int  NOT NULL,
    `decryptLimit` int  NOT NULL,
    `preload` bool  NOT NULL,
    `CAM` bool  NOT NULL,
    `NetProvider` int  NOT NULL,
    `stopgraph` bool  NOT NULL
);

-- Creating table 'CardGroups'

CREATE TABLE `CardGroups` (
    `idCardGroup` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `name` mediumtext  NOT NULL
);

-- Creating table 'CardGroupMaps'

CREATE TABLE `CardGroupMaps` (
    `idMapping` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idCard` int  NOT NULL,
    `idCardGroup` int  NOT NULL
);

-- Creating table 'Channels'

CREATE TABLE `Channels` (
    `idChannel` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `timesWatched` int  NOT NULL,
    `totalTimeWatched` datetime  NULL,
    `grabEpg` bool  NOT NULL,
    `lastGrabTime` datetime  NULL,
    `sortOrder` int  NOT NULL,
    `visibleInGuide` bool  NOT NULL,
    `externalId` mediumtext  NULL,
    `displayName` mediumtext  NOT NULL,
    `epgHasGaps` bool  NOT NULL,
    `mediaType` int  NOT NULL
);

-- Creating table 'ChannelGroups'

CREATE TABLE `ChannelGroups` (
    `idGroup` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `groupName` mediumtext  NOT NULL,
    `sortOrder` int  NOT NULL
);

-- Creating table 'ChannelLinkageMaps'

CREATE TABLE `ChannelLinkageMaps` (
    `idMapping` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idPortalChannel` int  NOT NULL,
    `idLinkedChannel` int  NOT NULL,
    `displayName` mediumtext  NOT NULL
);

-- Creating table 'ChannelMaps'

CREATE TABLE `ChannelMaps` (
    `idChannelMap` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idChannel` int  NOT NULL,
    `idCard` int  NOT NULL,
    `epgOnly` bool  NOT NULL
);

-- Creating table 'Conflicts'

CREATE TABLE `Conflicts` (
    `idConflict` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idSchedule` int  NOT NULL,
    `idConflictingSchedule` int  NOT NULL,
    `idChannel` int  NOT NULL,
    `conflictDate` datetime  NOT NULL,
    `idCard` int  NULL
);

-- Creating table 'DisEqcMotors'

CREATE TABLE `DisEqcMotors` (
    `idDiSEqCMotor` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idCard` int  NOT NULL,
    `idSatellite` int  NOT NULL,
    `position` int  NOT NULL
);

-- Creating table 'Favorites'

CREATE TABLE `Favorites` (
    `idFavorite` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idProgram` int  NOT NULL,
    `priority` int  NOT NULL,
    `timesWatched` int  NOT NULL
);

-- Creating table 'GroupMaps'

CREATE TABLE `GroupMaps` (
    `idMap` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idGroup` int  NOT NULL,
    `idChannel` int  NOT NULL,
    `SortOrder` int  NOT NULL,
    `mediaType` int  NOT NULL
);

-- Creating table 'Histories'

CREATE TABLE `Histories` (
    `idHistory` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idChannel` int  NOT NULL,
    `startTime` datetime  NOT NULL,
    `endTime` datetime  NOT NULL,
    `title` mediumtext  NOT NULL,
    `description` mediumtext  NOT NULL,
    `recorded` bool  NOT NULL,
    `watched` int  NOT NULL,
    `idProgramCategory` int  NULL
);

-- Creating table 'Keywords'

CREATE TABLE `Keywords` (
    `idKeyword` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `keywordName` mediumtext  NOT NULL,
    `rating` int  NOT NULL,
    `autoRecord` bool  NOT NULL,
    `searchIn` int  NOT NULL
);

-- Creating table 'KeywordMaps'

CREATE TABLE `KeywordMaps` (
    `idKeywordMap` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idKeyword` int  NOT NULL,
    `idChannelGroup` int  NOT NULL
);

-- Creating table 'PendingDeletions'

CREATE TABLE `PendingDeletions` (
    `idPendingDeletion` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `fileName` longtext  NOT NULL
);

-- Creating table 'PersonalTVGuideMaps'

CREATE TABLE `PersonalTVGuideMaps` (
    `idPersonalTVGuideMap` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idKeyword` int  NOT NULL,
    `idProgram` int  NOT NULL
);

-- Creating table 'Programs'

CREATE TABLE `Programs` (
    `idProgram` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idChannel` int  NOT NULL,
    `startTime` datetime  NOT NULL,
    `endTime` datetime  NOT NULL,
    `title` mediumtext  NOT NULL,
    `description` mediumtext  NOT NULL,
    `seriesNum` mediumtext  NOT NULL,
    `episodeNum` mediumtext  NOT NULL,
    `originalAirDate` datetime  NULL,
    `classification` mediumtext  NOT NULL,
    `starRating` int  NOT NULL,
    `parentalRating` int  NOT NULL,
    `episodeName` longtext  NOT NULL,
    `episodePart` longtext  NOT NULL,
    `state` int  NOT NULL,
    `previouslyShown` bool  NOT NULL,
    `idProgramCategory` int  NULL,
    `startTimeDayOfWeek` smallint  NOT NULL,
    `endTimeDayOfWeek` smallint  NOT NULL,
    `endTimeOffset` datetime  NOT NULL,
    `startTimeOffset` datetime  NOT NULL
);

-- Creating table 'ProgramCategories'

CREATE TABLE `ProgramCategories` (
    `idProgramCategory` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `category` mediumtext  NOT NULL
);

-- Creating table 'ProgramCredits'

CREATE TABLE `ProgramCredits` (
    `idProgramCredit` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idProgram` int  NOT NULL,
    `person` mediumtext  NOT NULL,
    `role` mediumtext  NOT NULL
);

-- Creating table 'Recordings'

CREATE TABLE `Recordings` (
    `idRecording` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idChannel` int  NULL,
    `startTime` datetime  NOT NULL,
    `endTime` datetime  NOT NULL,
    `title` mediumtext  NOT NULL,
    `description` mediumtext  NOT NULL,
    `fileName` mediumtext  NOT NULL,
    `keepUntil` int  NOT NULL,
    `keepUntilDate` datetime  NULL,
    `timesWatched` int  NOT NULL,
    `stopTime` int  NOT NULL,
    `episodeName` longtext  NOT NULL,
    `seriesNum` mediumtext  NOT NULL,
    `episodeNum` mediumtext  NOT NULL,
    `episodePart` longtext  NOT NULL,
    `isRecording` bool  NOT NULL,
    `idSchedule` int  NULL,
    `mediaType` int  NOT NULL,
    `idProgramCategory` int  NULL
);

-- Creating table 'RuleBasedSchedules'

CREATE TABLE `RuleBasedSchedules` (
    `id_RuleBasedSchedule` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `scheduleName` mediumtext  NOT NULL,
    `maxAirings` int  NOT NULL,
    `priority` int  NOT NULL,
    `directory` mediumtext  NOT NULL,
    `quality` int  NOT NULL,
    `keepMethod` int  NOT NULL,
    `keepDate` datetime  NULL,
    `preRecordInterval` int  NOT NULL,
    `postRecordInterval` int  NOT NULL,
    `rules` longtext  NULL
);

-- Creating table 'Satellites'

CREATE TABLE `Satellites` (
    `idSatellite` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `satelliteName` mediumtext  NOT NULL,
    `transponderFileName` mediumtext  NOT NULL
);

-- Creating table 'Schedules'

CREATE TABLE `Schedules` (
    `id_Schedule` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idChannel` int  NOT NULL,
    `scheduleType` int  NOT NULL,
    `programName` mediumtext  NOT NULL,
    `startTime` datetime  NOT NULL,
    `endTime` datetime  NOT NULL,
    `maxAirings` int  NOT NULL,
    `priority` int  NOT NULL,
    `directory` mediumtext  NOT NULL,
    `quality` int  NOT NULL,
    `keepMethod` int  NOT NULL,
    `keepDate` datetime  NULL,
    `preRecordInterval` int  NOT NULL,
    `postRecordInterval` int  NOT NULL,
    `canceled` datetime  NOT NULL,
    `series` bool  NOT NULL,
    `idParentSchedule` int  NULL
);

-- Creating table 'ScheduleRulesTemplates'

CREATE TABLE `ScheduleRulesTemplates` (
    `idScheduleRulesTemplate` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `name` mediumtext  NOT NULL,
    `rules` longtext  NOT NULL,
    `enabled` bool  NOT NULL,
    `usages` int  NOT NULL,
    `editable` bool  NOT NULL
);

-- Creating table 'Settings'

CREATE TABLE `Settings` (
    `idSetting` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `tag` mediumtext  NOT NULL,
    `value` mediumtext  NOT NULL
);

-- Creating table 'SoftwareEncoders'

CREATE TABLE `SoftwareEncoders` (
    `idEncoder` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `priority` int  NOT NULL,
    `name` mediumtext  NOT NULL,
    `type` int  NOT NULL,
    `reusable` bool  NOT NULL
);

-- Creating table 'Timespans'

CREATE TABLE `Timespans` (
    `idTimespan` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `startTime` datetime  NOT NULL,
    `endTime` datetime  NOT NULL,
    `dayOfWeek` int  NOT NULL,
    `idKeyword` int  NOT NULL
);

-- Creating table 'TuningDetails'

CREATE TABLE `TuningDetails` (
    `idTuning` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idChannel` int  NOT NULL,
    `name` mediumtext  NOT NULL,
    `provider` mediumtext  NOT NULL,
    `channelType` int  NOT NULL,
    `channelNumber` int  NOT NULL,
    `frequency` int  NOT NULL,
    `countryId` int  NOT NULL,
    `networkId` int  NOT NULL,
    `transportId` int  NOT NULL,
    `serviceId` int  NOT NULL,
    `pmtPid` int  NOT NULL,
    `freeToAir` bool  NOT NULL,
    `modulation` int  NOT NULL,
    `polarisation` int  NOT NULL,
    `symbolrate` int  NOT NULL,
    `diseqc` int  NOT NULL,
    `switchingFrequency` int  NOT NULL,
    `bandwidth` int  NOT NULL,
    `majorChannel` int  NOT NULL,
    `minorChannel` int  NOT NULL,
    `videoSource` int  NOT NULL,
    `tuningSource` int  NOT NULL,
    `band` int  NOT NULL,
    `satIndex` int  NOT NULL,
    `innerFecRate` int  NOT NULL,
    `pilot` int  NOT NULL,
    `rollOff` int  NOT NULL,
    `url` mediumtext  NOT NULL,
    `bitrate` int  NOT NULL,
    `audioSource` int  NOT NULL,
    `isVCRSignal` bool  NOT NULL,
    `mediaType` int  NOT NULL
);

-- Creating table 'TvMovieMappings'

CREATE TABLE `TvMovieMappings` (
    `idMapping` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idChannel` int  NOT NULL,
    `stationName` mediumtext  NOT NULL,
    `timeSharingStart` mediumtext  NOT NULL,
    `timeSharingEnd` mediumtext  NOT NULL
);

-- Creating table 'Versions'

CREATE TABLE `Versions` (
    `idVersion` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `versionNumber` int  NOT NULL
);

-- Creating table 'RecordingCredits'

CREATE TABLE `RecordingCredits` (
    `idRecordingCredit` int AUTO_INCREMENT PRIMARY KEY NOT NULL,
    `idRecording` int  NOT NULL,
    `person` mediumtext  NOT NULL,
    `role` mediumtext  NOT NULL
);



-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------



-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on `idGroup` in table 'GroupMaps'

ALTER TABLE `GroupMaps`
ADD CONSTRAINT `FK_GroupMapChannelGroup`
    FOREIGN KEY (`idGroup`)
    REFERENCES `ChannelGroups`
        (`idGroup`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GroupMapChannelGroup'

CREATE INDEX `IX_FK_GroupMapChannelGroup` 
    ON `GroupMaps`
    (`idGroup`);

-- Creating foreign key on `idChannel` in table 'GroupMaps'

ALTER TABLE `GroupMaps`
ADD CONSTRAINT `FK_GroupMapChannel`
    FOREIGN KEY (`idChannel`)
    REFERENCES `Channels`
        (`idChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GroupMapChannel'

CREATE INDEX `IX_FK_GroupMapChannel` 
    ON `GroupMaps`
    (`idChannel`);

-- Creating foreign key on `idCard` in table 'CardGroupMaps'

ALTER TABLE `CardGroupMaps`
ADD CONSTRAINT `FK_CardGroupMapCard`
    FOREIGN KEY (`idCard`)
    REFERENCES `Cards`
        (`idCard`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardGroupMapCard'

CREATE INDEX `IX_FK_CardGroupMapCard` 
    ON `CardGroupMaps`
    (`idCard`);

-- Creating foreign key on `idCard` in table 'DisEqcMotors'

ALTER TABLE `DisEqcMotors`
ADD CONSTRAINT `FK_DisEqcMotorCard`
    FOREIGN KEY (`idCard`)
    REFERENCES `Cards`
        (`idCard`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DisEqcMotorCard'

CREATE INDEX `IX_FK_DisEqcMotorCard` 
    ON `DisEqcMotors`
    (`idCard`);

-- Creating foreign key on `idChannel` in table 'Recordings'

ALTER TABLE `Recordings`
ADD CONSTRAINT `FK_ChannelRecording`
    FOREIGN KEY (`idChannel`)
    REFERENCES `Channels`
        (`idChannel`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelRecording'

CREATE INDEX `IX_FK_ChannelRecording` 
    ON `Recordings`
    (`idChannel`);

-- Creating foreign key on `idCardGroup` in table 'CardGroupMaps'

ALTER TABLE `CardGroupMaps`
ADD CONSTRAINT `FK_CardGroupMapCardGroup`
    FOREIGN KEY (`idCardGroup`)
    REFERENCES `CardGroups`
        (`idCardGroup`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardGroupMapCardGroup'

CREATE INDEX `IX_FK_CardGroupMapCardGroup` 
    ON `CardGroupMaps`
    (`idCardGroup`);

-- Creating foreign key on `idChannel` in table 'Programs'

ALTER TABLE `Programs`
ADD CONSTRAINT `FK_ChannelProgram`
    FOREIGN KEY (`idChannel`)
    REFERENCES `Channels`
        (`idChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelProgram'

CREATE INDEX `IX_FK_ChannelProgram` 
    ON `Programs`
    (`idChannel`);

-- Creating foreign key on `idCard` in table 'ChannelMaps'

ALTER TABLE `ChannelMaps`
ADD CONSTRAINT `FK_CardChannelMap`
    FOREIGN KEY (`idCard`)
    REFERENCES `Cards`
        (`idCard`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardChannelMap'

CREATE INDEX `IX_FK_CardChannelMap` 
    ON `ChannelMaps`
    (`idCard`);

-- Creating foreign key on `idChannel` in table 'ChannelMaps'

ALTER TABLE `ChannelMaps`
ADD CONSTRAINT `FK_ChannelChannelMap`
    FOREIGN KEY (`idChannel`)
    REFERENCES `Channels`
        (`idChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelChannelMap'

CREATE INDEX `IX_FK_ChannelChannelMap` 
    ON `ChannelMaps`
    (`idChannel`);

-- Creating foreign key on `idChannel` in table 'Schedules'

ALTER TABLE `Schedules`
ADD CONSTRAINT `FK_ChannelSchedule`
    FOREIGN KEY (`idChannel`)
    REFERENCES `Channels`
        (`idChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelSchedule'

CREATE INDEX `IX_FK_ChannelSchedule` 
    ON `Schedules`
    (`idChannel`);

-- Creating foreign key on `idParentSchedule` in table 'Schedules'

ALTER TABLE `Schedules`
ADD CONSTRAINT `FK_ScheduleParentSchedule`
    FOREIGN KEY (`idParentSchedule`)
    REFERENCES `Schedules`
        (`id_Schedule`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleParentSchedule'

CREATE INDEX `IX_FK_ScheduleParentSchedule` 
    ON `Schedules`
    (`idParentSchedule`);

-- Creating foreign key on `idProgramCategory` in table 'Programs'

ALTER TABLE `Programs`
ADD CONSTRAINT `FK_ProgramProgramCategory`
    FOREIGN KEY (`idProgramCategory`)
    REFERENCES `ProgramCategories`
        (`idProgramCategory`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramProgramCategory'

CREATE INDEX `IX_FK_ProgramProgramCategory` 
    ON `Programs`
    (`idProgramCategory`);

-- Creating foreign key on `idProgram` in table 'ProgramCredits'

ALTER TABLE `ProgramCredits`
ADD CONSTRAINT `FK_ProgramProgramCredit`
    FOREIGN KEY (`idProgram`)
    REFERENCES `Programs`
        (`idProgram`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramProgramCredit'

CREATE INDEX `IX_FK_ProgramProgramCredit` 
    ON `ProgramCredits`
    (`idProgram`);

-- Creating foreign key on `idChannel` in table 'Histories'

ALTER TABLE `Histories`
ADD CONSTRAINT `FK_ChannelHistory`
    FOREIGN KEY (`idChannel`)
    REFERENCES `Channels`
        (`idChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelHistory'

CREATE INDEX `IX_FK_ChannelHistory` 
    ON `Histories`
    (`idChannel`);

-- Creating foreign key on `idChannel` in table 'TuningDetails'

ALTER TABLE `TuningDetails`
ADD CONSTRAINT `FK_ChannelTuningDetail`
    FOREIGN KEY (`idChannel`)
    REFERENCES `Channels`
        (`idChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelTuningDetail'

CREATE INDEX `IX_FK_ChannelTuningDetail` 
    ON `TuningDetails`
    (`idChannel`);

-- Creating foreign key on `idChannel` in table 'TvMovieMappings'

ALTER TABLE `TvMovieMappings`
ADD CONSTRAINT `FK_ChannelTvMovieMapping`
    FOREIGN KEY (`idChannel`)
    REFERENCES `Channels`
        (`idChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelTvMovieMapping'

CREATE INDEX `IX_FK_ChannelTvMovieMapping` 
    ON `TvMovieMappings`
    (`idChannel`);

-- Creating foreign key on `idSatellite` in table 'DisEqcMotors'

ALTER TABLE `DisEqcMotors`
ADD CONSTRAINT `FK_DisEqcMotorSatellite`
    FOREIGN KEY (`idSatellite`)
    REFERENCES `Satellites`
        (`idSatellite`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DisEqcMotorSatellite'

CREATE INDEX `IX_FK_DisEqcMotorSatellite` 
    ON `DisEqcMotors`
    (`idSatellite`);

-- Creating foreign key on `idProgram` in table 'PersonalTVGuideMaps'

ALTER TABLE `PersonalTVGuideMaps`
ADD CONSTRAINT `FK_ProgramPersonalTVGuideMap`
    FOREIGN KEY (`idProgram`)
    REFERENCES `Programs`
        (`idProgram`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramPersonalTVGuideMap'

CREATE INDEX `IX_FK_ProgramPersonalTVGuideMap` 
    ON `PersonalTVGuideMaps`
    (`idProgram`);

-- Creating foreign key on `idKeyword` in table 'PersonalTVGuideMaps'

ALTER TABLE `PersonalTVGuideMaps`
ADD CONSTRAINT `FK_KeywordPersonalTVGuideMap`
    FOREIGN KEY (`idKeyword`)
    REFERENCES `Keywords`
        (`idKeyword`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_KeywordPersonalTVGuideMap'

CREATE INDEX `IX_FK_KeywordPersonalTVGuideMap` 
    ON `PersonalTVGuideMaps`
    (`idKeyword`);

-- Creating foreign key on `idSchedule` in table 'Recordings'

ALTER TABLE `Recordings`
ADD CONSTRAINT `FK_ScheduleRecording`
    FOREIGN KEY (`idSchedule`)
    REFERENCES `Schedules`
        (`id_Schedule`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleRecording'

CREATE INDEX `IX_FK_ScheduleRecording` 
    ON `Recordings`
    (`idSchedule`);

-- Creating foreign key on `idKeyword` in table 'KeywordMaps'

ALTER TABLE `KeywordMaps`
ADD CONSTRAINT `FK_KeywordKeywordMap`
    FOREIGN KEY (`idKeyword`)
    REFERENCES `Keywords`
        (`idKeyword`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_KeywordKeywordMap'

CREATE INDEX `IX_FK_KeywordKeywordMap` 
    ON `KeywordMaps`
    (`idKeyword`);

-- Creating foreign key on `idChannelGroup` in table 'KeywordMaps'

ALTER TABLE `KeywordMaps`
ADD CONSTRAINT `FK_KeywordMapChannelGroup`
    FOREIGN KEY (`idChannelGroup`)
    REFERENCES `ChannelGroups`
        (`idGroup`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_KeywordMapChannelGroup'

CREATE INDEX `IX_FK_KeywordMapChannelGroup` 
    ON `KeywordMaps`
    (`idChannelGroup`);

-- Creating foreign key on `idRecording` in table 'RecordingCredits'

ALTER TABLE `RecordingCredits`
ADD CONSTRAINT `FK_RecordingRecordingCredit`
    FOREIGN KEY (`idRecording`)
    REFERENCES `Recordings`
        (`idRecording`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RecordingRecordingCredit'

CREATE INDEX `IX_FK_RecordingRecordingCredit` 
    ON `RecordingCredits`
    (`idRecording`);

-- Creating foreign key on `idSchedule` in table 'CanceledSchedules'

ALTER TABLE `CanceledSchedules`
ADD CONSTRAINT `FK_ScheduleCanceledSchedule`
    FOREIGN KEY (`idSchedule`)
    REFERENCES `Schedules`
        (`id_Schedule`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleCanceledSchedule'

CREATE INDEX `IX_FK_ScheduleCanceledSchedule` 
    ON `CanceledSchedules`
    (`idSchedule`);

-- Creating foreign key on `idLinkedChannel` in table 'ChannelLinkageMaps'

ALTER TABLE `ChannelLinkageMaps`
ADD CONSTRAINT `FK_ChannelLinkMap`
    FOREIGN KEY (`idLinkedChannel`)
    REFERENCES `Channels`
        (`idChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelLinkMap'

CREATE INDEX `IX_FK_ChannelLinkMap` 
    ON `ChannelLinkageMaps`
    (`idLinkedChannel`);

-- Creating foreign key on `idPortalChannel` in table 'ChannelLinkageMaps'

ALTER TABLE `ChannelLinkageMaps`
ADD CONSTRAINT `FK_ChannelPortalMap`
    FOREIGN KEY (`idPortalChannel`)
    REFERENCES `Channels`
        (`idChannel`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelPortalMap'

CREATE INDEX `IX_FK_ChannelPortalMap` 
    ON `ChannelLinkageMaps`
    (`idPortalChannel`);

-- Creating foreign key on `idProgramCategory` in table 'Recordings'

ALTER TABLE `Recordings`
ADD CONSTRAINT `FK_RecordingProgramCategory`
    FOREIGN KEY (`idProgramCategory`)
    REFERENCES `ProgramCategories`
        (`idProgramCategory`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RecordingProgramCategory'

CREATE INDEX `IX_FK_RecordingProgramCategory` 
    ON `Recordings`
    (`idProgramCategory`);

-- Creating foreign key on `idCard` in table 'Conflicts'

ALTER TABLE `Conflicts`
ADD CONSTRAINT `FK_CardConflict`
    FOREIGN KEY (`idCard`)
    REFERENCES `Cards`
        (`idCard`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CardConflict'

CREATE INDEX `IX_FK_CardConflict` 
    ON `Conflicts`
    (`idCard`);

-- Creating foreign key on `idChannel` in table 'Conflicts'

ALTER TABLE `Conflicts`
ADD CONSTRAINT `FK_ChannelConflict`
    FOREIGN KEY (`idChannel`)
    REFERENCES `Channels`
        (`idChannel`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChannelConflict'

CREATE INDEX `IX_FK_ChannelConflict` 
    ON `Conflicts`
    (`idChannel`);

-- Creating foreign key on `idSchedule` in table 'Conflicts'

ALTER TABLE `Conflicts`
ADD CONSTRAINT `FK_ScheduleConflict`
    FOREIGN KEY (`idSchedule`)
    REFERENCES `Schedules`
        (`id_Schedule`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleConflict'

CREATE INDEX `IX_FK_ScheduleConflict` 
    ON `Conflicts`
    (`idSchedule`);

-- Creating foreign key on `idConflictingSchedule` in table 'Conflicts'

ALTER TABLE `Conflicts`
ADD CONSTRAINT `FK_ScheduleConflict1`
    FOREIGN KEY (`idConflictingSchedule`)
    REFERENCES `Schedules`
        (`id_Schedule`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ScheduleConflict1'

CREATE INDEX `IX_FK_ScheduleConflict1` 
    ON `Conflicts`
    (`idConflictingSchedule`);

-- Creating foreign key on `idProgramCategory` in table 'Histories'

ALTER TABLE `Histories`
ADD CONSTRAINT `FK_ProgramCategoryHistory`
    FOREIGN KEY (`idProgramCategory`)
    REFERENCES `ProgramCategories`
        (`idProgramCategory`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProgramCategoryHistory'

CREATE INDEX `IX_FK_ProgramCategoryHistory` 
    ON `Histories`
    (`idProgramCategory`);

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
