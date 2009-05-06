USE %TvLibrary%;

ALTER TABLE "TuningDetail"
 ADD COLUMN "isVCRSignal" bit(1) NOT NULL;

UPDATE "Version" SET "versionNumber"=42;
