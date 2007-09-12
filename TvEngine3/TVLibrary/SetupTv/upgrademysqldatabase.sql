USE TvLibrary;
# 
--- version 33 ---
CREATE TABLE `RadioChannelGroup` (
  `idGroup` int(11) NOT NULL auto_increment,
  `groupName` varchar(200) NOT NULL,
  `sortOrder` int(11) NOT NULL,
  PRIMARY KEY  (`idGroup`),
  KEY `IDX_RadioChannelGroup` (`sortOrder`)
) ENGINE=MyISAM AUTO_INCREMENT=3 DEFAULT CHARSET=latin1;
#
CREATE TABLE `RadioGroupMap` (
  `idMap` int(11) NOT NULL auto_increment,
  `idGroup` int(11) NOT NULL,
  `idChannel` int(11) NOT NULL,
  `SortOrder` int(11) NOT NULL,
  PRIMARY KEY  (`idMap`),
  KEY `FK_RadioGroupMap_Channel` (`idChannel`),
  KEY `FK_RadioGroupMap_ChannelGroup` (`idGroup`)
) ENGINE=MyISAM AUTO_INCREMENT=145 DEFAULT CHARSET=latin1 ROW_FORMAT=FIXED;
#
DELETE FROM `Version`
#
INSERT INTO `Version` (`versionNumber`) VALUES(33)
#
