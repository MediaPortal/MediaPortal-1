



-- -----------------------------------------------------------
-- Entity Designer DDL Script for MySQL Server 4.1 and higher
-- -----------------------------------------------------------
-- Date Created: 08/21/2015 12:14:54
-- Generated from EDMX file: F:\sdev\Code\MediaPortal\MediaPortal-1_TVE35\TvEngine3\Mediaportal\TV\Server\TVDatabase\EntityModel\Model.edmx
-- Target version: 2.0.0.0
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- NOTE: if the constraint does not exist, an ignorable error will be reported.
-- --------------------------------------------------

--    ALTER TABLE `GroupMaps` DROP CONSTRAINT `FK_GroupMapChannelGroup`;
--    ALTER TABLE `GroupMaps` DROP CONSTRAINT `FK_GroupMapChannel`;
--    ALTER TABLE `DiseqcMotors` DROP CONSTRAINT `FK_DiseqcMotorTuner`;
--    ALTER TABLE `Recordings` DROP CONSTRAINT `FK_ChannelRecording`;
--    ALTER TABLE `Programs` DROP CONSTRAINT `FK_ChannelProgram`;
--    ALTER TABLE `ChannelMaps` DROP CONSTRAINT `FK_TunerChannelMap`;
--    ALTER TABLE `ChannelMaps` DROP CONSTRAINT `FK_ChannelChannelMap`;
--    ALTER TABLE `Schedules` DROP CONSTRAINT `FK_ChannelSchedule`;
--    ALTER TABLE `Schedules` DROP CONSTRAINT `FK_ScheduleParentSchedule`;
--    ALTER TABLE `Programs` DROP CONSTRAINT `FK_ProgramProgramCategory`;
--    ALTER TABLE `ProgramCredits` DROP CONSTRAINT `FK_ProgramProgramCredit`;
--    ALTER TABLE `Histories` DROP CONSTRAINT `FK_ChannelHistory`;
--    ALTER TABLE `TuningDetails` DROP CONSTRAINT `FK_ChannelTuningDetail`;
--    ALTER TABLE `DiseqcMotors` DROP CONSTRAINT `FK_DiseqcMotorSatellite`;
--    ALTER TABLE `Recordings` DROP CONSTRAINT `FK_ScheduleRecording`;
--    ALTER TABLE `RecordingCredits` DROP CONSTRAINT `FK_RecordingRecordingCredit`;
--    ALTER TABLE `CanceledSchedules` DROP CONSTRAINT `FK_ScheduleCanceledSchedule`;
--    ALTER TABLE `ChannelLinkageMaps` DROP CONSTRAINT `FK_ChannelLinkMap`;
--    ALTER TABLE `ChannelLinkageMaps` DROP CONSTRAINT `FK_ChannelPortalMap`;
--    ALTER TABLE `Recordings` DROP CONSTRAINT `FK_RecordingProgramCategory`;
--    ALTER TABLE `Conflicts` DROP CONSTRAINT `FK_TunerConflict`;
--    ALTER TABLE `Conflicts` DROP CONSTRAINT `FK_ChannelConflict`;
--    ALTER TABLE `Conflicts` DROP CONSTRAINT `FK_ScheduleConflict`;
--    ALTER TABLE `Conflicts` DROP CONSTRAINT `FK_ScheduleConflict1`;
--    ALTER TABLE `Histories` DROP CONSTRAINT `FK_ProgramCategoryHistory`;
--    ALTER TABLE `TuningDetails` DROP CONSTRAINT `FK_LnbTypeTuningDetail`;
--    ALTER TABLE `ProgramCategories` DROP CONSTRAINT `FK_GuideCategoryProgramCategory`;
--    ALTER TABLE `Tuners` DROP CONSTRAINT `FK_TunerTunerGroup`;
--    ALTER TABLE `TunerProperties` DROP CONSTRAINT `FK_TunerTunerProperty`;
--    ALTER TABLE `AnalogTunerSettings` DROP CONSTRAINT `FK_TunerAnalogTunerSettings`;
--    ALTER TABLE `AnalogTunerSettings` DROP CONSTRAINT `FK_VideoEncoderAnalogTunerSettings`;
--    ALTER TABLE `AnalogTunerSettings` DROP CONSTRAINT `FK_AudioEncoderAnalogTunerSettings`;

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------
SET foreign_key_checks = 0;
    DROP TABLE IF EXISTS `CanceledSchedules`;
    DROP TABLE IF EXISTS `Tuners`;
    DROP TABLE IF EXISTS `TunerGroups`;
    DROP TABLE IF EXISTS `Channels`;
    DROP TABLE IF EXISTS `ChannelGroups`;
    DROP TABLE IF EXISTS `ChannelLinkageMaps`;
    DROP TABLE IF EXISTS `ChannelMaps`;
    DROP TABLE IF EXISTS `Conflicts`;
    DROP TABLE IF EXISTS `DiseqcMotors`;
    DROP TABLE IF EXISTS `GroupMaps`;
    DROP TABLE IF EXISTS `Histories`;
    DROP TABLE IF EXISTS `PendingDeletions`;
    DROP TABLE IF EXISTS `Programs`;
    DROP TABLE IF EXISTS `ProgramCategories`;
    DROP TABLE IF EXISTS `ProgramCredits`;
    DROP TABLE IF EXISTS `Recordings`;
    DROP TABLE IF EXISTS `RuleBasedSchedules`;
    DROP TABLE IF EXISTS `Satellites`;
    DROP TABLE IF EXISTS `Schedules`;
    DROP TABLE IF EXISTS `ScheduleRulesTemplates`;
    DROP TABLE IF EXISTS `Settings`;
    DROP TABLE IF EXISTS `TuningDetails`;
    DROP TABLE IF EXISTS `Versions`;
    DROP TABLE IF EXISTS `LnbTypes`;
    DROP TABLE IF EXISTS `RecordingCredits`;
    DROP TABLE IF EXISTS `GuideCategories`;
    DROP TABLE IF EXISTS `TunerProperties`;
    DROP TABLE IF EXISTS `AnalogTunerSettings`;
    DROP TABLE IF EXISTS `VideoEncoders`;
    DROP TABLE IF EXISTS `AudioEncoders`;
