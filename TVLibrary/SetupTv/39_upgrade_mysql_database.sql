USE %TvLibrary%;

ALTER TABLE "Card"
 ADD COLUMN "preload" bit(1) NOT NULL;

UPDATE "Version" SET "versionNumber"=39;
