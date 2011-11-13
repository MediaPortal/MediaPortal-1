USE %TvLibrary%;

INSERT INTO setting (tag, `value`) values ("hostName", (SELECT hostName FROM server LIMIT 1));
INSERT INTO setting (tag, `value`) values ("rtspPort", (SELECT rtspPort FROM server LIMIT 1));

ALTER TABLE recording DROP COLUMN idServer, DROP INDEX FK_Recording_Server;

ALTER TABLE card DROP COLUMN idServer, DROP INDEX FK_Card_Server;

DROP TABLE server;

UPDATE Version SET versionNumber=59;