SET foreign_key_checks = 1;

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

CREATE TABLE `CanceledSchedules`(
	`IdCanceledSchedule` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`IdSchedule` int NOT NULL, 
	`IdChannel` int NOT NULL, 
	`CancelDateTime` datetime NOT NULL);

ALTER TABLE `CanceledSchedules` ADD PRIMARY KEY (IdCanceledSchedule);




CREATE TABLE `Tuners`(
	`IdTuner` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`ExternalId` varchar (200) NOT NULL, 
	`Name` varchar (200) NOT NULL, 
	`Priority` int NOT NULL, 
	`IsEnabled` bool NOT NULL, 
	`UseForEpgGrabbing` bool NOT NULL, 
	`RecordingFolder` varchar (300) NOT NULL, 
	`TimeshiftingFolder` varchar (300) NOT NULL, 
	`SupportedBroadcastStandards` int NOT NULL, 
	`UseConditionalAccess` bool NOT NULL, 
	`CamType` int NOT NULL, 
	`DecryptLimit` int NOT NULL, 
	`MultiChannelDecryptMode` int NOT NULL, 
	`ConditionalAccessProviders` varchar (1000) NOT NULL, 
	`Preload` bool NOT NULL, 
	`BdaNetworkProvider` int NOT NULL, 
	`IdleMode` int NOT NULL, 
	`AlwaysSendDiseqcCommands` bool NOT NULL, 
	`PidFilterMode` int NOT NULL, 
	`UseCustomTuning` bool NOT NULL, 
	`IdTunerGroup` int, 
	`TsWriterInputDumpMask` int NOT NULL, 
	`TsMuxerInputDumpMask` int NOT NULL, 
	`DisableTsWriterCrcChecking` bool NOT NULL);

