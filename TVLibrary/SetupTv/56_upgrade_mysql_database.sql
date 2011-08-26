USE %TvLibrary%;

ALTER TABLE "TuningDetail" DROP COLUMN "pcrPid"; 
ALTER TABLE "TuningDetail" DROP COLUMN "videoPid"; 
ALTER TABLE "TuningDetail" DROP COLUMN "audioPid"; 

UPDATE Version SET versionNumber=56;
