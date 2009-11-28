USE %TvLibrary%;

ALTER TABLE "Card"
 ADD COLUMN "NetProvider" TINYINT DEFAULT 0 NOT NULL;

UPDATE Version SET versionNumber=50;