ALTER TABLE `Tuners` ADD PRIMARY KEY (IdTuner);




CREATE TABLE `TunerGroups`(
	`IdTunerGroup` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`Name` varchar (200) NOT NULL);

ALTER TABLE `TunerGroups` ADD PRIMARY KEY (IdTunerGroup);




CREATE TABLE `Channels`(
	`IdChannel` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`TimesWatched` int NOT NULL, 
	`TotalTimeWatched` datetime, 
	`LastGrabTime` datetime, 
	`VisibleInGuide` bool NOT NULL, 
	`ExternalId` varchar (200), 
	`Name` varchar (200) NOT NULL, 
	`MediaType` int NOT NULL, 
	`ChannelNumber` varchar (10) NOT NULL);

ALTER TABLE `Channels` ADD PRIMARY KEY (IdChannel);




CREATE TABLE `ChannelGroups`(
	`IdGroup` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`GroupName` varchar (200) NOT NULL, 
	`SortOrder` int NOT NULL, 
	`MediaType` int NOT NULL);

ALTER TABLE `ChannelGroups` ADD PRIMARY KEY (IdGroup);




CREATE TABLE `ChannelLinkageMaps`(
	`IdMapping` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`IdPortalChannel` int NOT NULL, 
	`IdLinkedChannel` int NOT NULL, 
	`Name` varchar (200) NOT NULL);

ALTER TABLE `ChannelLinkageMaps` ADD PRIMARY KEY (IdMapping);




CREATE TABLE `ChannelMaps`(
	`IdChannelMap` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`IdChannel` int NOT NULL, 
	`IdTuner` int NOT NULL);

ALTER TABLE `ChannelMaps` ADD PRIMARY KEY (IdChannelMap);




CREATE TABLE `Conflicts`(
	`IdConflict` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`IdSchedule` int NOT NULL, 
	`IdConflictingSchedule` int NOT NULL, 
	`IdChannel` int NOT NULL, 
	`ConflictDate` datetime NOT NULL, 
	`IdTuner` int);

ALTER TABLE `Conflicts` ADD PRIMARY KEY (IdConflict);




CREATE TABLE `DiseqcMotors`(
	`IdDiseqcMotor` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`IdTuner` int NOT NULL, 
	`IdSatellite` int NOT NULL, 
	`Position` int NOT NULL);

ALTER TABLE `DiseqcMotors` ADD PRIMARY KEY (IdDiseqcMotor);




CREATE TABLE `GroupMaps`(
	`IdMap` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`IdGroup` int NOT NULL, 
	`IdChannel` int NOT NULL, 
	`SortOrder` int NOT NULL);

ALTER TABLE `GroupMaps` ADD PRIMARY KEY (IdMap);




CREATE TABLE `Histories`(
	`IdHistory` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`IdChannel` int NOT NULL, 
	`StartTime` datetime NOT NULL, 
	`EndTime` datetime NOT NULL, 
	`Title` varchar (1000) NOT NULL, 
	`Description` varchar (1000) NOT NULL, 
	`Recorded` bool NOT NULL, 
	`Watched` int NOT NULL, 
	`IdProgramCategory` int);

ALTER TABLE `Histories` ADD PRIMARY KEY (IdHistory);




CREATE TABLE `PendingDeletions`(
	`IdPendingDeletion` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`FileName` longtext NOT NULL);

ALTER TABLE `PendingDeletions` ADD PRIMARY KEY (IdPendingDeletion);




