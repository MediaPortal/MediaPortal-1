DROP DATABASE IF EXISTS "picturedatabase";
#

CREATE DATABASE "picturedatabase";
#

use "picturedatabase";
#

CREATE TABLE `picture` (
  `idPicture` int(11) NOT NULL AUTO_INCREMENT,
  `strFile`  varchar(250) DEFAULT NULL,
  `iRotation` int(11) DEFAULT NULL,
  `strDateTaken`  varchar(250) DEFAULT NULL,
  PRIMARY KEY (`idPicture`),
  UNIQUE KEY `idPicture` (`idPicture`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;




