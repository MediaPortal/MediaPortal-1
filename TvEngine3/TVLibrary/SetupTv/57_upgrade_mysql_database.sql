USE %TvLibrary%;

ALTER TABLE channel DROP COLUMN name;
ALTER TABLE channel DROP COLUMN freetoair;


UPDATE Version SET versionNumber=57;