CREATE TABLE `Programs`(
	`IdProgram` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`IdChannel` int NOT NULL, 
	`StartTime` datetime NOT NULL, 
	`EndTime` datetime NOT NULL, 
	`Title` varchar (2000) NOT NULL, 
	`Description` varchar (8000) NOT NULL, 
	`EpisodeName` varchar (2000), 
	`SeriesId` varchar (200), 
	`SeasonNumber` int, 
	`EpisodeId` varchar (200), 
	`EpisodeNumber` int, 
	`EpisodePartNumber` int, 
	`IsPreviouslyShown` bool, 
	`OriginalAirDate` datetime, 
	`IdProgramCategory` int, 
	`Classification` varchar (200), 
	`Advisories` int NOT NULL, 
	`IsHighDefinition` bool, 
	`IsThreeDimensional` bool, 
	`AudioLanguages` varchar (50), 
	`SubtitlesLanguages` varchar (50), 
	`IsLive` bool, 
	`ProductionYear` int, 
	`ProductionCountry` varchar (200), 
	`StarRating` decimal( 10, 2 ) , 
	`StarRatingMaximum` decimal( 10, 2 ) , 
	`State` int NOT NULL, 
	`StartTimeDayOfWeek` smallint NOT NULL, 
	`EndTimeDayOfWeek` smallint NOT NULL, 
	`EndTimeOffset` datetime NOT NULL, 
	`StartTimeOffset` datetime NOT NULL);

ALTER TABLE `Programs` ADD PRIMARY KEY (IdProgram);




CREATE TABLE `ProgramCategories`(
	`IdProgramCategory` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`Category` varchar (50) NOT NULL, 
	`IdGuideCategory` int);

ALTER TABLE `ProgramCategories` ADD PRIMARY KEY (IdProgramCategory);




CREATE TABLE `ProgramCredits`(
	`IdProgramCredit` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`IdProgram` int NOT NULL, 
	`Person` varchar (200) NOT NULL, 
	`Role` varchar (50) NOT NULL);

ALTER TABLE `ProgramCredits` ADD PRIMARY KEY (IdProgramCredit);




CREATE TABLE `Recordings`(
	`IdRecording` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`IdChannel` int, 
	`StartTime` datetime NOT NULL, 
	`EndTime` datetime NOT NULL, 
	`Title` varchar (2000) NOT NULL, 
	`Description` varchar (8000) NOT NULL, 
	`FileName` varchar (260) NOT NULL, 
	`KeepUntil` int NOT NULL, 
	`KeepUntilDate` datetime, 
	`TimesWatched` int NOT NULL, 
	`StopTime` int NOT NULL, 
	`EpisodeName` longtext NOT NULL, 
	`SeriesNum` varchar (200) NOT NULL, 
	`EpisodeNum` varchar (200) NOT NULL, 
	`EpisodePart` longtext NOT NULL, 
	`IsRecording` bool NOT NULL, 
	`IdSchedule` int, 
	`MediaType` int NOT NULL, 
	`IdProgramCategory` int);

ALTER TABLE `Recordings` ADD PRIMARY KEY (IdRecording);




CREATE TABLE `RuleBasedSchedules`(
	`IdRuleBasedSchedule` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`ScheduleName` varchar (256) NOT NULL, 
	`MaxAirings` int NOT NULL, 
	`Priority` int NOT NULL, 
	`Directory` varchar (1024) NOT NULL, 
	`Quality` int NOT NULL, 
	`KeepMethod` int NOT NULL, 
	`KeepDate` datetime, 
	`PreRecordInterval` int NOT NULL, 
	`PostRecordInterval` int NOT NULL, 
	`Rules` longtext);

ALTER TABLE `RuleBasedSchedules` ADD PRIMARY KEY (IdRuleBasedSchedule);




CREATE TABLE `Satellites`(
	`IdSatellite` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`Longitude` int NOT NULL, 
	`Name` varchar (200) NOT NULL);

ALTER TABLE `Satellites` ADD PRIMARY KEY (IdSatellite);




CREATE TABLE `Schedules`(
	`IdSchedule` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`IdChannel` int NOT NULL, 
	`ScheduleType` int NOT NULL, 
	`ProgramName` varchar (256) NOT NULL, 
	`StartTime` datetime NOT NULL, 
	`EndTime` datetime NOT NULL, 
	`MaxAirings` int NOT NULL, 
	`Priority` int NOT NULL, 
	`Directory` varchar (1024) NOT NULL, 
	`Quality` int NOT NULL, 
	`KeepMethod` int NOT NULL, 
	`KeepDate` datetime, 
	`PreRecordInterval` int, 
	`PostRecordInterval` int, 
	`Canceled` datetime NOT NULL, 
	`Series` bool NOT NULL, 
	`IdParentSchedule` int);

