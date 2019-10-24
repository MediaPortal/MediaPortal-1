-- -----------------------------------------------------
-- Table `canceledschedule`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `canceledschedule` (
  `idCanceledSchedule` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idSchedule` int NOT NULL ,
  `cancelDateTime` DATETIME NOT NULL ,
  `idChannel` int NOT NULL DEFAULT '0');

CREATE INDEX `FK_CanceledSchedule_Schedule` ON `canceledschedule` (`idSchedule` ASC) ;


-- -----------------------------------------------------
-- Table `card`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `card` (
  `idCard` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `devicePath` VARCHAR(2000) NOT NULL ,
  `name` VARCHAR(200) NOT NULL ,
  `priority` int NOT NULL ,
  `grabEPG` bit NOT NULL ,
  `lastEpgGrab` DATETIME NOT NULL ,
  `recordingFolder` VARCHAR(256) NOT NULL ,
  `idServer` int NOT NULL ,
  `enabled` bit NOT NULL ,
  `camType` int NOT NULL ,
  `timeshiftingFolder` VARCHAR(256) NOT NULL ,
  `recordingFormat` int NOT NULL ,
  `decryptLimit` int NOT NULL ,
  `preload` bit NOT NULL ,
  `CAM` bit NOT NULL ,
  `NetProvider` TINYINT(4) NOT NULL DEFAULT '0' ,
  `stopgraph` bit NOT NULL DEFAULT 1);

CREATE INDEX `FK_Card_Server` ON `card` (`idServer` ASC) ;


-- -----------------------------------------------------
-- Table `cardgroup`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `cardgroup` (
  `idCardGroup` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `name` VARCHAR(255) NOT NULL);


-- -----------------------------------------------------
-- Table `cardgroupmap`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `cardgroupmap` (
  `idMapping` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idCard` int NOT NULL ,
  `idCardGroup` int NOT NULL);

CREATE INDEX `FK_CardGroupMap_Card` ON `cardgroupmap` (`idCard` ASC) ;

CREATE INDEX `FK_CardGroupMap_CardGroup` ON `cardgroupmap` (`idCardGroup` ASC) ;


-- -----------------------------------------------------
-- Table `channel`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `channel` (
  `idChannel` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `isRadio` bit NOT NULL ,
  `isTv` bit NOT NULL ,
  `timesWatched` int NOT NULL ,
  `totalTimeWatched` DATETIME NOT NULL ,
  `grabEpg` bit NOT NULL ,
  `lastGrabTime` DATETIME NOT NULL ,
  `sortOrder` int NOT NULL ,
  `visibleInGuide` bit NOT NULL ,
  `externalId` VARCHAR(200) NOT NULL ,
  `displayName` VARCHAR(200) NOT NULL ,
  `epgHasGaps` bit NULL DEFAULT NULL,
  `channelNumber` int NOT NULL DEFAULT 10000);

CREATE INDEX `idxChannel` ON `channel` (`isTv` ASC, `sortOrder` ASC) ;

CREATE INDEX `idxChannelRadio` ON `channel` (`isRadio` ASC) ;


-- -----------------------------------------------------
-- Table `channelgroup`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `channelgroup` (
  `idGroup` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `groupName` VARCHAR(200) NOT NULL ,
  `sortOrder` int NOT NULL);

CREATE INDEX `IDX_ChannelGroup` ON `channelgroup` (`sortOrder` ASC) ;


-- -----------------------------------------------------
-- Table `channellinkagemap`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `channellinkagemap` (
  `idMapping` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idPortalChannel` int NOT NULL ,
  `idLinkedChannel` int NOT NULL ,
  `displayName` VARCHAR(200) NOT NULL);


-- -----------------------------------------------------
-- Table `channelmap`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `channelmap` (
  `idChannelMap` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idChannel` int NOT NULL ,
  `idCard` int NOT NULL ,
  `epgOnly` bit NULL DEFAULT NULL);

CREATE INDEX `FK_ChannelMap_Cards` ON `channelmap` (`idCard` ASC) ;

CREATE INDEX `FK_ChannelMap_Channels` ON `channelmap` (`idChannel` ASC) ;


