USE %TvLibrary%;

ALTER TABLE Card  MODIFY stopgraph bit(1) DEFAULT 1;

UPDATE Version SET versionNumber = 59;