USE %TvLibrary%;

ALTER TABLE "Recording" ADD COLUMN "isRecording" BIT NOT NULL DEFAULT 0; 
ALTER TABLE "Recording" ADD COLUMN "idSchedule" INT NOT NULL DEFAULT 0; 

ALTER TABLE "Program" ADD COLUMN "state" INT NOT NULL DEFAULT 0; 
UPDATE "Program" SET state=1 WHERE notify=1; 
ALTER TABLE "Program" DROP COLUMN "notify"; 
ALTER TABLE "Program" ADD INDEX IDX_Program_State(state); 

ALTER TABLE "Schedule" ADD COLUMN "idParentSchedule" INT NOT NULL DEFAULT 0; 

UPDATE Version SET versionNumber=49;
