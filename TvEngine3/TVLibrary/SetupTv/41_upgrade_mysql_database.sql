USE %TvLibrary%;

ALTER TABLE "tuningdetail"
 ADD COLUMN "audioSource" int(1) NOT NULL;

UPDATE "Version" SET "versionNumber"=41;
