USE %TvLibrary%;

ALTER TABLE Channel DROP COLUMN name;
ALTER TABLE Channel DROP COLUMN freetoair;


UPDATE Version SET versionNumber=57;
