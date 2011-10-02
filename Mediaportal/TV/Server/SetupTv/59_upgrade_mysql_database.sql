USE %TvLibrary%;

ALTER TABLE "Recording"
 ADD COLUMN "mediaType" int(1) NOT NULL;

UPDATE Version SET versionNumber=59;
USE %TvLibrary%;

ALTER TABLE Card  MODIFY stopgraph bit(1) NOT NULL DEFAULT 1;

UPDATE Version SET versionNumber = 59;