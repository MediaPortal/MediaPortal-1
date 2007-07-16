USE TvLibrary;
# --- version 28 ---

CREATE TABLE `Keyword`(
	`idKeyword` int NOT NULL auto_increment,
  `keywordName` varchar(200) NOT NULL,  
	`rating` int NOT NULL,
	`autoRecord` bit(1) NOT NULL,
	`searchIn` int NOT NULL,
  PRIMARY KEY  (`idKeyword`)
) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
#

CREATE TABLE `Timespan`(
	`idTimespan` int NOT NULL auto_increment,
	`startTime` datetime NOT NULL,
	`endTime` datetime NOT NULL, 
	`dayOfWeek` int NOT NULL,
  PRIMARY KEY  (`idTimespan`)
) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
#

CREATE TABLE `PersonalTVGuideMap`(
	`idPersonalTVGuideMap` int NOT NULL auto_increment,
	`idKeyword` int NOT NULL,
  `idProgram` int NOT NULL,
  PRIMARY KEY  (`idPersonalTVGuideMap`)
) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
#

CREATE TABLE `KeywordMap`(
	`idKeywordMap` int NOT NULL auto_increment,
	`idKeyword` int NOT NULL,
  `idChannelGroup` int NOT NULL,
  PRIMARY KEY  (`idKeywordMap`)
) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
#


CREATE TABLE `channellinkagemap` (
  `idMapping` int(11) NOT NULL auto_increment,
  `idPortalChannel` int(11) NOT NULL,
  `idLinkedChannel` int(11) NOT NULL,
  PRIMARY KEY (`idMapping`)
) ENGINE=MyISAM AUTO_INCREMENT=68 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
#  
UPDATE `version` SET `versionNumber`=29;
#
