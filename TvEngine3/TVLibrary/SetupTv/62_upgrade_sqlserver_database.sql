USE %TvLibrary%;
GO

INSERT INTO setting (tag, `value`) values ("hostName", (SELECT TOP 1 hostName FROM server))
GO

INSERT INTO setting (tag, `value`) values ("rtspPort", (SELECT rtspPort FROM server LIMIT 1))
GO

ALTER TABLE recording DROP COLUMN idServer
GO

ALTER TABLE card DROP COLUMN idServer
GO

DROP TABLE server;

UPDATE Version SET versionNumber=62
GO