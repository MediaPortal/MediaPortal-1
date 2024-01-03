USE %TvLibrary%;

ALTER TABLE "Card"
 ADD COLUMN "symbolRateMultiplier" INT NOT NULL DEFAULT 0;

UPDATE Version SET versionNumber=62;
