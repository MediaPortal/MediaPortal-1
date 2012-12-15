USE %TvLibrary%;

ALTER TABLE "program"

ADD COLUMN "seriesId" VARCHAR(200);

ALTER TABLE "program"

ADD COLUMN "seriesTermination" TINYINT;

ALTER TABLE "schedule"

ADD COLUMN "seriesId" VARCHAR(200);

UPDATE Version SET versionNumber=62;