-- -----------------------------------------------------
-- Table `conflict`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `conflict` (
  `idConflict` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idSchedule` int NOT NULL ,
  `idConflictingSchedule` int NOT NULL ,
  `idChannel` int NOT NULL ,
  `conflictDate` DATETIME NOT NULL ,
  `idCard` int NULL DEFAULT NULL);

CREATE INDEX `FK_Conflict_Channel` ON `conflict` (`idChannel` ASC) ;

CREATE INDEX `FK_Conflict_Schedule` ON `conflict` (`idSchedule` ASC) ;

CREATE INDEX `FK_Conflict_Schedule1` ON `conflict` (`idConflictingSchedule` ASC) ;


-- -----------------------------------------------------
-- Table `diseqcmotor`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `diseqcmotor` (
  `idDiSEqCMotor` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idCard` int NOT NULL ,
  `idSatellite` int NOT NULL ,
  `position` int NOT NULL);

CREATE INDEX `FK_DisEqcMotor_Satellite` ON `diseqcmotor` (`idSatellite` ASC) ;

CREATE INDEX `FK_DisEqcMotor_Card` ON `diseqcmotor` (`idCard` ASC) ;


-- -----------------------------------------------------
-- Table `favorite`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `favorite` (
  `idFavorite` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idProgram` int NOT NULL ,
  `priority` int NOT NULL ,
  `timesWatched` int NOT NULL);

CREATE INDEX `FK_Favorites_Programs` ON `favorite` (`idProgram` ASC) ;


-- -----------------------------------------------------
-- Table `groupmap`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `groupmap` (
  `idMap` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idGroup` int NOT NULL ,
  `idChannel` int NOT NULL ,
  `SortOrder` int NOT NULL);

CREATE INDEX `FK_GroupMap_Channel` ON `groupmap` (`idChannel` ASC) ;

CREATE INDEX `FK_GroupMap_ChannelGroup` ON `groupmap` (`idGroup` ASC) ;


-- -----------------------------------------------------
-- Table `history`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `history` (
  `idHistory` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idChannel` int NOT NULL ,
  `startTime` DATETIME NOT NULL ,
  `endTime` DATETIME NOT NULL ,
  `title` VARCHAR(1000) NOT NULL ,
  `description` TEXT NOT NULL ,
  `genre` VARCHAR(1000) NOT NULL ,
  `recorded` bit NOT NULL ,
  `watched` int NOT NULL);

CREATE INDEX `FK_History_Channel` ON `history` (`idChannel` ASC) ;


-- -----------------------------------------------------
-- Table `keyword`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `keyword` (
  `idKeyword` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `keywordName` VARCHAR(200) NOT NULL ,
  `rating` int NOT NULL ,
  `autoRecord` bit NOT NULL ,
  `searchIn` int NOT NULL);


-- -----------------------------------------------------
-- Table `keywordmap`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `keywordmap` (
  `idKeywordMap` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idKeyword` int NOT NULL ,
  `idChannelGroup` int NOT NULL);


-- -----------------------------------------------------
-- Table `pendingdeletion`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `pendingdeletion` (
  `idPendingDeletion` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `fileName` VARCHAR(260) NOT NULL);


-- -----------------------------------------------------
-- Table `personaltvguidemap`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `personaltvguidemap` (
  `idPersonalTVGuideMap` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idKeyword` int NOT NULL ,
  `idProgram` int NOT NULL);


-- -----------------------------------------------------
-- Table `program`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `program` (
  `idProgram` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idChannel` int NOT NULL ,
  `startTime` DATETIME NOT NULL ,
  `endTime` DATETIME NOT NULL ,
  `title` VARCHAR(2000) NOT NULL ,
  `description` TEXT NOT NULL ,
  `seriesNum` VARCHAR(200) NOT NULL ,
  `episodeNum` VARCHAR(200) NOT NULL ,
  `genre` VARCHAR(200) NOT NULL ,
  `originalAirDate` DATETIME NOT NULL ,
  `classification` VARCHAR(200) NOT NULL ,
  `starRating` int NOT NULL ,
  `parentalRating` int NOT NULL ,
  `episodeName` TEXT NOT NULL ,
  `episodePart` TEXT NOT NULL ,
  `state` int NOT NULL DEFAULT 0);

CREATE UNIQUE INDEX `idProgramBeginEnd` ON `program` (`idChannel` ASC, `startTime` ASC, `endTime` ASC) ;

CREATE INDEX `IDX_Program_State` ON `program` (`state` ASC) ;


-- -----------------------------------------------------
-- Table `radiochannelgroup`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `radiochannelgroup` (
  `idGroup` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `groupName` VARCHAR(200) NOT NULL ,
  `sortOrder` int NOT NULL);

