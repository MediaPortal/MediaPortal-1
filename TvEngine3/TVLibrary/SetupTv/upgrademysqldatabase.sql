USE TvLibrary;
#
CREATE TABLE `channellinkagemap` (
  `idMapping` int(11) NOT NULL auto_increment,
  `idPortalChannel` int(11) NOT NULL,
  `idLinkedChannel` int(11) NOT NULL,
  PRIMARY KEY (`idMapping`)
) ENGINE=MyISAM AUTO_INCREMENT=68 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
#  
UPDATE `version` SET `versionNumber`=26;
#
