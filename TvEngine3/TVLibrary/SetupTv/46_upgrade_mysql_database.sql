USE %TvLibrary%;

ALTER TABLE "Recording"
  ADD COLUMN "episodeName" text NOT NULL;

UPDATE "Version" SET "versionNumber"=46;