DROP DATABASE IF EXISTS "foldersetting";
#

CREATE DATABASE "foldersetting";
#

use "foldersetting";
#

CREATE TABLE "tblpath" (
  "idPath" int(11) NOT NULL AUTO_INCREMENT,
  "strPath" varchar(250) DEFAULT NULL,
  PRIMARY KEY ("idPath"),
  UNIQUE KEY "idPath" ("idPath")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "tblsetting" (
  "idSetting" int(11) NOT NULL AUTO_INCREMENT,
  "idPath" int(11) DEFAULT NULL,
  "tagName" varchar(250) DEFAULT NULL,
  "tagValue" varchar(1000) DEFAULT NULL,
  PRIMARY KEY ("idSetting"),
  UNIQUE KEY "idSetting" ("idSetting")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#