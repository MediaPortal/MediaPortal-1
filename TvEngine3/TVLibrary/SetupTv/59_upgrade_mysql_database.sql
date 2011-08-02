USE %TvLibrary%;

ALTER TABLE Card  MODIFY stopgraph DEFAULT 1;

UPDATE Version SET versionNumber = 59;