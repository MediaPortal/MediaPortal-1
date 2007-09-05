-- MySQL Administrator dump 1.4
--
-- ------------------------------------------------------
-- Server version	5.0.26-community-nt


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;


--
-- Create schema TvLibrary
--

CREATE DATABASE IF NOT EXISTS TvLibrary;
USE TvLibrary;
#
--
-- Definition of table `CanceledSchedule`
--

DROP TABLE IF EXISTS `CanceledSchedule`;
CREATE TABLE `CanceledSchedule` (
  `idCanceledSchedule` int(11) NOT NULL auto_increment,
  `idSchedule` int(11) NOT NULL,
  `cancelDateTime` datetime NOT NULL,
  PRIMARY KEY  (`idCanceledSchedule`),
  KEY `FK_CanceledSchedule_Schedule` (`idSchedule`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#
--
-- Dumping data for table `CanceledSchedule`
--

/*!40000 ALTER TABLE `CanceledSchedule` DISABLE KEYS */;
/*!40000 ALTER TABLE `CanceledSchedule` ENABLE KEYS */;


--
-- Definition of table `Card`
--

DROP TABLE IF EXISTS `Card`;
CREATE TABLE `Card` (
  `idCard` int(11) NOT NULL auto_increment,
  `devicePath` varchar(2000) NOT NULL,
  `name` varchar(200) NOT NULL,
  `priority` int(11) NOT NULL,
  `grabEPG` bit(1) NOT NULL,
  `lastEpgGrab` datetime NOT NULL,
  `recordingFolder` varchar(256) NOT NULL,
  `idServer` int(11) NOT NULL,
  `enabled` bit(1) NOT NULL,
  `camType` int(11) NOT NULL,
  `timeshiftingFolder` varchar(256) NOT NULL,
  `recordingFormat` int(11) NOT NULL,
  `decryptLimit` int(11) NOT NULL,
  PRIMARY KEY  (`idCard`),
  KEY `FK_Card_Server` (`idServer`)
) ENGINE=MyISAM AUTO_INCREMENT=5 DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `CardGroup`
--

DROP TABLE IF EXISTS `CardGroup`;
CREATE TABLE `CardGroup` (
  `idCardGroup` int(11) NOT NULL auto_increment,
  `name` varchar(255) NOT NULL,
  PRIMARY KEY  (`idCardGroup`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `CardGroupMap`
--

DROP TABLE IF EXISTS `CardGroupMap`;
CREATE TABLE `CardGroupMap` (
  `idMapping` int(11) NOT NULL auto_increment,
  `idCard` int(11) NOT NULL,
  `idCardGroup` int(11) NOT NULL,
  PRIMARY KEY  (`idMapping`),
  KEY `FK_CardGroupMap_Card` (`idCard`),
  KEY `FK_CardGroupMap_CardGroup` (`idCardGroup`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `Channel`
--

DROP TABLE IF EXISTS `Channel`;
CREATE TABLE `Channel` (
  `idChannel` int(11) NOT NULL auto_increment,
  `name` varchar(200) NOT NULL,
  `isRadio` bit(1) NOT NULL,
  `isTv` bit(1) NOT NULL,
  `timesWatched` int(11) NOT NULL,
  `totalTimeWatched` datetime NOT NULL,
  `grabEpg` bit(1) NOT NULL,
  `lastGrabTime` datetime NOT NULL,
  `sortOrder` int(11) NOT NULL,
  `visibleInGuide` bit(1) NOT NULL,
  `externalId` varchar(200) NOT NULL,
  `freetoair` bit(1) NOT NULL,
  `displayName` varchar(200) NOT NULL,
  PRIMARY KEY  (`idChannel`),
  KEY `IDX_Channel1` (`isTv`,`sortOrder`)
) ENGINE=MyISAM AUTO_INCREMENT=383 DEFAULT CHARSET=latin1;
#

--
-- Definition of table `ChannelGroup`
--

DROP TABLE IF EXISTS `ChannelGroup`;
CREATE TABLE `ChannelGroup` (
  `idGroup` int(11) NOT NULL auto_increment,
  `groupName` varchar(200) NOT NULL,
  `sortOrder` int(11) NOT NULL,
  PRIMARY KEY  (`idGroup`),
  KEY `IDX_ChannelGroup` (`sortOrder`)
) ENGINE=MyISAM AUTO_INCREMENT=3 DEFAULT CHARSET=latin1;
#

--
-- Definition of table `ChannelMap`
--

DROP TABLE IF EXISTS `ChannelMap`;
CREATE TABLE `ChannelMap` (
  `idChannelMap` int(11) NOT NULL auto_increment,
  `idChannel` int(11) NOT NULL,
  `idCard` int(11) NOT NULL,
  PRIMARY KEY  (`idChannelMap`),
  KEY `FK_ChannelMap_Cards` (`idCard`),
  KEY `FK_ChannelMap_Channels` (`idChannel`)
) ENGINE=MyISAM AUTO_INCREMENT=289 DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `Conflict`
--

DROP TABLE IF EXISTS `Conflict`;
CREATE TABLE `Conflict` (
  `idConflict` int(11) NOT NULL auto_increment,
  `idSchedule` int(11) NOT NULL,
  `idConflictingSchedule` int(11) NOT NULL,
  `idChannel` int(11) NOT NULL,
  `conflictDate` datetime NOT NULL,
  `idCard` int(11) default NULL,
  PRIMARY KEY  (`idConflict`),
  KEY `FK_Conflict_Channel` (`idChannel`),
  KEY `FK_Conflict_Schedule` (`idSchedule`),
  KEY `FK_Conflict_Schedule1` (`idConflictingSchedule`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `DiSEqCMotor`
--

DROP TABLE IF EXISTS `DiSEqCMotor`;
CREATE TABLE `DiSEqCMotor` (
  `idDiSEqCMotor` int(11) NOT NULL auto_increment,
  `idCard` int(11) NOT NULL,
  `idSatellite` int(11) NOT NULL,
  `position` int(11) NOT NULL,
  PRIMARY KEY  (`idDiSEqCMotor`),
  KEY `FK_DisEqcMotor_Satellite` (`idSatellite`),
  KEY `FK_DisEqcMotor_Card` (`idCard`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `Favorite`
--

DROP TABLE IF EXISTS `Favorite`;
CREATE TABLE `Favorite` (
  `idFavorite` int(11) NOT NULL auto_increment,
  `idProgram` int(11) NOT NULL,
  `priority` int(11) NOT NULL,
  `timesWatched` int(11) NOT NULL,
  PRIMARY KEY  (`idFavorite`),
  KEY `FK_Favorites_Programs` (`idProgram`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `GroupMap`
--

DROP TABLE IF EXISTS `GroupMap`;
CREATE TABLE `GroupMap` (
  `idMap` int(11) NOT NULL auto_increment,
  `idGroup` int(11) NOT NULL,
  `idChannel` int(11) NOT NULL,
  `SortOrder` int(11) NOT NULL,
  PRIMARY KEY  (`idMap`),
  KEY `FK_GroupMap_Channel` (`idChannel`),
  KEY `FK_GroupMap_ChannelGroup` (`idGroup`)
) ENGINE=MyISAM AUTO_INCREMENT=145 DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `Program`
--

DROP TABLE IF EXISTS `Program`;
CREATE TABLE `Program` (
  `idProgram` int(11) NOT NULL auto_increment,
  `idChannel` int(11) NOT NULL,
  `startTime` datetime NOT NULL,
  `endTime` datetime NOT NULL,
  `title` varchar(2000) NOT NULL,
  `description` varchar(4000) NOT NULL,
  `seriesNum` varchar(200) NOT NULL,
  `episodeNum` varchar(200) NOT NULL,
  `genre` varchar(200) NOT NULL,
  `originalAirDate` datetime NOT NULL,
  `classification` varchar(200) NOT NULL,
  `starRating` int(11) NOT NULL,
  `notify` bit(1) NOT NULL,
  `parentalRating` int(11) NOT NULL,
  PRIMARY KEY  (`idProgram`),
  KEY `IDX_StartTime` (`startTime`),
  KEY `IDX_Program1` (`idChannel`),
  KEY `IDX_Program2` (`idChannel`,`startTime`,`endTime`)
) ENGINE=MyISAM AUTO_INCREMENT=26096 DEFAULT CHARSET=latin1;
#

--
-- Definition of table `Recording`
--

DROP TABLE IF EXISTS `Recording`;
CREATE TABLE `Recording` (
  `idRecording` int(11) NOT NULL auto_increment,
  `idChannel` int(11) NOT NULL,
  `startTime` datetime NOT NULL,
  `endTime` datetime NOT NULL,
  `title` varchar(200) NOT NULL,
  `description` varchar(4000) NOT NULL,
  `genre` varchar(200) NOT NULL,
  `fileName` varchar(1024) NOT NULL,
  `keepUntil` int(11) NOT NULL,
  `keepUntilDate` datetime NOT NULL,
  `timesWatched` int(11) NOT NULL,
  `idServer` int(11) NOT NULL,
  `stopTime` int(11) NOT NULL,
  PRIMARY KEY  (`idRecording`),
  KEY `FK_Recording_Server` (`idServer`),
  KEY `FK_Recordings_Channels` (`idChannel`)
) ENGINE=MyISAM AUTO_INCREMENT=43 DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `Satellite`
--

DROP TABLE IF EXISTS `Satellite`;
CREATE TABLE `Satellite` (
  `idSatellite` int(11) NOT NULL auto_increment,
  `satelliteName` varchar(200) NOT NULL,
  `transponderFileName` varchar(200) NOT NULL,
  PRIMARY KEY  (`idSatellite`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `Schedule`
--

DROP TABLE IF EXISTS `Schedule`;
CREATE TABLE `Schedule` (
  `id_Schedule` int(11) NOT NULL auto_increment,
  `idChannel` int(11) NOT NULL,
  `scheduleType` int(11) NOT NULL,
  `programName` varchar(256) NOT NULL,
  `startTime` datetime NOT NULL,
  `endTime` datetime NOT NULL,
  `maxAirings` int(11) NOT NULL,
  `priority` int(11) NOT NULL,
  `directory` varchar(1024) NOT NULL,
  `quality` int(11) NOT NULL,
  `keepMethod` int(11) NOT NULL,
  `keepDate` datetime NOT NULL,
  `preRecordInterval` int(11) NOT NULL,
  `postRecordInterval` int(11) NOT NULL,
  `canceled` datetime NOT NULL,
  `recommendedCard` int(11) NOT NULL,
  PRIMARY KEY  (`id_Schedule`),
  KEY `FK_Schedule_Channel` (`idChannel`)
) ENGINE=MyISAM AUTO_INCREMENT=43 DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `Server`
--

DROP TABLE IF EXISTS `Server`;
CREATE TABLE `Server` (
  `idServer` int(11) NOT NULL auto_increment,
  `isMaster` bit(1) NOT NULL,
  `hostName` varchar(256) NOT NULL,
  PRIMARY KEY  (`idServer`)
) ENGINE=MyISAM AUTO_INCREMENT=3 DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `Setting`
--

DROP TABLE IF EXISTS `Setting`;
CREATE TABLE `Setting` (
  `idSetting` int(11) NOT NULL auto_increment,
  `tag` varchar(200) NOT NULL,
  `value` varchar(4096) NOT NULL,
  PRIMARY KEY  (`idSetting`)
) ENGINE=MyISAM AUTO_INCREMENT=68 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
#

--
-- Definition of table `ChannelLinkageMap`
--
DROP TABLE IF EXISTS `ChannelLinkageMap`;
CREATE TABLE `ChannelLinkageMap` (
  `idMapping` int(11) NOT NULL auto_increment,
  `idPortalChannel` int(11) NOT NULL,
  `idLinkedChannel` int(11) NOT NULL,
  PRIMARY KEY (`idMapping`)
) ENGINE=MyISAM AUTO_INCREMENT=68 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
#

--
-- Definition of table `TuningDetail`
--

DROP TABLE IF EXISTS `TuningDetail`;
CREATE TABLE `TuningDetail` (
  `idTuning` int(11) NOT NULL auto_increment,
  `idChannel` int(11) NOT NULL,
  `name` varchar(200) NOT NULL,
  `provider` varchar(200) NOT NULL,
  `channelType` int(11) NOT NULL,
  `channelNumber` int(11) NOT NULL,
  `frequency` int(11) NOT NULL,
  `countryId` int(11) NOT NULL,
  `isRadio` bit(1) NOT NULL,
  `isTv` bit(1) NOT NULL,
  `networkId` int(11) NOT NULL,
  `transportId` int(11) NOT NULL,
  `serviceId` int(11) NOT NULL,
  `pmtPid` int(11) NOT NULL,
  `freeToAir` bit(1) NOT NULL,
  `modulation` int(11) NOT NULL,
  `polarisation` int(11) NOT NULL,
  `symbolrate` int(11) NOT NULL,
  `diseqc` int(11) NOT NULL,
  `switchingFrequency` int(11) NOT NULL,
  `bandwidth` int(11) NOT NULL,
  `majorChannel` int(11) NOT NULL,
  `minorChannel` int(11) NOT NULL,
  `pcrPid` int(11) NOT NULL,
  `videoSource` int(11) NOT NULL,
  `tuningSource` int(11) NOT NULL,
  `videoPid` int(11) NOT NULL,
  `audioPid` int(11) NOT NULL,
  `band` int(11) NOT NULL,
  `satIndex` int(11) NOT NULL,
  `innerFecRate` int(11) NOT NULL,
  `pilot` int(11) NOT NULL,
  `rollOff` int(11) NOT NULL,
  PRIMARY KEY  (`idTuning`),
  KEY `IDX_TuningDetail1` (`idChannel`)
) ENGINE=MyISAM AUTO_INCREMENT=396 DEFAULT CHARSET=latin1;
#

--
-- Definition of table `TvMovieMapping`
--

DROP TABLE IF EXISTS `TvMovieMapping`;
CREATE TABLE `TvMovieMapping`(
	`idMapping` int(11) NOT NULL auto_increment,
	`idChannel` int(11) NOT NULL,
	`stationName` varchar(200) NOT NULL,
	`timeSharingStart` varchar(200) NOT NULL,
	`timeSharingEnd` varchar(200) NOT NULL,
  PRIMARY KEY  (`idMapping`),
  KEY `FK_TvMovieMapping_Channel` (`idChannel`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
#

DROP TABLE IF EXISTS `History`;
CREATE TABLE `History`(
	`idHistory` int NOT NULL auto_increment,
	`idChannel` int NOT NULL,
	`startTime` datetime NOT NULL,
	`endTime` datetime NOT NULL,
	`title` varchar(1000) NOT NULL,
	`description` varchar(1000) NOT NULL,
	`genre` varchar(1000) NOT NULL,
	`recorded` bit NOT NULL,
	`watched` int NOT NULL,
  PRIMARY KEY  (`idHistory`),
  KEY `FK_History_Channel` (`idChannel`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
#

--
-- Definition of table `Keyword`
--

DROP TABLE IF EXISTS `Keyword`;
CREATE TABLE `Keyword`(
	`idKeyword` int NOT NULL auto_increment,
  `keywordName` varchar(200) NOT NULL,  
	`rating` int NOT NULL,
	`autoRecord` bit(1) NOT NULL,
	`searchIn` int NOT NULL,
  PRIMARY KEY  (`idKeyword`)
) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
#

--
-- Definition of table `Timespan`
--

DROP TABLE IF EXISTS `Timespan`;
CREATE TABLE `Timespan`(
	`idTimespan` int NOT NULL auto_increment,
	`startTime` datetime NOT NULL,
	`endTime` datetime NOT NULL, 
	`dayOfWeek` int NOT NULL,
  PRIMARY KEY  (`idTimespan`)
) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
#

--
-- Definition of table `PersonalTVGuideMap`
--

DROP TABLE IF EXISTS `PersonalTVGuideMap`;
CREATE TABLE `PersonalTVGuideMap`(
	`idPersonalTVGuideMap` int NOT NULL auto_increment,
	`idKeyword` int NOT NULL,
  `idProgram` int NOT NULL,
  PRIMARY KEY  (`idPersonalTVGuideMap`)
) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
#

--
-- Definition of table `KeywordMap`
--

DROP TABLE IF EXISTS `KeywordMap`;
CREATE TABLE `KeywordMap`(
	`idKeywordMap` int NOT NULL auto_increment,
	`idKeyword` int NOT NULL,
  `idChannelGroup` int NOT NULL,
  PRIMARY KEY  (`idKeywordMap`)
) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
#


--
-- Definition of table `Version`
--

DROP TABLE IF EXISTS `Version`;
CREATE TABLE `Version` (
  `idVersion` int(11) NOT NULL auto_increment,
  `versionNumber` int(11) NOT NULL,
  PRIMARY KEY  (`idVersion`)
) ENGINE=MyISAM AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;
#
--
-- Dumping data for table `Version`
--

/*!40000 ALTER TABLE `Version` DISABLE KEYS */;
INSERT INTO `Version` VALUES  (1,30);
/*!40000 ALTER TABLE `Version` ENABLE KEYS */;
#



/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;


CREATE INDEX idProgramStart ON Program (startTime);
#
CREATE INDEX idxChannel ON Channel (isTv,sortOrder);
#
CREATE INDEX idProgramChannel ON Program (idChannel);
#
CREATE INDEX idProgramBeginEnd ON Program (idChannel,startTime,endTime);
#
