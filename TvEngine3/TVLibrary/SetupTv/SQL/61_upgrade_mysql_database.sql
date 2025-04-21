USE %TvLibrary%;

ALTER TABLE Channel ADD COLUMN channelNumber INT(11) NULL DEFAULT NULL;
UPDATE Channel c SET c.channelNumber = (SELECT t.channelNumber FROM TuningDetail t WHERE t.idChannel = c.idChannel ORDER BY t.idTuning LIMIT 1);
UPDATE Channel SET channelNumber = 10000 WHERE ISNULL(channelNumber);
ALTER TABLE Channel CHANGE channelNumber channelNumber INT(11) NOT NULL DEFAULT 10000;

UPDATE Version SET versionNumber=61;
