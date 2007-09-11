USE TvLibrary

--- version 32 ---
GO

ALTER TABLE TuningDetail ADD url varchar(200), bitrate int
GO

UPDATE TuningDetail SET url='',bitrate=0
GO

DELETE FROM Version
GO

insert into Version(versionNumber) values(32)
GO
