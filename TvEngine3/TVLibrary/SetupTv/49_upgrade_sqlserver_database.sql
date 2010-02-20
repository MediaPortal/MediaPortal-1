use %TvLibrary%
GO

ALTER TABLE Recording
  ADD isRecording BIT NOT NULL CONSTRAINT [DF_Recording_isRecording] DEFAULT 0,
      idSchedule INT NOT NULL CONSTRAINT [DF_Recording_idSchedule] DEFAULT 0         
GO

DROP INDEX IDX_Program_Notify ON Program
DROP INDEX IX_Program_EPG_Lookup ON Program 
GO

ALTER TABLE Program
  ADD state INT NOT NULL CONSTRAINT [DF_Program_state] DEFAULT 0 
GO

UPDATE Program SET state=1 WHERE notify=1 
GO

ALTER TABLE Program
  DROP COLUMN notify 
GO

ALTER TABLE Schedule
  ADD idParentSchedule int NOT NULL CONSTRAINT [DF_Schedule_idParentSchedule] DEFAULT 0   
GO

CREATE UNIQUE INDEX IX_Program_EPG_Lookup ON Program 
(
	idChannel ASC,
	startTime ASC,
	endTime ASC
)
INCLUDE
(
    title,
    description,
    seriesNum,
    episodeNum,
    genre,
    originalAirDate,
    classification,
    starRating,
    state,
    parentalRating
)
CREATE INDEX IDX_Program_State ON Program(state) 
GO

UPDATE Version SET versionNumber=49 
GO
