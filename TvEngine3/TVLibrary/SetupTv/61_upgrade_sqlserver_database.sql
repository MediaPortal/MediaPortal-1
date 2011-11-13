USE %TvLibrary%;
GO

INSERT INTO setting (tag, `value`) values ("hostName", (SELECT TOP 1 hostName FROM server))

UPDATE Version SET versionNumber=59
GO