USE %TvLibrary%;

ALTER TABLE "Recording"
 ADD COLUMN "mediaType" int(1) NOT NULL;

UPDATE Version SET versionNumber=60;