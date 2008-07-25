USE %TvLibrary%;

ALTER TABLE "Channel"
 ADD COLUMN "preload" bit(1) default NULL;

UPDATE "Version" SET "versionNumber"=39;
