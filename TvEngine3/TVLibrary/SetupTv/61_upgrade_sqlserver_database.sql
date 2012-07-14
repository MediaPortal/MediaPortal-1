USE %TvLibrary%;
GO

ALTER TABLE channel ADD channelNumber INT NOT NULL CONSTRAINT [DF_Channel_ChannelNumber] DEFAULT 0
UPDATE channel c SET c.channelnumber = (SELECT TOP 1 t.channelNumber FROM tuningdetail t WHERE t.idChannel = c.idChannel ORDER BY t.idTuning)

UPDATE Version SET versionNumber=61
GO
USE %TvLibrary%;
GO

ALTER TABLE channel ADD channelNumber INT NOT NULL CONSTRAINT [DF_Channel_ChannelNumber] DEFAULT 0
UPDATE channel c SET c.channelnumber = (SELECT TOP 1 t.channelNumber FROM tuningdetail t WHERE t.idChannel = c.idChannel ORDER BY t.idTuning)

UPDATE Version SET versionNumber=61
GO
