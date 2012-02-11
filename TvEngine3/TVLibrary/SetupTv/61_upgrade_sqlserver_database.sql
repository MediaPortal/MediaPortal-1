use %TvLibrary%
GO
 
ALTER TABLE program
	ADD seriesId INT
GO
 
ALTER TABLE program
	ADD seriesTermination TINYINT
GO
 
ALTER TABLE schedule
	ADD seriesId INT
GO

UPDATE Version SET versionNumber=61
GO
