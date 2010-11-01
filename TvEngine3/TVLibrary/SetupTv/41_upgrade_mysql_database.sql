USE %TvLibrary%;

ALTER TABLE "TuningDetail"
 ADD COLUMN "audioSource" int(1) NOT NULL;

UPDATE "Version" SET "versionNumber"=41;
