USE %TvLibrary%;

ALTER TABLE "Channel" ADD COLUMN "channelNumber" int(11) NOT NULL DEFAULT 10000;
UPDATE Channel c set c.channelNumber = (SELECT t.channelNumber FROM TuningDetail t WHERE t.idChannel = c.idChannel ORDER BY t.idTuning limit 1);

UPDATE Version SET versionNumber=61;
