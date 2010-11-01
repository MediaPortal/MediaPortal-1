USE %TvLibrary%;

ALTER TABLE "Recording"
  MODIFY COLUMN "fileName" VARCHAR(260) NOT NULL,
  ADD INDEX "IDX_Recording_Filename"("fileName");

ALTER TABLE "Program" 
  ADD INDEX "IDX_Program_Notify"("notify");

UPDATE "Version" SET "versionNumber"=45;