ALTER TABLE `Schedules` ADD PRIMARY KEY (IdSchedule);




CREATE TABLE `ScheduleRulesTemplates`(
	`IdScheduleRulesTemplate` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`Name` varchar (50) NOT NULL, 
	`Rules` longtext NOT NULL, 
	`Enabled` bool NOT NULL, 
	`Usages` int NOT NULL, 
	`Editable` bool NOT NULL);

ALTER TABLE `ScheduleRulesTemplates` ADD PRIMARY KEY (IdScheduleRulesTemplate);




CREATE TABLE `Settings`(
	`IdSetting` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`Tag` varchar (200) NOT NULL, 
	`Value` varchar (4096) NOT NULL);

ALTER TABLE `Settings` ADD PRIMARY KEY (IdSetting);




CREATE TABLE `TuningDetails`(
	`IdTuning` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`IdChannel` int NOT NULL, 
	`MediaType` int NOT NULL, 
	`Priority` int NOT NULL, 
	`BroadcastStandard` int NOT NULL, 
	`Name` varchar (200) NOT NULL, 
	`Provider` varchar (200) NOT NULL, 
	`LogicalChannelNumber` varchar (10) NOT NULL, 
	`IsEncrypted` bool NOT NULL, 
	`IsHighDefinition` bool NOT NULL, 
	`IsThreeDimensional` bool NOT NULL, 
	`OriginalNetworkId` int NOT NULL, 
	`TransportStreamId` int NOT NULL, 
	`ServiceId` int NOT NULL, 
	`FreesatChannelId` int NOT NULL, 
	`OpenTvChannelId` int NOT NULL, 
	`EpgOriginalNetworkId` int NOT NULL, 
	`EpgTransportStreamId` int NOT NULL, 
	`EpgServiceId` int NOT NULL, 
	`SourceId` int NOT NULL, 
	`PmtPid` int NOT NULL, 
	`PhysicalChannelNumber` int NOT NULL, 
	`Frequency` int NOT NULL, 
	`CountryId` int NOT NULL, 
	`Modulation` int NOT NULL, 
	`Polarisation` int NOT NULL, 
	`SymbolRate` int NOT NULL, 
	`DiSEqC` int NOT NULL, 
	`Bandwidth` int NOT NULL, 
	`VideoSource` int NOT NULL, 
	`TuningSource` int NOT NULL, 
	`SatIndex` int NOT NULL, 
	`FecCodeRate` int NOT NULL, 
	`PilotTonesState` int NOT NULL, 
	`RollOffFactor` int NOT NULL, 
	`StreamId` int NOT NULL, 
	`Url` varchar (200) NOT NULL, 
	`AudioSource` int NOT NULL, 
	`IsVcrSignal` bool NOT NULL, 
	`IdLnbType` int);

ALTER TABLE `TuningDetails` ADD PRIMARY KEY (IdTuning);




CREATE TABLE `Versions`(
	`IdVersion` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`VersionNumber` int NOT NULL);

ALTER TABLE `Versions` ADD PRIMARY KEY (IdVersion);




CREATE TABLE `LnbTypes`(
	`IdLnbType` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`Name` varchar (200) NOT NULL, 
	`LowBandFrequency` int NOT NULL, 
	`HighBandFrequency` int NOT NULL, 
	`SwitchFrequency` int NOT NULL, 
	`IsBandStacked` bool NOT NULL);

ALTER TABLE `LnbTypes` ADD PRIMARY KEY (IdLnbType);




CREATE TABLE `RecordingCredits`(
	`IdRecordingCredit` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`IdRecording` int NOT NULL, 
	`Person` varchar (200) NOT NULL, 
	`Role` varchar (50) NOT NULL);

ALTER TABLE `RecordingCredits` ADD PRIMARY KEY (IdRecordingCredit);




