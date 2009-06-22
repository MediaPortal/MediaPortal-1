USE %TvLibrary%;

ALTER TABLE "Recording"
 MODIFY COLUMN "fileName" VARCHAR(260) NOT NULL,
 ADD INDEX "IDX_Recording_Filename"("fileName");

UPDATE "Version" SET "versionNumber"=45;