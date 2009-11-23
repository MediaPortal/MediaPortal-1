use %TvLibrary%
GO

ALTER TABLE Recording
  ADD isRecording bit NOT NULL DEFAULT 0,
      idSchedule int NOT NULL DEFAULT 0      
        
GO

DROP INDEX IDX_Program_Notify ON Program
DROP INDEX IX_Program_EPG_Lookup ON Program 

GO

ALTER TABLE Program
  ADD state int NOT NULL DEFAULT 0

ALTER TABLE Program
  DROP COLUMN notify

ALTER TABLE Schedule
  ADD idParentSchedule int NOT NULL DEFAULT 0
  
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
