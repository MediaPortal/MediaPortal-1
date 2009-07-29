USE %TvLibrary%;

ALTER TABLE "Recording"
  ADD COLUMN "seriesNum" VARCHAR(200) NOT NULL
  ADD COLUMN "episodeNum" VARCHAR(200) NOT NULL
  ADD COLUMN "episodePart" TEXT NOT NULL;

UPDATE "Version" SET "versionNumber"=47;
