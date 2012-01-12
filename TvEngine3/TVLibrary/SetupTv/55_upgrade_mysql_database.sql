USE %TvLibrary%;

CREATE TABLE "PendingDeletion" (
  "idPendingDeletion" int(11) NOT NULL auto_increment,  
  "fileName" varchar(260) NOT NULL, 
  PRIMARY KEY  ("idPendingDeletion")
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;


UPDATE Version SET versionNumber=55;
