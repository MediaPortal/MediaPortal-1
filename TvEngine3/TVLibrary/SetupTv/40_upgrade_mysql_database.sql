USE %TvLibrary%;

ALTER TABLE "Card"
 ADD COLUMN "CAM" bit(0) NOT NULL;

UPDATE "Version" SET "versionNumber"=40;