CREATE INDEX `IDX_RadioChannelGroup` ON `radiochannelgroup` (`sortOrder` ASC) ;


-- -----------------------------------------------------
-- Table `radiogroupmap`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `radiogroupmap` (
  `idMap` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idGroup` int NOT NULL ,
  `idChannel` int NOT NULL ,
  `SortOrder` int NOT NULL);

CREATE INDEX `FK_RadioGroupMap_Channel` ON `radiogroupmap` (`idChannel` ASC) ;

CREATE INDEX `FK_RadioGroupMap_ChannelGroup` ON `radiogroupmap` (`idGroup` ASC) ;

CREATE INDEX `IDX_RadioGroupMap_SortOrder` ON `radiogroupmap` (`SortOrder` ASC) ;


-- -----------------------------------------------------
-- Table `recording`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `recording` (
  `idRecording` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idChannel` int NOT NULL ,
  `startTime` DATETIME NOT NULL ,
  `endTime` DATETIME NOT NULL ,
  `title` VARCHAR(2000) NOT NULL ,
  `description` VARCHAR(8000) NOT NULL ,
  `genre` VARCHAR(200) NOT NULL ,
  `fileName` VARCHAR(260) NOT NULL ,
  `keepUntil` int NOT NULL ,
  `keepUntilDate` DATETIME NOT NULL ,
  `timesWatched` int NOT NULL ,
  `idServer` int NOT NULL ,
  `stopTime` int NOT NULL ,
  `episodeName` TEXT NOT NULL ,
  `seriesNum` VARCHAR(200) NOT NULL ,
  `episodeNum` VARCHAR(200) NOT NULL ,
  `episodePart` TEXT NOT NULL ,
  `isRecording` bit NOT NULL DEFAULT 0 ,
  `idSchedule` int NOT NULL DEFAULT 0);

CREATE INDEX `FK_Recording_Server` ON `recording` (`idServer` ASC) ;

CREATE INDEX `FK_Recordings_Channels` ON `recording` (`idChannel` ASC) ;

CREATE INDEX `IDX_Recording_Filename` ON `recording` (`fileName` ASC) ;


-- -----------------------------------------------------
-- Table `satellite`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `satellite` (
  `idSatellite` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `satelliteName` VARCHAR(200) NOT NULL ,
  `transponderFileName` VARCHAR(200) NOT NULL);


-- -----------------------------------------------------
-- Table `schedule`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `schedule` (
  `id_Schedule` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idChannel` int NOT NULL ,
  `scheduleType` int NOT NULL ,
  `programName` VARCHAR(256) NOT NULL ,
  `startTime` DATETIME NOT NULL ,
  `endTime` DATETIME NOT NULL ,
  `maxAirings` int NOT NULL ,
  `priority` int NOT NULL ,
  `directory` VARCHAR(1024) NOT NULL ,
  `quality` int NOT NULL ,
  `keepMethod` int NOT NULL ,
  `keepDate` DATETIME NOT NULL ,
  `preRecordInterval` int NOT NULL ,
  `postRecordInterval` int NOT NULL ,
  `canceled` DATETIME NOT NULL ,
  `recommendedCard` int NOT NULL ,
  `series` bit NOT NULL ,
  `idParentSchedule` int NOT NULL DEFAULT '0');

CREATE INDEX `FK_Schedule_Channel` ON `schedule` (`idChannel` ASC) ;

CREATE INDEX `IDX_Schedule_ScheduleType` ON `schedule` (`scheduleType` ASC) ;

CREATE INDEX `IDX_Schedule_ProgramName` ON `schedule` (`programName` ASC) ;

CREATE INDEX `IDX_Schedule_StartTime` ON `schedule` (`startTime` ASC) ;

CREATE INDEX `IDX_Schedule_EndTime` ON `schedule` (`endTime` ASC) ;


-- -----------------------------------------------------
-- Table `server`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `server` (
  `idServer` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `isMaster` bit NOT NULL ,
  `hostName` VARCHAR(256) NOT NULL ,
  `rtspPort` int NOT NULL DEFAULT '554' );


-- -----------------------------------------------------
-- Table `setting`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `setting` (
  `idSetting` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `tag` VARCHAR(200) NOT NULL ,
  `value` VARCHAR(4096) NOT NULL);

CREATE INDEX `IDX_Setting_Tag` ON `setting` (`tag` ASC) ;


-- -----------------------------------------------------
-- Table `softwareencoder`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `softwareencoder` (
  `idEncoder` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `priority` int NOT NULL ,
  `name` VARCHAR(200) NOT NULL ,
  `type` int NOT NULL ,
  `reusable` bit NOT NULL DEFAULT 1);


-- -----------------------------------------------------
-- Table `timespan`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `timespan` (
  `idTimespan` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idKeyword` int NOT NULL ,
  `startTime` DATETIME NOT NULL ,
  `endTime` DATETIME NOT NULL ,
  `dayOfWeek` int NOT NULL);