CREATE TABLE `GuideCategories`(
	`IdGuideCategory` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`Name` varchar (1000) NOT NULL, 
	`IsMovie` bool NOT NULL, 
	`IsEnabled` bool NOT NULL);

ALTER TABLE `GuideCategories` ADD PRIMARY KEY (IdGuideCategory);




CREATE TABLE `TunerProperties`(
	`IdTunerProperty` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`PropertyId` int NOT NULL, 
	`Value` int NOT NULL, 
	`Default` int NOT NULL, 
	`Minimum` int NOT NULL, 
	`Maximum` int NOT NULL, 
	`Step` int NOT NULL, 
	`PossibleValueFlags` int NOT NULL, 
	`ValueFlags` int NOT NULL, 
	`IdTuner` int NOT NULL);

ALTER TABLE `TunerProperties` ADD PRIMARY KEY (IdTunerProperty);




CREATE TABLE `AnalogTunerSettings`(
	`IdAnalogTunerSettings` int NOT NULL, 
	`VideoStandard` int NOT NULL, 
	`SupportedVideoStandards` int NOT NULL, 
	`FrameSize` int NOT NULL, 
	`SupportedFrameSizes` int NOT NULL, 
	`FrameRate` int NOT NULL, 
	`SupportedFrameRates` int NOT NULL, 
	`IdVideoEncoder` int, 
	`IdAudioEncoder` int, 
	`EncoderBitRateModeTimeShifting` int NOT NULL, 
	`EncoderBitRateTimeShifting` int NOT NULL, 
	`EncoderBitRatePeakTimeShifting` int NOT NULL, 
	`EncoderBitRateModeRecording` int NOT NULL, 
	`EncoderBitRateRecording` int NOT NULL, 
	`EncoderBitRatePeakRecording` int NOT NULL, 
	`ExternalInputSourceVideo` int NOT NULL, 
	`ExternalInputSourceAudio` int NOT NULL, 
	`ExternalInputCountryId` int NOT NULL, 
	`ExternalInputPhysicalChannelNumber` int NOT NULL, 
	`ExternalTunerProgram` varchar (300) NOT NULL, 
	`ExternalTunerProgramArguments` varchar (200) NOT NULL, 
	`SupportedVideoSources` int NOT NULL, 
	`SupportedAudioSources` int NOT NULL);

ALTER TABLE `AnalogTunerSettings` ADD PRIMARY KEY (IdAnalogTunerSettings);




CREATE TABLE `VideoEncoders`(
	`IdVideoEncoder` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`Priority` int NOT NULL, 
	`Name` varchar (200) NOT NULL, 
	`IsCombined` bool NOT NULL, 
	`ClassId` varchar (50) NOT NULL);

ALTER TABLE `VideoEncoders` ADD PRIMARY KEY (IdVideoEncoder);




CREATE TABLE `AudioEncoders`(
	`IdAudioEncoder` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`Priority` int NOT NULL, 
	`Name` varchar (200) NOT NULL, 
	`ClassId` varchar (50) NOT NULL);

ALTER TABLE `AudioEncoders` ADD PRIMARY KEY (IdAudioEncoder);






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

-- Creating foreign key on `IdTuner` in table 'DiseqcMotors'

