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
-- Create schema tvlibrary
--

CREATE DATABASE IF NOT EXISTS tvlibrary;
USE tvlibrary;
#
--
-- Definition of table `canceledschedule`
--

DROP TABLE IF EXISTS `canceledschedule`;
CREATE TABLE `canceledschedule` (
  `idCanceledSchedule` int(11) NOT NULL auto_increment,
  `idSchedule` int(11) NOT NULL,
  `cancelDateTime` datetime NOT NULL,
  PRIMARY KEY  (`idCanceledSchedule`),
  KEY `FK_CanceledSchedule_Schedule` (`idSchedule`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#
--
-- Dumping data for table `canceledschedule`
--

/*!40000 ALTER TABLE `canceledschedule` DISABLE KEYS */;
/*!40000 ALTER TABLE `canceledschedule` ENABLE KEYS */;


--
-- Definition of table `card`
--

DROP TABLE IF EXISTS `card`;
CREATE TABLE `card` (
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
-- Definition of table `cardgroup`
--

DROP TABLE IF EXISTS `cardgroup`;
CREATE TABLE `cardgroup` (
  `idCardGroup` int(11) NOT NULL auto_increment,
  `name` varchar(255) NOT NULL,
  PRIMARY KEY  (`idCardGroup`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `cardgroupmap`
--

DROP TABLE IF EXISTS `cardgroupmap`;
CREATE TABLE `cardgroupmap` (
  `idMapping` int(11) NOT NULL auto_increment,
  `idCard` int(11) NOT NULL,
  `idCardGroup` int(11) NOT NULL,
  PRIMARY KEY  (`idMapping`),
  KEY `FK_CardGroupMap_Card` (`idCard`),
  KEY `FK_CardGroupMap_CardGroup` (`idCardGroup`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `channel`
--

DROP TABLE IF EXISTS `channel`;
CREATE TABLE `channel` (
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
  PRIMARY KEY  (`idChannel`),
  KEY `IDX_Channel1` (`isTv`,`sortOrder`)
) ENGINE=MyISAM AUTO_INCREMENT=383 DEFAULT CHARSET=latin1;
#

--
-- Definition of table `channelgroup`
--

DROP TABLE IF EXISTS `channelgroup`;
CREATE TABLE `channelgroup` (
  `idGroup` int(11) NOT NULL auto_increment,
  `groupName` varchar(200) NOT NULL,
  PRIMARY KEY  (`idGroup`)
) ENGINE=MyISAM AUTO_INCREMENT=3 DEFAULT CHARSET=latin1;
#

--
-- Definition of table `channelmap`
--

DROP TABLE IF EXISTS `channelmap`;
CREATE TABLE `channelmap` (
  `idChannelMap` int(11) NOT NULL auto_increment,
  `idChannel` int(11) NOT NULL,
  `idCard` int(11) NOT NULL,
  PRIMARY KEY  (`idChannelMap`),
  KEY `FK_ChannelMap_Cards` (`idCard`),
  KEY `FK_ChannelMap_Channels` (`idChannel`)
) ENGINE=MyISAM AUTO_INCREMENT=289 DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `conflict`
--

DROP TABLE IF EXISTS `conflict`;
CREATE TABLE `conflict` (
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
-- Definition of table `diseqcmotor`
--

DROP TABLE IF EXISTS `diseqcmotor`;
CREATE TABLE `diseqcmotor` (
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
-- Definition of table `favorite`
--

DROP TABLE IF EXISTS `favorite`;
CREATE TABLE `favorite` (
  `idFavorite` int(11) NOT NULL auto_increment,
  `idProgram` int(11) NOT NULL,
  `priority` int(11) NOT NULL,
  `timesWatched` int(11) NOT NULL,
  PRIMARY KEY  (`idFavorite`),
  KEY `FK_Favorites_Programs` (`idProgram`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `groupmap`
--

DROP TABLE IF EXISTS `groupmap`;
CREATE TABLE `groupmap` (
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
-- Definition of table `program`
--

DROP TABLE IF EXISTS `program`;
CREATE TABLE `program` (
  `idProgram` int(11) NOT NULL auto_increment,
  `idChannel` int(11) NOT NULL,
  `startTime` datetime NOT NULL,
  `endTime` datetime NOT NULL,
  `title` varchar(2000) NOT NULL,
  `description` varchar(4000) NOT NULL,
  `genre` varchar(200) NOT NULL,
  `notify` bit(1) NOT NULL,
  PRIMARY KEY  (`idProgram`),
  KEY `IDX_StartTime` (`startTime`),
  KEY `IDX_Program1` (`idChannel`),
  KEY `IDX_Program2` (`idChannel`,`startTime`,`endTime`)
) ENGINE=MyISAM AUTO_INCREMENT=26096 DEFAULT CHARSET=latin1;
#

--
-- Definition of table `recording`
--

DROP TABLE IF EXISTS `recording`;
CREATE TABLE `recording` (
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
  PRIMARY KEY  (`idRecording`),
  KEY `FK_Recording_Server` (`idServer`),
  KEY `FK_Recordings_Channels` (`idChannel`)
) ENGINE=MyISAM AUTO_INCREMENT=43 DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `satellite`
--

DROP TABLE IF EXISTS `satellite`;
CREATE TABLE `satellite` (
  `idSatellite` int(11) NOT NULL auto_increment,
  `satelliteName` varchar(200) NOT NULL,
  `transponderFileName` varchar(200) NOT NULL,
  PRIMARY KEY  (`idSatellite`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `schedule`
--

DROP TABLE IF EXISTS `schedule`;
CREATE TABLE `schedule` (
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
-- Definition of table `server`
--

DROP TABLE IF EXISTS `server`;
CREATE TABLE `server` (
  `idServer` int(11) NOT NULL auto_increment,
  `isMaster` bit(1) NOT NULL,
  `hostName` varchar(256) NOT NULL,
  PRIMARY KEY  (`idServer`)
) ENGINE=MyISAM AUTO_INCREMENT=3 DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#

--
-- Definition of table `setting`
--

DROP TABLE IF EXISTS `setting`;
CREATE TABLE `setting` (
  `idSetting` int(11) NOT NULL auto_increment,
  `tag` varchar(200) NOT NULL,
  `value` varchar(4096) NOT NULL,
  PRIMARY KEY  (`idSetting`)
) ENGINE=MyISAM AUTO_INCREMENT=68 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
#

--
-- Definition of table `tuningdetail`
--

DROP TABLE IF EXISTS `tuningdetail`;
CREATE TABLE `tuningdetail` (
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
-- Definition of table `version`
--

DROP TABLE IF EXISTS `version`;
CREATE TABLE `version` (
  `idVersion` int(11) NOT NULL auto_increment,
  `versionNumber` int(11) NOT NULL,
  PRIMARY KEY  (`idVersion`)
) ENGINE=MyISAM AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;
#
--
-- Dumping data for table `version`
--

/*!40000 ALTER TABLE `version` DISABLE KEYS */;
INSERT INTO `version` VALUES  (1,22);
/*!40000 ALTER TABLE `version` ENABLE KEYS */;
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