-- -----------------------------------------------------
-- Table `tuningdetail`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `tuningdetail` (
  `idTuning` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idChannel` int NOT NULL ,
  `name` VARCHAR(200) NOT NULL ,
  `provider` VARCHAR(200) NOT NULL ,
  `channelType` int NOT NULL ,
  `channelNumber` int NOT NULL ,
  `frequency` int NOT NULL ,
  `countryId` int NOT NULL ,
  `isRadio` bit NOT NULL ,
  `isTv` bit NOT NULL ,
  `networkId` int NOT NULL ,
  `transportId` int NOT NULL ,
  `serviceId` int NOT NULL ,
  `pmtPid` int NOT NULL ,
  `freeToAir` bit NOT NULL ,
  `modulation` int NOT NULL ,
  `polarisation` int NOT NULL ,
  `symbolrate` int NOT NULL ,
  `diseqc` int NOT NULL ,
  `switchingFrequency` int NOT NULL ,
  `bandwidth` int NOT NULL ,
  `majorChannel` int NOT NULL ,
  `minorChannel` int NOT NULL ,
  `videoSource` int NOT NULL ,
  `tuningSource` int NOT NULL ,
  `band` int NOT NULL ,
  `satIndex` int NOT NULL ,
  `innerFecRate` int NOT NULL ,
  `pilot` int NOT NULL ,
  `rollOff` int NOT NULL ,
  `url` VARCHAR(200) NOT NULL ,
  `bitrate` int NOT NULL ,
  `audioSource` INT(1) NOT NULL ,
  `isVCRSignal` bit NOT NULL);

CREATE INDEX `IDX_TuningDetail1` ON `tuningdetail` (`idChannel` ASC) ;

CREATE INDEX `IDX_TuningDetail_Edit` ON `tuningdetail` (`networkId` ASC, `transportId` ASC, `serviceId` ASC) ;


-- -----------------------------------------------------
-- Table `tvmoviemapping`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `tvmoviemapping` (
  `idMapping` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `idChannel` int NOT NULL ,
  `stationName` VARCHAR(200) NOT NULL ,
  `timeSharingStart` VARCHAR(200) NOT NULL ,
  `timeSharingEnd` VARCHAR(200) NOT NULL );

CREATE INDEX `FK_TvMovieMapping_Channel` ON `tvmoviemapping` (`idChannel` ASC) ;


-- -----------------------------------------------------
-- Table `version`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `version` (
  `idVersion` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
  `versionNumber` int NOT NULL);


/*Data for the table "SoftwareEncoder" */

insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (1,1,'InterVideo Video Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (2,2,'Ulead MPEG Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (3,3,'MainConcept MPEG Video Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (4,4,'MainConcept Demo MPEG Video Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (5,5,'CyberLink MPEG Video Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (6,6,'CyberLink MPEG Video Encoder(Twinhan)',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (7,7,'MainConcept (Hauppauge) MPEG Video Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (8,8,'nanocosmos MPEG Video Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (9,9,'Pinnacle MPEG 2 Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (10,10,'MainConcept (HCW) MPEG-2 Video Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (11,11,'ATI MPEG Video Encoder',0);

insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (12,1,'InterVideo Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (13,2,'Ulead MPEG Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (14,3,'MainConcept MPEG Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (15,4,'MainConcept Demo MPEG Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (16,5,'CyberLink Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (17,6,'CyberLink Audio Encoder(Twinhan)',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (18,7,'Pinnacle MPEG Layer-2 Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (19,8,'MainConcept (Hauppauge) MPEG Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (20,9,'NVIDIA Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (21,10,'MainConcept (HCW) Layer II Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (22,11,'CyberLink MPEG Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (23,12,'ATI MPEG Audio Encoder',1);

INSERT INTO Setting (tag, value) VALUES ('softwareEncoderReuseLimit', '0');

insert into version(versionNumber) values (61);

