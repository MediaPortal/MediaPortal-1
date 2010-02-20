USE %TvLibrary%;

ALTER TABLE "Schedule"
 ADD COLUMN "series" bit(1) NOT NULL;

UPDATE "Version" SET "versionNumber"=43;