ALTER TABLE `DiseqcMotors`
ADD CONSTRAINT `FK_DiseqcMotorTuner`
    FOREIGN KEY (`IdTuner`)
    REFERENCES `Tuners`
        (`IdTuner`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DiseqcMotorTuner'

CREATE INDEX `IX_FK_DiseqcMotorTuner` 
    ON `DiseqcMotors`
    (`IdTuner`);

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

-- Creating foreign key on `IdTuner` in table 'ChannelMaps'

ALTER TABLE `ChannelMaps`
ADD CONSTRAINT `FK_TunerChannelMap`
    FOREIGN KEY (`IdTuner`)
    REFERENCES `Tuners`
        (`IdTuner`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TunerChannelMap'

CREATE INDEX `IX_FK_TunerChannelMap` 
    ON `ChannelMaps`
    (`IdTuner`);

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

-- Creating foreign key on `IdSatellite` in table 'DiseqcMotors'

ALTER TABLE `DiseqcMotors`
ADD CONSTRAINT `FK_DiseqcMotorSatellite`
    FOREIGN KEY (`IdSatellite`)
    REFERENCES `Satellites`
        (`IdSatellite`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DiseqcMotorSatellite'

CREATE INDEX `IX_FK_DiseqcMotorSatellite` 
    ON `DiseqcMotors`
    (`IdSatellite`);

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

-- Creating foreign key on `IdTuner` in table 'Conflicts'

ALTER TABLE `Conflicts`
ADD CONSTRAINT `FK_TunerConflict`
    FOREIGN KEY (`IdTuner`)
    REFERENCES `Tuners`
        (`IdTuner`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TunerConflict'

CREATE INDEX `IX_FK_TunerConflict` 
    ON `Conflicts`
    (`IdTuner`);

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

-- Creating foreign key on `IdGuideCategory` in table 'ProgramCategories'

ALTER TABLE `ProgramCategories`
ADD CONSTRAINT `FK_GuideCategoryProgramCategory`
    FOREIGN KEY (`IdGuideCategory`)
    REFERENCES `GuideCategories`
        (`IdGuideCategory`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GuideCategoryProgramCategory'

CREATE INDEX `IX_FK_GuideCategoryProgramCategory` 
    ON `ProgramCategories`
    (`IdGuideCategory`);

-- Creating foreign key on `IdTunerGroup` in table 'Tuners'

ALTER TABLE `Tuners`
ADD CONSTRAINT `FK_TunerTunerGroup`
    FOREIGN KEY (`IdTunerGroup`)
    REFERENCES `TunerGroups`
        (`IdTunerGroup`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TunerTunerGroup'

CREATE INDEX `IX_FK_TunerTunerGroup` 
    ON `Tuners`
    (`IdTunerGroup`);

-- Creating foreign key on `IdTuner` in table 'TunerProperties'

ALTER TABLE `TunerProperties`
ADD CONSTRAINT `FK_TunerTunerProperty`
    FOREIGN KEY (`IdTuner`)
    REFERENCES `Tuners`
        (`IdTuner`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TunerTunerProperty'

CREATE INDEX `IX_FK_TunerTunerProperty` 
    ON `TunerProperties`
    (`IdTuner`);

-- Creating foreign key on `IdAnalogTunerSettings` in table 'AnalogTunerSettings'

ALTER TABLE `AnalogTunerSettings`
ADD CONSTRAINT `FK_TunerAnalogTunerSettings`
    FOREIGN KEY (`IdAnalogTunerSettings`)
    REFERENCES `Tuners`
        (`IdTuner`)
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating foreign key on `IdVideoEncoder` in table 'AnalogTunerSettings'

ALTER TABLE `AnalogTunerSettings`
ADD CONSTRAINT `FK_VideoEncoderAnalogTunerSettings`
    FOREIGN KEY (`IdVideoEncoder`)
    REFERENCES `VideoEncoders`
        (`IdVideoEncoder`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_VideoEncoderAnalogTunerSettings'

CREATE INDEX `IX_FK_VideoEncoderAnalogTunerSettings` 
    ON `AnalogTunerSettings`
    (`IdVideoEncoder`);

-- Creating foreign key on `IdAudioEncoder` in table 'AnalogTunerSettings'

ALTER TABLE `AnalogTunerSettings`
ADD CONSTRAINT `FK_AudioEncoderAnalogTunerSettings`
    FOREIGN KEY (`IdAudioEncoder`)
    REFERENCES `AudioEncoders`
        (`IdAudioEncoder`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_AudioEncoderAnalogTunerSettings'

CREATE INDEX `IX_FK_AudioEncoderAnalogTunerSettings` 
    ON `AnalogTunerSettings`
    (`IdAudioEncoder`);

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
