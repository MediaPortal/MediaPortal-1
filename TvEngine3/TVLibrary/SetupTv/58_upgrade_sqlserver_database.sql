USE %TvLibrary%
GO

ALTER TABLE SoftwareEncoder ADD COLUMN "reusable" bit NOT NULL DEFAULT 1
GO

INSERT INTO Setting (tag, value) VALUE ('softwareEncoderReuseLimit', '0')
GO

UPDATE Version SET versionNumber = 58
GO