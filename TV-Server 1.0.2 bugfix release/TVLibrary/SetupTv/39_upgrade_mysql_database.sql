USE %TvLibrary%;

ALTER TABLE "card"
 ADD COLUMN "preload" bit(1) NOT NULL;

UPDATE "Version" SET "versionNumber"=39;
