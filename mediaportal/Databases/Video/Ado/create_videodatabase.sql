DROP DATABASE IF EXISTS "videodatabase";
#

CREATE DATABASE "videodatabase";
#

use "videodatabase";
#

CREATE TABLE "actorinfomovies" (
  "idActor" int(11) DEFAULT NULL,
  "idDirector" int(11) DEFAULT NULL,
  "strPlotOutline" varchar(3000) DEFAULT NULL,
  "strPlot" varchar(3000) DEFAULT NULL,
  "strTagLine" varchar(250) DEFAULT NULL,
  "strVotes" varchar(250) DEFAULT NULL,
  "fRating" varchar(250) DEFAULT NULL,
  "strCast" varchar(3000) DEFAULT NULL,
  "strCredits" varchar(250) DEFAULT NULL,
  "iYear" int(11) DEFAULT NULL,
  "strGenre" varchar(250) DEFAULT NULL,
  "strPictureURL" varchar(250) DEFAULT NULL,
  "strTitle" varchar(250) DEFAULT NULL,
  "IMDBID" varchar(250) DEFAULT NULL,
  "mpaa" varchar(250) DEFAULT NULL,
  "runtime" int(11) DEFAULT NULL,
  "iswatched" int(11) DEFAULT NULL,
  "role" varchar(250) DEFAULT NULL,
  "actorinfomoviesID" int(11) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY ("actorinfomoviesID"),
  UNIQUE KEY "actorinfomoviesID" ("actorinfomoviesID"),
  KEY "idxactorinfomovies_idActor" ("idActor")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "actorlinkmovie" (
  "idActor" int(11) DEFAULT NULL,
  "idMovie" int(11) DEFAULT NULL,
  "strRole" varchar(250) DEFAULT NULL,
  "actorlinkmovieId" int(11) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY ("actorlinkmovieId"),
  UNIQUE KEY "actorlinkmovieId" ("actorlinkmovieId"),
  KEY "idxactorlinkmovie_idMovie" ("idMovie"),
  KEY "idxactorlinkmovie_idActor" ("idActor")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "actors" (
  "idActor" int(11) NOT NULL AUTO_INCREMENT,
  "strActor" varchar(250) DEFAULT NULL,
  "IMDBActorID" varchar(250) DEFAULT NULL,
  PRIMARY KEY ("idActor"),
  UNIQUE KEY "idActor" ("idActor"),
  KEY "idxactors_idIMDB" ("IMDBActorID"),
  KEY "idxactors_strActor" ("strActor")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "actorinfo" (
  "idActor" int(11) NOT NULL,
  "dateofbirth" varchar(250) DEFAULT NULL,
  "placeofbirth" varchar(250) DEFAULT NULL,
  "minibio" varchar(3000) DEFAULT NULL,
  "biography" varchar(3000) DEFAULT NULL,
  "thumbURL" varchar(250) DEFAULT NULL,
  "IMDBActorID" varchar(250) DEFAULT NULL,
  "dateofdeath" varchar(250) DEFAULT NULL,
  "placeofdeath" varchar(250) DEFAULT NULL,
  "lastupdate" timestamp NOT NULL DEFAULT '0000-00-00 00:00:00',
  PRIMARY KEY ("idActor")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "bookmark" (
  "idBookmark" int(11) NOT NULL AUTO_INCREMENT,
  "idFile" int(11) DEFAULT NULL,
  "fPercentage" varchar(250) DEFAULT NULL,
  PRIMARY KEY ("idBookmark"),
  UNIQUE KEY "idBookmark" ("idBookmark")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "duration" (
  "idDuration" int(11) NOT NULL AUTO_INCREMENT,
  "idFile" int(11) DEFAULT NULL,
  "duration" int(11) DEFAULT NULL,
  PRIMARY KEY ("idDuration"),
  UNIQUE KEY "idDuration" ("idDuration"),
  UNIQUE KEY "idxduration_idFile" ("idFile")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "files" (
  "idFile" int(11) NOT NULL AUTO_INCREMENT,
  "idPath" int(11) DEFAULT NULL,
  "idMovie" int(11) DEFAULT NULL,
  "strFilename" varchar(250) DEFAULT NULL,
  PRIMARY KEY ("idFile"),
  UNIQUE KEY "idFile" ("idFile"),
  KEY "idxfiles_idPath" ("idPath","strFilename"),
  KEY "idxfiles_idMovie" ("idMovie")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "filesmediainfo" (
  "idFile" int(11) NOT NULL,
  "videoCodec" varchar(250) DEFAULT NULL,
  "videoResolution" varchar(250) DEFAULT NULL,
  "aspectRatio" varchar(250) DEFAULT NULL,
  "hasSubtitles" tinyint(1) DEFAULT NULL,
  "audioCodec" varchar(250) DEFAULT NULL,
  "audioChannels" varchar(250) DEFAULT NULL,
  PRIMARY KEY ("idFile"),
  UNIQUE KEY "idxfilesmediainfo_idFile" ("idFile")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "genre" (
  "idGenre" int(11) NOT NULL AUTO_INCREMENT,
  "strGenre" varchar(250) DEFAULT NULL,
  PRIMARY KEY ("idGenre"),
  UNIQUE KEY "idGenre" ("idGenre")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "genrelinkmovie" (
  "idGenre" int(11) DEFAULT NULL,
  "idMovie" int(11) DEFAULT NULL,
  "genreinkmovieId" int(11) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY ("genreinkmovieId"),
  UNIQUE KEY "genreinkmovieId" ("genreinkmovieId"),
  KEY "idxgenrelinkmovie_idMovie" ("idMovie"),
  KEY "idxgenrelinkmovie_idGenre" ("idGenre")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "imdbmovies" (
  "idIMDB" varchar(250) DEFAULT NULL,
  "idTmdb" varchar(250) DEFAULT NULL,
  "strPlot" varchar(3000) DEFAULT NULL,
  "strCast" varchar(3000) DEFAULT NULL,
  "strCredits" varchar(250) DEFAULT NULL,
  "iYear" int(11) DEFAULT NULL,
  "strGenre" varchar(250) DEFAULT NULL,
  "strPictureURL" varchar(250) DEFAULT NULL,
  "strTitle" varchar(250) DEFAULT NULL,
  "mpaa" varchar(250) DEFAULT NULL,
  "imdbmoviesId" int(11) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY ("imdbmoviesId"),
  UNIQUE KEY "imdbmoviesId" ("imdbmoviesId"),
  UNIQUE KEY "idximdbmovies_idIMDB" ("idIMDB")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "movie" (
  "idMovie" int(11) NOT NULL AUTO_INCREMENT,
  "idPath" int(11) DEFAULT NULL,
  "hasSubtitles" int(11) DEFAULT NULL,
  "discid" varchar(250) DEFAULT NULL,
  "watched" tinyint(1) DEFAULT NULL,
  "timeswatched" int(11) DEFAULT NULL,
  "iduration" int(11) DEFAULT NULL,
  "iwatchedPercent" int(11) DEFAULT NULL,
  PRIMARY KEY ("idMovie"),
  UNIQUE KEY "idMovie" ("idMovie"),
  KEY "idxmovie_idPath" ("idPath")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "movieinfo" (
  "idMovie" int(11) NOT NULL,
  "idDirector" int(11) DEFAULT NULL,
  "strDirector" varchar(250) DEFAULT NULL,
  "strPlotOutline" varchar(3000) DEFAULT NULL,
  "strPlot" varchar(3000) DEFAULT NULL,
  "strTagLine" varchar(250) DEFAULT NULL,
  "strVotes" varchar(250) DEFAULT NULL,
  "fRating" varchar(250) DEFAULT NULL,
  "strCast" varchar(3000) DEFAULT NULL,
  "strCredits" varchar(250) DEFAULT NULL,
  "iYear" int(11) DEFAULT NULL,
  "strGenre" varchar(250) DEFAULT NULL,
  "strPictureURL" varchar(250) DEFAULT NULL,
  "strTitle" varchar(250) DEFAULT NULL,
  "IMDBID" varchar(250) DEFAULT NULL,
  "mpaa" varchar(250) DEFAULT NULL,
  "runtime" int(11) DEFAULT NULL,
  "iswatched" int(11) DEFAULT NULL,
  "strUserReview" varchar(3000) DEFAULT NULL,
  "strFanartURL" varchar(250) DEFAULT NULL,
  "dateAdded" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "dateWatched" timestamp NOT NULL DEFAULT '0000-00-00 00:00:00',
  "studios" varchar(250) DEFAULT NULL,
  "country" varchar(250) DEFAULT NULL,
  "language" varchar(250) DEFAULT NULL,
  "lastupdate" timestamp NOT NULL DEFAULT '0000-00-00 00:00:00',
  "strSortTitle" varchar(250) DEFAULT NULL,
  PRIMARY KEY ("idMovie"),
  UNIQUE KEY "idxmovieinfo_idMovie" ("idMovie"),
  KEY "idxmovieinfo_strTitle" ("strTitle"),
  KEY "idxmovieinfo_idIMDB" ("IMDBID"),
  KEY "idxmovieinfo_idDirector" ("idDirector"),
  KEY "idxmovieinfo_iYear" ("iYear")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "path" (
  "idPath" int(11) NOT NULL AUTO_INCREMENT,
  "strPath" varchar(250) DEFAULT NULL,
  "cdlabel" varchar(250) DEFAULT NULL,
  PRIMARY KEY ("idPath"),
  UNIQUE KEY "idPath" ("idPath"),
  KEY "idxpath_strPath" ("strPath"),
  KEY "idxpath_idPath" ("idPath")
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;
#

CREATE TABLE "resume" (
  "idResume" int(11) NOT NULL AUTO_INCREMENT,
  "idFile" int(11) DEFAULT NULL,
  "stoptime" int(11) DEFAULT NULL,
  "resumeData" blob,
  "bdtitle" int(11) DEFAULT NULL,
  PRIMARY KEY ("idResume"),
  UNIQUE KEY "idResume" ("idResume")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "usergroup" (
  "idGroup" int(11) NOT NULL AUTO_INCREMENT,
  "strGroup" varchar(250) DEFAULT NULL,
  "strRule" varchar(250) DEFAULT NULL,
  "strGroupDescription" varchar(250) DEFAULT NULL,
  PRIMARY KEY ("idGroup"),
  UNIQUE KEY "idGroup" ("idGroup"),
  KEY "idxuserGroup_strGroup" ("strGroup")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "usergrouplinkmovie" (
  "idGroup" int(11) DEFAULT NULL,
  "idMovie" int(11) DEFAULT NULL,
  "usergrouplinkmovieId" int(11) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY ("usergrouplinkmovieId"),
  UNIQUE KEY "usergrouplinkmovieId" ("usergrouplinkmovieId"),
  KEY "idxusergrouplinkmovie_idMovie" ("idMovie"),
  KEY "idxusergrouplinkmovie_idGroup" ("idGroup")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#

CREATE TABLE "videothumbblist" (
  "idVideoThumbBList" int(11) NOT NULL AUTO_INCREMENT,
  "strPath" varchar(250) DEFAULT NULL,
  "strExpires" datetime DEFAULT NULL,
  "strFileDate" datetime DEFAULT NULL,
  "strFileSize" varchar(250) DEFAULT NULL,
  PRIMARY KEY ("idVideoThumbBList"),
  UNIQUE KEY "idVideoThumbBList" ("idVideoThumbBList"),
  KEY "idxVideoThumbBList_strPath" ("strPath","strExpires"),
  KEY "idxVideoThumbBList_strExpires" ("strExpires")
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#