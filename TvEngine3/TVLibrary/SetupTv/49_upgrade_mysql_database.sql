USE %TvLibrary%;

ALTER TABLE Recording ADD COLUMN isRecording bit DEFAULT 0 NOT NULL;
ALTER TABLE Recording ADD COLUMN idSchedule int DEFAULT 0 NOT NULL;

ALTER TABLE Program ADD COLUMN state int DEFAULT 0 NOT NULL, DROP COLUMN notify;
ALTER TABLE Program ADD INDEX IDX_Program_State(state);


ALTER TABLE Schedule ADD COLUMN idParentSchedule int DEFAULT 0 NOT NULL;

UPDATE Version SET versionNumber=49;
