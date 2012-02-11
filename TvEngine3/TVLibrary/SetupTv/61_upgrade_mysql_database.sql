USE %TvLibrary%;

ALTER TABLE "program"
	ADD COLUMN "seriesId" INT;
	
ALTER TABLE "program"
	ADD COLUMN "seriesTermination" TINYINT;

ALTER TABLE "schedule"
	ADD COLUMN "seriesId" INT;
	
UPDATE Version SET versionNumber=61;
