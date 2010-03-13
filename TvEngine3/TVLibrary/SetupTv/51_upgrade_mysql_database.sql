USE %TvLibrary%;

ALTER TABLE "CanceledSchedule"
 ADD COLUMN "idChannel" INT NOT NULL DEFAULT 0;

UPDATE Version SET versionNumber=51;