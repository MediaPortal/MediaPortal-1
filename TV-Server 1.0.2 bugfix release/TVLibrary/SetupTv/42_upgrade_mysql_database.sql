USE %TvLibrary%;

ALTER TABLE "tuningdetail"
 ADD COLUMN "isVCRSignal" bit(1) NOT NULL;

UPDATE "Version" SET "versionNumber"=42;
