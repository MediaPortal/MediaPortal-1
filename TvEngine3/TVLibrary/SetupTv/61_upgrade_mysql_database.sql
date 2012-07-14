USE %TvLibrary%;

ALTER TABLE channel ADD COLUMN "channelNumber" int(11) NOT NULL DEFAULT 0;
UPDATE channel c set c.channelnumber = (select t.channelNumber from tuningdetail t where t.idChannel = c.idChannel order by t.idTuning limit 1);

UPDATE Version SET versionNumber=61;
USE %TvLibrary%;

ALTER TABLE channel ADD COLUMN "channelNumber" int(11) NOT NULL DEFAULT 0;
UPDATE channel c set c.channelnumber = (select t.channelNumber from tuningdetail t where t.idChannel = c.idChannel order by t.idTuning limit 1);

UPDATE Version SET versionNumber=61;
