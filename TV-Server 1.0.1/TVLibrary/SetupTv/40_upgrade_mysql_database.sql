USE %TvLibrary%;

ALTER TABLE "card"
 ADD COLUMN "CAM" bit(0) NOT NULL;

UPDATE "Version" SET "versionNumber"=40;
